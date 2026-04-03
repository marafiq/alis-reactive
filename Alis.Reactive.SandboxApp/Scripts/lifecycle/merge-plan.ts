import type { Plan, Workflow } from "../types";
import { cloneContracts, mergeContractMaps, pruneUnreferencedContracts } from "./contract-map";
import { cloneBindings, cloneObjects, mergeBindingMaps, mergeObjectMaps } from "./object-map";

type WireWorkflows = (workflows: Workflow[], plan: Plan, signal?: AbortSignal) => void;
type UnwireFields = (fieldIds: string[]) => void;

export interface MergeHooks {
  wireWorkflows: WireWorkflows;
  unwireFields: UnwireFields;
}

export class PlanRegistry {
  private readonly plans = new Map<string, Plan>();
  private readonly rootPlanIds = new Set<string>();
  private readonly rootContracts = new Map<string, Plan["contracts"]>();
  private readonly rootObjects = new Map<string, Plan["objects"]>();
  private readonly rootBindings = new Map<string, Plan["bindings"]>();
  private readonly sourceOwners = new Map<string, string>();
  private readonly abortControllers = new Map<string, AbortController>();
  private readonly sourceWorkflowRefs = new Map<string, Workflow[]>();
  private readonly sourceContracts = new Map<string, Plan["contracts"]>();
  private readonly sourceObjects = new Map<string, Plan["objects"]>();
  private readonly sourceBindings = new Map<string, Plan["bindings"]>();

  register(plan: Plan): void {
    this.plans.set(plan.planId, plan);
    this.rootPlanIds.add(plan.planId);
    this.rootContracts.set(plan.planId, cloneContracts(plan.contracts));
    this.rootObjects.set(plan.planId, cloneObjects(plan.objects));
    this.rootBindings.set(plan.planId, cloneBindings(plan.bindings));
  }

  add(incoming: Plan, hooks: MergeHooks): Plan {
    const sourceId = incoming.sourceId;
    const previousPlanId = sourceId ? this.sourceOwners.get(sourceId) : undefined;

    if (sourceId && previousPlanId) {
      this.removeSource(previousPlanId, sourceId, hooks.unwireFields);
    }

    let target = this.plans.get(incoming.planId);
    if (!target) {
      target = {
        version: 2,
        planId: incoming.planId,
        contracts: {},
        objects: {},
        bindings: {},
        workflows: [],
      };
      this.plans.set(incoming.planId, target);
    }

    mergeContractMaps(target.contracts, incoming.contracts);
    mergeObjectMaps(target.contracts, target.objects, incoming.contracts, incoming.objects);
    mergeBindingMaps(target.bindings, incoming.bindings);
    pruneUnreferencedContracts(target.contracts, target.objects);

    const abort = sourceId ? new AbortController() : undefined;
    hooks.wireWorkflows(incoming.workflows, target, abort?.signal);
    target.workflows.push(...incoming.workflows);

    if (sourceId && abort) {
      this.sourceOwners.set(sourceId, incoming.planId);
      this.abortControllers.set(sourceId, abort);
      this.sourceWorkflowRefs.set(sourceId, [...incoming.workflows]);
      this.sourceContracts.set(sourceId, cloneContracts(incoming.contracts));
      this.sourceObjects.set(sourceId, cloneObjects(incoming.objects));
      this.sourceBindings.set(sourceId, cloneBindings(incoming.bindings));
    }

    return target;
  }

  get(planId: string): Plan | undefined {
    return this.plans.get(planId);
  }

  reset(): void {
    this.plans.clear();
    this.rootPlanIds.clear();
    this.rootContracts.clear();
    this.rootObjects.clear();
    this.rootBindings.clear();
    this.sourceOwners.clear();
    for (const abort of this.abortControllers.values()) abort.abort();
    this.abortControllers.clear();
    this.sourceWorkflowRefs.clear();
    this.sourceContracts.clear();
    this.sourceObjects.clear();
    this.sourceBindings.clear();
  }

  private removeSource(planId: string, sourceId: string, unwireFields: UnwireFields): void {
    const plan = this.plans.get(planId);
    if (!plan) {
      this.clearTracking(sourceId);
      return;
    }

    this.abortControllers.get(sourceId)?.abort();

    const workflows = this.sourceWorkflowRefs.get(sourceId);
    if (workflows) {
      for (const workflow of workflows) {
        const index = plan.workflows.indexOf(workflow);
        if (index >= 0) plan.workflows.splice(index, 1);
      }
    }

    const rebuilt = this.rebuildState(planId, sourceId);
    const removedFieldIds = Object.entries(plan.objects)
      .filter(([key]) => !(key in rebuilt.objects))
      .map(([, objectRef]) => objectRef.elementId)
      .filter((id): id is string => !!id);

    if (removedFieldIds.length > 0) {
      unwireFields(removedFieldIds);
    }

    plan.contracts = rebuilt.contracts;
    plan.objects = rebuilt.objects;
    plan.bindings = rebuilt.bindings;

    this.clearTracking(sourceId);

    if (!this.rootPlanIds.has(planId)
      && plan.workflows.length === 0
      && Object.keys(plan.objects).length === 0
      && Object.keys(plan.bindings).length === 0) {
      this.plans.delete(planId);
    }
  }

  private clearTracking(sourceId: string): void {
    this.sourceOwners.delete(sourceId);
    this.abortControllers.delete(sourceId);
    this.sourceWorkflowRefs.delete(sourceId);
    this.sourceContracts.delete(sourceId);
    this.sourceObjects.delete(sourceId);
    this.sourceBindings.delete(sourceId);
  }

  private rebuildState(planId: string, sourceIdBeingRemoved: string): Pick<Plan, "contracts" | "objects" | "bindings"> {
    const rebuilt = {
      contracts: cloneContracts(this.rootContracts.get(planId) ?? {}),
      objects: cloneObjects(this.rootObjects.get(planId) ?? {}),
      bindings: cloneBindings(this.rootBindings.get(planId) ?? {}),
    };

    for (const [sourceId, ownerPlanId] of this.sourceOwners.entries()) {
      if (sourceId === sourceIdBeingRemoved || ownerPlanId !== planId) {
        continue;
      }

      const contracts = this.sourceContracts.get(sourceId);
      const objects = this.sourceObjects.get(sourceId);
      const bindings = this.sourceBindings.get(sourceId);

      if (contracts) {
        mergeContractMaps(rebuilt.contracts, contracts);
      }

      if (contracts && objects) {
        mergeObjectMaps(rebuilt.contracts, rebuilt.objects, contracts, objects);
      }

      if (bindings) {
        mergeBindingMaps(rebuilt.bindings, bindings);
      }
    }

    pruneUnreferencedContracts(rebuilt.contracts, rebuilt.objects);
    return rebuilt;
  }
}

const registry = new PlanRegistry();

export function composeInitialPlans(plans: Plan[]): Plan[] {
  const byPlanId = new Map<string, Plan>();

  for (const plan of plans) {
    const existing = byPlanId.get(plan.planId);
    if (!existing) {
      byPlanId.set(plan.planId, {
        version: 2,
        planId: plan.planId,
        contracts: cloneContracts(plan.contracts),
        objects: cloneObjects(plan.objects),
        bindings: cloneBindings(plan.bindings),
        workflows: [...plan.workflows],
      });
      continue;
    }

    mergeContractMaps(existing.contracts, plan.contracts);
    mergeObjectMaps(existing.contracts, existing.objects, plan.contracts, plan.objects);
    mergeBindingMaps(existing.bindings, plan.bindings);
    pruneUnreferencedContracts(existing.contracts, existing.objects);
    existing.workflows.push(...plan.workflows);
  }

  return Array.from(byPlanId.values());
}

export function registerBootedPlan(plan: Plan): void { registry.register(plan); }
export function applyMergedPlan(incoming: Plan, hooks: MergeHooks): Plan { return registry.add(incoming, hooks); }
export function getBootedPlan(planId: string): Plan | undefined { return registry.get(planId); }
export function resetMergePlanState(): void { registry.reset(); }
