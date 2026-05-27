// merge-plan.ts — applied browser plans and partial slot replacement.

import type { Plan, Behavior, BrowserObjectContract, ComponentValidation } from "../types";
import { unwireField } from "../validation/live-clear";
import {
  mergeBootComponent,
  mergeSlotComponent,
  mergeValidationRules,
  replaceValidationRules,
  type SlotComponentLoad,
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

interface LoadedTypeContract {
  readonly typeKey: string;
  readonly contract: BrowserObjectContract;
}

interface LoadedSlotPlan {
  readonly planId: PlanId;
  readonly slotLoad: AbortController;
  readonly behaviors: Behavior[];
  readonly componentLoads: SlotComponentLoad[];
  readonly typeContracts: LoadedTypeContract[];
}

export class AppliedBrowserPlans {
  private readonly plans = new Map<PlanId, Plan>();
  private readonly rootPlanIds = new Set<PlanId>();
  private readonly rootComponents = new Set<string>();
  private readonly rootTypes = new Map<string, BrowserObjectContract>();
  private readonly rootValidationRules = new Map<string, ComponentValidation[]>();
  private readonly slots = new Map<SlotId, LoadedSlotPlan[]>();

  register(plan: Plan): void {
    this.plans.set(plan.planId, plan);
    this.rootPlanIds.add(plan.planId);
    this.recordRootPlan(plan);
  }

  loadPartialSlot(slotId: SlotId, plans: Plan[], hooks: MergeHooks): PlanId[] {
    const affectedPlanIds = new Set(this.unapplyPartialSlot(slotId));
    if (plans.length === 0) return [...affectedPlanIds];

    const slotLoad = new AbortController();
    const loadedPlans: LoadedSlotPlan[] = [];
    for (const plan of plans) {
      const loadedPlan = this.applySlotPlan(slotLoad, plan, hooks);
      loadedPlans.push(loadedPlan);
      affectedPlanIds.add(loadedPlan.planId);
    }

    this.slots.set(slotId, loadedPlans);
    return [...affectedPlanIds];
  }

  unloadPartialSlot(slotId: SlotId): PlanId[] {
    return this.unapplyPartialSlot(slotId);
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

  private unapplyPartialSlot(slotId: SlotId): PlanId[] {
    const slotPlans = this.takeSlotPlans(slotId);
    const affectedPlanIds = new Set<PlanId>();
    if (slotPlans.length > 0) slotPlans[0]!.slotLoad.abort();

    for (const slotPlan of slotPlans) {
      affectedPlanIds.add(slotPlan.planId);
      this.removeSlotPlan(slotPlan);
    }

    return [...affectedPlanIds];
  }

  private applySlotPlan(
    slotLoad: AbortController,
    incoming: Plan,
    hooks: MergeHooks,
  ): LoadedSlotPlan {
    const target = this.ensureTarget(incoming.planId);
    const typeContracts = this.mergeTypes(incoming, target);
    const componentLoads = this.mergeComponents(incoming, target);

    hooks.wireBehaviors(incoming.behaviors, target, slotLoad.signal);
    target.behaviors.push(...incoming.behaviors);
    hooks.wireContainerValidation(target, slotLoad.signal);

    return {
      planId: incoming.planId,
      slotLoad,
      behaviors: [...incoming.behaviors],
      componentLoads,
      typeContracts,
    };
  }

  private removeSlotPlan(slotPlan: LoadedSlotPlan): void {
    const plan = this.plans.get(slotPlan.planId)!;
    this.removeBehaviors(plan, slotPlan.behaviors);
    this.removeComponentLoads(plan, slotPlan);
    this.removeTypes(plan, slotPlan);

    if (this.canPruneMergedPlan(slotPlan.planId, plan)) {
      this.plans.delete(slotPlan.planId);
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

  private mergeTypes(incoming: Plan, target: Plan): LoadedTypeContract[] {
    const loadedTypes: LoadedTypeContract[] = [];
    for (const [typeKey, contract] of Object.entries(incoming.types)) {
      target.types[typeKey] = mergeObjectContracts(target.types[typeKey], contract);
      loadedTypes.push({ typeKey, contract });
    }

    return loadedTypes;
  }

  private mergeComponents(incoming: Plan, target: Plan): SlotComponentLoad[] {
    const componentLoads: SlotComponentLoad[] = [];
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

  private removeComponentLoads(plan: Plan, slotPlan: LoadedSlotPlan): void {
    for (const load of slotPlan.componentLoads) {
      if (load.kind === "validation-rules") {
        this.recomputeValidationRules(plan, slotPlan.planId, load.containerKey);
        continue;
      }

      if (load.kind === "layout-object") {
        this.removeLayoutObject(plan, slotPlan.planId, load);
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
    load: Extract<SlotComponentLoad, { kind: "component" }>,
  ): void {
    const component = plan.components[load.componentKey];
    if (component?.id !== load.componentId) return;

    unwireField(component.id);
    delete plan.components[load.componentKey];
  }

  private removeLayoutObject(
    plan: Plan,
    planId: PlanId,
    load: Extract<SlotComponentLoad, { kind: "layout-object" }>,
  ): void {
    if (this.rootOwnsComponent(planId, load.componentKey)) return;
    if (this.activeSlotsReferenceLayoutObject(planId, load.componentKey)) return;

    const component = plan.components[load.componentKey];
    if (component?.id !== load.componentId) return;

    unwireField(component.id);
    delete plan.components[load.componentKey];
  }

  private removeTypes(plan: Plan, slotPlan: LoadedSlotPlan): void {
    const typeKeys = new Set(slotPlan.typeContracts.map(contract => contract.typeKey));
    for (const typeKey of typeKeys) {
      this.recomputeType(plan, slotPlan.planId, typeKey);
    }
  }

  private recomputeType(plan: Plan, planId: PlanId, typeKey: string): void {
    const rootContract = this.rootTypes.get(planTypeKey(planId, typeKey));
    let remaining = rootContract === undefined
      ? undefined
      : mergeObjectContracts(undefined, rootContract);

    for (const slotPlan of this.activeSlotPlans()) {
      if (slotPlan.planId !== planId) continue;
      for (const contract of slotPlan.typeContracts) {
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
    for (const slotPlan of this.activeSlotPlans()) {
      if (slotPlan.planId !== planId) continue;
      if (slotPlan.componentLoads.some(load => load.kind === "layout-object" && load.componentKey === componentKey)) {
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

  private takeSlotPlans(slotId: SlotId): LoadedSlotPlan[] {
    const slotPlans = this.slots.get(slotId) ?? [];
    this.slots.delete(slotId);
    return [...slotPlans];
  }

  private *activeSlotPlans(): Iterable<LoadedSlotPlan> {
    for (const slotPlans of this.slots.values()) {
      yield* slotPlans;
    }
  }

  private *activeValidationRuleSets(
    planId: PlanId,
    containerKey: string,
  ): Iterable<ComponentValidation[]> {
    for (const slotPlan of this.activeSlotPlans()) {
      if (slotPlan.planId !== planId) continue;
      for (const load of slotPlan.componentLoads) {
        if (load.kind !== "validation-rules") continue;
        if (load.containerKey !== containerKey) continue;
        yield load.rules;
      }
    }
  }

  private abortSlotLoads(): void {
    for (const slotPlans of this.slots.values()) {
      if (slotPlans.length > 0) slotPlans[0]!.slotLoad.abort();
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
