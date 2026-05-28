// browser-plans.ts - boot plan composition and partial slot lifetimes.

import type { PlanDocument, Behavior } from "../types";
import {
  mergeBootComponent,
  mergeSlotComponent,
} from "./component-merge";
import { mergeObjectContracts } from "./object-contracts";

type PlanId = string;
type PartialSlotId = string;

type WireBehaviors = (behaviors: Behavior[], plan: PlanDocument, signal?: AbortSignal) => void;
type WireContainerValidation = (plan: PlanDocument, signal?: AbortSignal) => void;

export interface BrowserPlanWiring {
  wireBehaviors: WireBehaviors;
  wireContainerValidation: WireContainerValidation;
}

interface PartialSlot {
  readonly abortController: AbortController;
  readonly plans: PlanDocument[];
}

export class AppliedBrowserPlans {
  private readonly plans = new Map<PlanId, PlanDocument>();
  private readonly bootPlans = new Map<PlanId, PlanDocument>();
  private readonly slots = new Map<PartialSlotId, PartialSlot>();

  register(plan: PlanDocument): void {
    this.plans.set(plan.planId, plan);
    this.bootPlans.set(plan.planId, snapshotPlan(plan));
  }

  loadPartialSlot(slotId: PartialSlotId, plans: PlanDocument[], wiring: BrowserPlanWiring): PlanId[] {
    const affectedPlanIds = new Set(this.removeSlot(slotId));

    const abortController = new AbortController();
    const slotPlans = plans.map(snapshotPlan);
    this.slots.set(slotId, { abortController, plans: slotPlans });

    for (const plan of slotPlans) {
      affectedPlanIds.add(plan.planId);
    }

    this.recomposePlans(affectedPlanIds);
    for (const plan of slotPlans) {
      const activePlan = this.plans.get(plan.planId)!;
      wiring.wireBehaviors(plan.behaviors, activePlan, abortController.signal);
      wiring.wireContainerValidation(activePlan, abortController.signal);
    }

    return [...affectedPlanIds];
  }

  unloadPartialSlot(slotId: PartialSlotId): PlanId[] {
    const affectedPlanIds = new Set(this.removeSlot(slotId));
    this.recomposePlans(affectedPlanIds);
    return [...affectedPlanIds];
  }

  get(planId: string): PlanDocument | undefined {
    return this.plans.get(planId);
  }

  reset(): void {
    this.abortSlots();
    this.plans.clear();
    this.bootPlans.clear();
    this.slots.clear();
  }

  private removeSlot(slotId: PartialSlotId): PlanId[] {
    const slot = this.slots.get(slotId);
    if (slot === undefined) return [];

    this.slots.delete(slotId);
    slot.abortController.abort();
    return planIdsIn(slot.plans);
  }

  private recomposePlans(planIds: Iterable<PlanId>): void {
    for (const planId of planIds) {
      this.recomposePlan(planId);
    }
  }

  private recomposePlan(planId: PlanId): void {
    const bootPlan = this.bootPlans.get(planId);
    const slotPlans = this.plansLoadedBySlots(planId);

    if (bootPlan === undefined && slotPlans.length === 0) {
      this.plans.delete(planId);
      return;
    }

    const target = this.activePlanDocument(planId);
    resetPlanDocument(target, planId);

    if (bootPlan !== undefined) {
      composeBootPlanInto(target, bootPlan);
    }

    for (const slotPlan of slotPlans) {
      composeSlotPlanInto(target, slotPlan, bootPlan);
    }
  }

  private activePlanDocument(planId: PlanId): PlanDocument {
    let activePlan = this.plans.get(planId);
    if (activePlan === undefined) {
      activePlan = emptyPlan(planId);
      this.plans.set(planId, activePlan);
    }

    return activePlan;
  }

  private plansLoadedBySlots(planId: PlanId): PlanDocument[] {
    const plans: PlanDocument[] = [];
    for (const slot of this.slots.values()) {
      for (const plan of slot.plans) {
        if (plan.planId === planId) plans.push(plan);
      }
    }

    return plans;
  }

  private abortSlots(): void {
    for (const slot of this.slots.values()) {
      slot.abortController.abort();
    }
  }
}

export const appliedBrowserPlans = new AppliedBrowserPlans();

export function composeInitialPlans(plans: PlanDocument[]): PlanDocument[] {
  const assembledPlans = new Map<PlanId, PlanDocument>();
  for (const plan of plans) {
    const assembled = assembledPlans.get(plan.planId) ?? emptyPlan(plan.planId);
    assembledPlans.set(plan.planId, assembled);
    composeBootPlanInto(assembled, plan);
  }

  return Array.from(assembledPlans.values());
}

function composeBootPlanInto(target: PlanDocument, incoming: PlanDocument): void {
  mergeTypeContracts(target, incoming);

  for (const [componentKey, component] of Object.entries(incoming.components)) {
    mergeBootComponent(target, componentKey, component);
  }

  target.behaviors.push(...incoming.behaviors);
}

function composeSlotPlanInto(
  target: PlanDocument,
  incoming: PlanDocument,
  bootPlan: PlanDocument | undefined,
): void {
  mergeTypeContracts(target, incoming);

  for (const [componentKey, component] of Object.entries(incoming.components)) {
    mergeSlotComponent(
      target,
      componentKey,
      component,
      bootPlan?.components[componentKey] !== undefined,
    );
  }

  target.behaviors.push(...incoming.behaviors);
}

function mergeTypeContracts(target: PlanDocument, incoming: PlanDocument): void {
  for (const [typeKey, contract] of Object.entries(incoming.types)) {
    target.types[typeKey] = mergeObjectContracts(target.types[typeKey], contract);
  }
}

function resetPlanDocument(plan: PlanDocument, planId: PlanId): void {
  plan.version = 3;
  plan.planId = planId;
  plan.scope = { kind: "root" };
  plan.types = {};
  plan.components = {};
  plan.behaviors = [];
}

function emptyPlan(planId: PlanId): PlanDocument {
  return {
    version: 3,
    planId,
    scope: { kind: "root" },
    types: {},
    components: {},
    behaviors: [],
  };
}

function snapshotPlan(plan: PlanDocument): PlanDocument {
  return {
    version: plan.version,
    planId: plan.planId,
    scope: { ...plan.scope },
    types: { ...plan.types },
    components: { ...plan.components },
    behaviors: [...plan.behaviors],
  };
}

function planIdsIn(plans: PlanDocument[]): PlanId[] {
  return Array.from(new Set(plans.map(plan => plan.planId)));
}
