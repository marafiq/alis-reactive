// merge-plan.ts — applied browser plans and partial slot replacement.

import type { Plan, Behavior, BrowserObjectContract, ComponentValidation } from "../types";
import { unwireField } from "../validation/live-clear";
import {
  mergeBootComponent,
  mergeSlotComponent,
  mergeValidationRules,
  replaceValidationRules,
  type ComponentLoad,
} from "./component-slots";
import { mergeObjectContracts } from "./object-contracts";

type PlanId = string;
type SlotId = string;

type WireBehaviors = (behaviors: Behavior[], plan: Plan, signal?: AbortSignal) => void;
type WireContainerValidation = (plan: Plan, signal?: AbortSignal) => void;

export interface MergeHooks {
  wireBehaviors: WireBehaviors;
  wireContainerValidation: WireContainerValidation;
}

interface ObjectContractLoad {
  readonly typeKey: string;
  readonly contract: BrowserObjectContract;
}

interface SlotPlanLoad {
  readonly planId: PlanId;
  readonly behaviors: Behavior[];
  readonly componentLoads: ComponentLoad[];
  readonly objectContractLoads: ObjectContractLoad[];
}

interface SlotLoad {
  readonly abortController: AbortController;
  readonly planLoads: SlotPlanLoad[];
}

export class AppliedBrowserPlans {
  private readonly plans = new Map<PlanId, Plan>();
  private readonly rootPlanIds = new Set<PlanId>();
  private readonly rootComponents = new Set<string>();
  private readonly rootTypes = new Map<string, BrowserObjectContract>();
  private readonly rootValidationRules = new Map<string, ComponentValidation[]>();
  private readonly slots = new Map<SlotId, SlotLoad>();

  register(plan: Plan): void {
    this.plans.set(plan.planId, plan);
    this.rootPlanIds.add(plan.planId);
    this.recordRootPlan(plan);
  }

  loadPartialSlot(slotId: SlotId, plans: Plan[], hooks: MergeHooks): PlanId[] {
    const affectedPlanIds = new Set(this.removePartialSlot(slotId));
    if (plans.length === 0) return [...affectedPlanIds];

    const abortController = new AbortController();
    const planLoads: SlotPlanLoad[] = [];
    for (const plan of plans) {
      const planLoad = this.applyPlanLoad(abortController, plan, hooks);
      planLoads.push(planLoad);
      affectedPlanIds.add(planLoad.planId);
    }

    this.slots.set(slotId, { abortController, planLoads });
    return [...affectedPlanIds];
  }

  unloadPartialSlot(slotId: SlotId): PlanId[] {
    return this.removePartialSlot(slotId);
  }

  get(planId: string): Plan | undefined {
    return this.plans.get(planId);
  }

  reset(): void {
    this.abortSlotLoads();
    this.plans.clear();
    this.rootPlanIds.clear();
    this.rootComponents.clear();
    this.rootTypes.clear();
    this.rootValidationRules.clear();
    this.slots.clear();
  }

  private removePartialSlot(slotId: SlotId): PlanId[] {
    const slotLoad = this.takeSlotLoad(slotId);
    const affectedPlanIds = new Set<PlanId>();
    if (slotLoad === undefined) return [];

    slotLoad.abortController.abort();
    for (const planLoad of slotLoad.planLoads) {
      affectedPlanIds.add(planLoad.planId);
      this.removePlanLoad(planLoad);
    }

    return [...affectedPlanIds];
  }

  private applyPlanLoad(
    abortController: AbortController,
    incoming: Plan,
    hooks: MergeHooks,
  ): SlotPlanLoad {
    const target = this.ensureTarget(incoming.planId);
    const objectContractLoads = this.mergeObjectContractsFrom(incoming, target);
    const componentLoads = this.mergeComponents(incoming, target);

    hooks.wireBehaviors(incoming.behaviors, target, abortController.signal);
    target.behaviors.push(...incoming.behaviors);
    hooks.wireContainerValidation(target, abortController.signal);

    return {
      planId: incoming.planId,
      behaviors: [...incoming.behaviors],
      componentLoads,
      objectContractLoads,
    };
  }

  private removePlanLoad(planLoad: SlotPlanLoad): void {
    const plan = this.plans.get(planLoad.planId)!;
    this.removeBehaviors(plan, planLoad.behaviors);
    this.removeComponentLoads(plan, planLoad);
    this.removeTypes(plan, planLoad);

    if (this.canPruneMergedPlan(planLoad.planId, plan)) {
      this.plans.delete(planLoad.planId);
    }
  }

  private ensureTarget(planId: PlanId): Plan {
    let target = this.plans.get(planId);
    if (target === undefined) {
      target = { version: 3, planId, scope: { kind: "root" }, types: {}, components: {}, behaviors: [] };
      this.plans.set(planId, target);
    }
    return target;
  }

  private mergeObjectContractsFrom(incoming: Plan, target: Plan): ObjectContractLoad[] {
    const objectContractLoads: ObjectContractLoad[] = [];
    for (const [typeKey, contract] of Object.entries(incoming.types)) {
      target.types[typeKey] = mergeObjectContracts(target.types[typeKey], contract);
      objectContractLoads.push({ typeKey, contract });
    }

    return objectContractLoads;
  }

  private mergeComponents(incoming: Plan, target: Plan): ComponentLoad[] {
    const componentLoads: ComponentLoad[] = [];
    for (const [componentKey, component] of Object.entries(incoming.components)) {
      componentLoads.push(
        ...mergeSlotComponent(
          target,
          { componentKey, component },
          this.rootOwnsComponent(incoming.planId, componentKey),
        ),
      );
    }

    return componentLoads;
  }

  private recordRootPlan(plan: Plan): void {
    for (const [typeKey, contract] of Object.entries(plan.types)) {
      const key = planTypeKey(plan.planId, typeKey);
      this.rootTypes.set(key, mergeObjectContracts(this.rootTypes.get(key), contract));
    }
    for (const [componentKey, component] of Object.entries(plan.components)) {
      this.rootComponents.add(planComponentKey(plan.planId, componentKey));
      if (component.container.kind === "validation-container") {
        this.rootValidationRules.set(
          planComponentKey(plan.planId, componentKey),
          [...component.container.validationRules],
        );
      }
    }
  }

  private removeBehaviors(plan: Plan, behaviors: Behavior[]): void {
    const removed = new Set(behaviors);
    plan.behaviors = plan.behaviors.filter(behavior => !removed.has(behavior));
  }

  private removeComponentLoads(plan: Plan, planLoad: SlotPlanLoad): void {
    for (const load of planLoad.componentLoads) {
      if (load.kind === "validation-rules") {
        this.recomputeValidationRules(plan, planLoad.planId, load.containerKey);
        continue;
      }

      if (load.kind === "layout-object") {
        this.removeLayoutObject(plan, planLoad.planId, load);
        continue;
      }

      this.removeMountedComponent(plan, load);
    }
  }

  private recomputeValidationRules(plan: Plan, planId: PlanId, containerKey: string): void {
    const container = plan.components[containerKey];
    if (container?.container.kind !== "validation-container") return;

    const ruleSets = [
      this.rootValidationRules.get(planComponentKey(planId, containerKey)) ?? [],
      ...this.activeValidationRuleSets(planId, containerKey),
    ];
    replaceValidationRules(container, mergeValidationRules(ruleSets));
  }

  private removeMountedComponent(
    plan: Plan,
    load: Extract<ComponentLoad, { kind: "component" }>,
  ): void {
    const component = plan.components[load.componentKey];
    if (component?.id !== load.componentId) return;

    unwireField(component.id);
    delete plan.components[load.componentKey];
  }

  private removeLayoutObject(
    plan: Plan,
    planId: PlanId,
    load: Extract<ComponentLoad, { kind: "layout-object" }>,
  ): void {
    if (this.rootOwnsComponent(planId, load.componentKey)) return;
    if (this.activeSlotsReferenceLayoutObject(planId, load.componentKey)) return;

    const component = plan.components[load.componentKey];
    if (component?.id !== load.componentId) return;

    unwireField(component.id);
    delete plan.components[load.componentKey];
  }

  private removeTypes(plan: Plan, planLoad: SlotPlanLoad): void {
    const typeKeys = new Set(planLoad.objectContractLoads.map(contract => contract.typeKey));
    for (const typeKey of typeKeys) {
      this.recomputeType(plan, planLoad.planId, typeKey);
    }
  }

  private recomputeType(plan: Plan, planId: PlanId, typeKey: string): void {
    const rootContract = this.rootTypes.get(planTypeKey(planId, typeKey));
    let remaining = rootContract === undefined
      ? undefined
      : mergeObjectContracts(undefined, rootContract);

    for (const planLoad of this.activeSlotPlanLoads()) {
      if (planLoad.planId !== planId) continue;
      for (const contract of planLoad.objectContractLoads) {
        if (contract.typeKey !== typeKey) continue;
        remaining = mergeObjectContracts(remaining, contract.contract);
      }
    }

    if (remaining === undefined) {
      delete plan.types[typeKey];
      return;
    }

    plan.types[typeKey] = remaining;
  }

  private activeSlotsReferenceLayoutObject(planId: PlanId, componentKey: string): boolean {
    for (const planLoad of this.activeSlotPlanLoads()) {
      if (planLoad.planId !== planId) continue;
      if (planLoad.componentLoads.some(load => load.kind === "layout-object" && load.componentKey === componentKey)) {
        return true;
      }
    }

    return false;
  }

  private rootOwnsComponent(planId: PlanId, componentKey: string): boolean {
    return this.rootComponents.has(planComponentKey(planId, componentKey));
  }

  private canPruneMergedPlan(planId: PlanId, plan: Plan): boolean {
    const planWasNotBootedAsRoot = !this.rootPlanIds.has(planId);
    const planHasNoBehaviors = plan.behaviors.length === 0;
    const planHasNoComponents = Object.keys(plan.components).length === 0;
    const planHasNoTypes = Object.keys(plan.types).length === 0;

    return planWasNotBootedAsRoot && planHasNoBehaviors && planHasNoComponents && planHasNoTypes;
  }

  private takeSlotLoad(slotId: SlotId): SlotLoad | undefined {
    const slotLoad = this.slots.get(slotId);
    this.slots.delete(slotId);
    return slotLoad;
  }

  private *activeSlotPlanLoads(): Iterable<SlotPlanLoad> {
    for (const slotLoad of this.slots.values()) {
      yield* slotLoad.planLoads;
    }
  }

  private *activeValidationRuleSets(
    planId: PlanId,
    containerKey: string,
  ): Iterable<ComponentValidation[]> {
    for (const planLoad of this.activeSlotPlanLoads()) {
      if (planLoad.planId !== planId) continue;
      for (const load of planLoad.componentLoads) {
        if (load.kind !== "validation-rules") continue;
        if (load.containerKey !== containerKey) continue;
        yield load.rules;
      }
    }
  }

  private abortSlotLoads(): void {
    for (const slotLoad of this.slots.values()) {
      slotLoad.abortController.abort();
    }
  }
}

const browserPlans = new AppliedBrowserPlans();

export function composeInitialPlans(plans: Plan[]): Plan[] {
  const assembledPlans = new Map<PlanId, Plan>();
  for (const plan of plans) {
    let assembled = assembledPlans.get(plan.planId);
    if (assembled === undefined) {
      assembled = {
        version: 3,
        planId: plan.planId,
        scope: { kind: "root" },
        types: {},
        components: {},
        behaviors: [],
      };
      assembledPlans.set(plan.planId, assembled);
    }

    acceptBootPlan(assembled, plan);
  }

  return Array.from(assembledPlans.values());
}

function acceptBootPlan(assembled: Plan, loadedPlan: Plan): void {
  for (const [typeKey, contract] of Object.entries(loadedPlan.types)) {
    assembled.types[typeKey] = mergeObjectContracts(assembled.types[typeKey], contract);
  }

  for (const [componentKey, component] of Object.entries(loadedPlan.components)) {
    mergeBootComponent(assembled, { componentKey, component });
  }

  assembled.behaviors.push(...loadedPlan.behaviors);
}

function planComponentKey(planId: PlanId, componentKey: string): string {
  return `${planId}:component:${componentKey}`;
}

function planTypeKey(planId: PlanId, typeKey: string): string {
  return `${planId}:type:${typeKey}`;
}

export function registerBootedPlan(plan: Plan): void { browserPlans.register(plan); }
export function applyPartialSlotLoad(slotId: SlotId, plans: Plan[], hooks: MergeHooks): PlanId[] {
  return browserPlans.loadPartialSlot(slotId, plans, hooks);
}
export function applyPartialSlotUnload(slotId: SlotId): PlanId[] {
  return browserPlans.unloadPartialSlot(slotId);
}
export function getBootedPlan(planId: string): Plan | undefined { return browserPlans.get(planId); }
export function resetMergePlanState(): void { browserPlans.reset(); }
