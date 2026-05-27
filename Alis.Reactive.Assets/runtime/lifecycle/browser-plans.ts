// browser-plans.ts - boot plan composition and partial slot lifetimes.

import type { PlanDocument, Behavior, BrowserObjectContract, ComponentValidation } from "../types";
import { unwireField } from "../validation/live-clear";
import {
  mergeBootComponent,
  mergeSlotComponent,
  mergeValidationRules,
  replaceValidationRules,
  type LoadedComponent,
} from "./component-slots";
import { mergeObjectContracts } from "./object-contracts";

type PlanId = string;
type PartialSlotId = string;

type WireBehaviors = (behaviors: Behavior[], plan: PlanDocument, signal?: AbortSignal) => void;
type WireContainerValidation = (plan: PlanDocument, signal?: AbortSignal) => void;

export interface PlanLifecycleHooks {
  wireBehaviors: WireBehaviors;
  wireContainerValidation: WireContainerValidation;
}

interface LoadedObjectContract {
  readonly typeKey: string;
  readonly contract: BrowserObjectContract;
}

interface LoadedPartialPlan {
  readonly planId: PlanId;
  readonly behaviors: Behavior[];
  readonly components: LoadedComponent[];
  readonly objectContracts: LoadedObjectContract[];
}

interface LoadedPartialSlot {
  readonly abortController: AbortController;
  readonly loadedPlans: LoadedPartialPlan[];
}

export class BrowserPlanStore {
  private readonly plans = new Map<PlanId, PlanDocument>();
  private readonly bootedPlanIds = new Set<PlanId>();
  private readonly bootedComponents = new Set<string>();
  private readonly bootedTypes = new Map<string, BrowserObjectContract>();
  private readonly bootedValidationRules = new Map<string, ComponentValidation[]>();
  private readonly slots = new Map<PartialSlotId, LoadedPartialSlot>();

  register(plan: PlanDocument): void {
    this.plans.set(plan.planId, plan);
    this.bootedPlanIds.add(plan.planId);
    this.recordBootPlan(plan);
  }

  loadPartialSlot(slotId: PartialSlotId, plans: PlanDocument[], hooks: PlanLifecycleHooks): PlanId[] {
    const affectedPlanIds = new Set(this.removePartialSlot(slotId));
    if (plans.length === 0) return [...affectedPlanIds];

    const abortController = new AbortController();
    const loadedPlans: LoadedPartialPlan[] = [];
    for (const plan of plans) {
      const loadedPlan = this.loadPartialPlan(abortController, plan, hooks);
      loadedPlans.push(loadedPlan);
      affectedPlanIds.add(loadedPlan.planId);
    }

    this.slots.set(slotId, { abortController, loadedPlans });
    return [...affectedPlanIds];
  }

  unloadPartialSlot(slotId: PartialSlotId): PlanId[] {
    return this.removePartialSlot(slotId);
  }

  get(planId: string): PlanDocument | undefined {
    return this.plans.get(planId);
  }

  reset(): void {
    this.abortLoadedPartialSlots();
    this.plans.clear();
    this.bootedPlanIds.clear();
    this.bootedComponents.clear();
    this.bootedTypes.clear();
    this.bootedValidationRules.clear();
    this.slots.clear();
  }

  private removePartialSlot(slotId: PartialSlotId): PlanId[] {
    const partialSlot = this.takeLoadedPartialSlot(slotId);
    const affectedPlanIds = new Set<PlanId>();
    if (partialSlot === undefined) return [];

    partialSlot.abortController.abort();
    for (const loadedPlan of partialSlot.loadedPlans) {
      affectedPlanIds.add(loadedPlan.planId);
      this.unloadPartialPlan(loadedPlan);
    }

    return [...affectedPlanIds];
  }

  private loadPartialPlan(
    abortController: AbortController,
    incoming: PlanDocument,
    hooks: PlanLifecycleHooks,
  ): LoadedPartialPlan {
    const target = this.ensureTarget(incoming.planId);
    const objectContracts = this.mergeObjectContractsFrom(incoming, target);
    const components = this.mergeComponents(incoming, target);

    hooks.wireBehaviors(incoming.behaviors, target, abortController.signal);
    target.behaviors.push(...incoming.behaviors);
    hooks.wireContainerValidation(target, abortController.signal);

    return {
      planId: incoming.planId,
      behaviors: [...incoming.behaviors],
      components,
      objectContracts,
    };
  }

  private unloadPartialPlan(loadedPlan: LoadedPartialPlan): void {
    const plan = this.plans.get(loadedPlan.planId)!;
    this.removeBehaviors(plan, loadedPlan.behaviors);
    this.removeLoadedComponents(plan, loadedPlan);
    this.removeTypes(plan, loadedPlan);

    if (this.canRemovePlanDocument(loadedPlan.planId, plan)) {
      this.plans.delete(loadedPlan.planId);
    }
  }

  private ensureTarget(planId: PlanId): PlanDocument {
    let target = this.plans.get(planId);
    if (target === undefined) {
      target = { version: 3, planId, scope: { kind: "root" }, types: {}, components: {}, behaviors: [] };
      this.plans.set(planId, target);
    }
    return target;
  }

  private mergeObjectContractsFrom(incoming: PlanDocument, target: PlanDocument): LoadedObjectContract[] {
    const objectContracts: LoadedObjectContract[] = [];
    for (const [typeKey, contract] of Object.entries(incoming.types)) {
      target.types[typeKey] = mergeObjectContracts(target.types[typeKey], contract);
      objectContracts.push({ typeKey, contract });
    }

    return objectContracts;
  }

  private mergeComponents(incoming: PlanDocument, target: PlanDocument): LoadedComponent[] {
    const components: LoadedComponent[] = [];
    for (const [componentKey, component] of Object.entries(incoming.components)) {
      components.push(
        ...mergeSlotComponent(
          target,
          { componentKey, component },
          this.componentWasBooted(incoming.planId, componentKey),
        ),
      );
    }

    return components;
  }

  private recordBootPlan(plan: PlanDocument): void {
    for (const [typeKey, contract] of Object.entries(plan.types)) {
      const key = planTypeKey(plan.planId, typeKey);
      this.bootedTypes.set(key, mergeObjectContracts(this.bootedTypes.get(key), contract));
    }
    for (const [componentKey, component] of Object.entries(plan.components)) {
      this.bootedComponents.add(planComponentKey(plan.planId, componentKey));
      if (component.container.kind === "validation-container") {
        this.bootedValidationRules.set(
          planComponentKey(plan.planId, componentKey),
          [...component.container.validationRules],
        );
      }
    }
  }

  private removeBehaviors(plan: PlanDocument, behaviors: Behavior[]): void {
    const removed = new Set(behaviors);
    plan.behaviors = plan.behaviors.filter(behavior => !removed.has(behavior));
  }

  private removeLoadedComponents(plan: PlanDocument, loadedPlan: LoadedPartialPlan): void {
    for (const load of loadedPlan.components) {
      if (load.kind === "validation-rules") {
        this.recomputeValidationRules(plan, loadedPlan.planId, load.containerKey);
        continue;
      }

      if (load.kind === "layout-object") {
        this.removeLayoutObject(plan, loadedPlan.planId, load);
        continue;
      }

      this.removeMountedComponent(plan, load);
    }
  }

  private recomputeValidationRules(plan: PlanDocument, planId: PlanId, containerKey: string): void {
    const container = plan.components[containerKey];
    if (container?.container.kind !== "validation-container") return;

    const ruleSets = [
      this.bootedValidationRules.get(planComponentKey(planId, containerKey)) ?? [],
      ...this.activeValidationRuleSets(planId, containerKey),
    ];
    replaceValidationRules(container, mergeValidationRules(ruleSets));
  }

  private removeMountedComponent(
    plan: PlanDocument,
    load: Extract<LoadedComponent, { kind: "component" }>,
  ): void {
    const component = plan.components[load.componentKey];
    if (component?.id !== load.componentId) return;

    unwireField(component.id);
    delete plan.components[load.componentKey];
  }

  private removeLayoutObject(
    plan: PlanDocument,
    planId: PlanId,
    load: Extract<LoadedComponent, { kind: "layout-object" }>,
  ): void {
    if (this.componentWasBooted(planId, load.componentKey)) return;
    if (this.activeSlotsReferenceLayoutObject(planId, load.componentKey)) return;

    const component = plan.components[load.componentKey];
    if (component?.id !== load.componentId) return;

    unwireField(component.id);
    delete plan.components[load.componentKey];
  }

  private removeTypes(plan: PlanDocument, loadedPlan: LoadedPartialPlan): void {
    const typeKeys = new Set(loadedPlan.objectContracts.map(contract => contract.typeKey));
    for (const typeKey of typeKeys) {
      this.recomputeType(plan, loadedPlan.planId, typeKey);
    }
  }

  private recomputeType(plan: PlanDocument, planId: PlanId, typeKey: string): void {
    const rootContract = this.bootedTypes.get(planTypeKey(planId, typeKey));
    let remaining = rootContract === undefined
      ? undefined
      : mergeObjectContracts(undefined, rootContract);

    for (const loadedPlan of this.activeLoadedPartialPlans()) {
      if (loadedPlan.planId !== planId) continue;
      for (const contract of loadedPlan.objectContracts) {
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
    for (const loadedPlan of this.activeLoadedPartialPlans()) {
      if (loadedPlan.planId !== planId) continue;
      if (loadedPlan.components.some(load => load.kind === "layout-object" && load.componentKey === componentKey)) {
        return true;
      }
    }

    return false;
  }

  private componentWasBooted(planId: PlanId, componentKey: string): boolean {
    return this.bootedComponents.has(planComponentKey(planId, componentKey));
  }

  private canRemovePlanDocument(planId: PlanId, plan: PlanDocument): boolean {
    const planWasNotBooted = !this.bootedPlanIds.has(planId);
    const planHasNoBehaviors = plan.behaviors.length === 0;
    const planHasNoComponents = Object.keys(plan.components).length === 0;
    const planHasNoTypes = Object.keys(plan.types).length === 0;

    return planWasNotBooted && planHasNoBehaviors && planHasNoComponents && planHasNoTypes;
  }

  private takeLoadedPartialSlot(slotId: PartialSlotId): LoadedPartialSlot | undefined {
    const partialSlot = this.slots.get(slotId);
    this.slots.delete(slotId);
    return partialSlot;
  }

  private *activeLoadedPartialPlans(): Iterable<LoadedPartialPlan> {
    for (const partialSlot of this.slots.values()) {
      yield* partialSlot.loadedPlans;
    }
  }

  private *activeValidationRuleSets(
    planId: PlanId,
    containerKey: string,
  ): Iterable<ComponentValidation[]> {
    for (const loadedPlan of this.activeLoadedPartialPlans()) {
      if (loadedPlan.planId !== planId) continue;
      for (const load of loadedPlan.components) {
        if (load.kind !== "validation-rules") continue;
        if (load.containerKey !== containerKey) continue;
        yield load.rules;
      }
    }
  }

  private abortLoadedPartialSlots(): void {
    for (const partialSlot of this.slots.values()) {
      partialSlot.abortController.abort();
    }
  }
}

const browserPlans = new BrowserPlanStore();

export function composeInitialPlans(plans: PlanDocument[]): PlanDocument[] {
  const assembledPlans = new Map<PlanId, PlanDocument>();
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

    composeBootPlanInto(assembled, plan);
  }

  return Array.from(assembledPlans.values());
}

function composeBootPlanInto(assembled: PlanDocument, loadedPlan: PlanDocument): void {
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

export function registerBootedPlan(plan: PlanDocument): void { browserPlans.register(plan); }
export function applyPartialSlotLoad(slotId: PartialSlotId, plans: PlanDocument[], hooks: PlanLifecycleHooks): PlanId[] {
  return browserPlans.loadPartialSlot(slotId, plans, hooks);
}
export function applyPartialSlotUnload(slotId: PartialSlotId): PlanId[] {
  return browserPlans.unloadPartialSlot(slotId);
}
export function getBootedPlan(planId: string): PlanDocument | undefined { return browserPlans.get(planId); }
export function resetBrowserPlansForTests(): void { browserPlans.reset(); }
