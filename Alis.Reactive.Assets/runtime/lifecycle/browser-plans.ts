// browser-plans.ts - boot plan composition and partial slot lifetimes.

import type { PlanDocument, Behavior, BrowserObjectContract, ComponentValidation } from "../types";
import { unwireField } from "../validation/live-clear";
import {
  mergeBootComponent,
  mergeSlotComponent,
  mergeValidationRules,
  replaceValidationRules,
  type ComponentSlotLoad,
} from "./component-slots";
import { mergeObjectContracts } from "./object-contracts";

type PlanId = string;
type PartialSlotId = string;

type WireBehaviors = (behaviors: Behavior[], plan: PlanDocument, signal?: AbortSignal) => void;
type WireContainerValidation = (plan: PlanDocument, signal?: AbortSignal) => void;

export interface BrowserPlanWiring {
  wireBehaviors: WireBehaviors;
  wireContainerValidation: WireContainerValidation;
}

interface LoadedTypeContract {
  readonly typeKey: string;
  readonly contract: BrowserObjectContract;
}

interface LoadedPartialPlan {
  readonly planId: PlanId;
  readonly behaviors: Behavior[];
  readonly components: ComponentSlotLoad[];
  readonly typeContracts: LoadedTypeContract[];
}

interface PartialSlotLoad {
  readonly abortController: AbortController;
  readonly plans: LoadedPartialPlan[];
}

export class BrowserPlanStore {
  private readonly plans = new Map<PlanId, PlanDocument>();
  private readonly bootedPlanIds = new Set<PlanId>();
  private readonly bootedComponents = new Set<string>();
  private readonly bootedTypes = new Map<string, BrowserObjectContract>();
  private readonly bootedValidationRules = new Map<string, ComponentValidation[]>();
  private readonly slots = new Map<PartialSlotId, PartialSlotLoad>();

  register(plan: PlanDocument): void {
    this.plans.set(plan.planId, plan);
    this.bootedPlanIds.add(plan.planId);
    this.recordBootPlan(plan);
  }

  loadPartialSlot(slotId: PartialSlotId, plans: PlanDocument[], wiring: BrowserPlanWiring): PlanId[] {
    const affectedPlanIds = new Set(this.removePartialSlot(slotId));
    if (plans.length === 0) return [...affectedPlanIds];

    const abortController = new AbortController();
    const slotPlans: LoadedPartialPlan[] = [];
    for (const plan of plans) {
      const slotPlan = this.mergePartialPlan(abortController, plan, wiring);
      slotPlans.push(slotPlan);
      affectedPlanIds.add(slotPlan.planId);
    }

    this.slots.set(slotId, { abortController, plans: slotPlans });
    return [...affectedPlanIds];
  }

  unloadPartialSlot(slotId: PartialSlotId): PlanId[] {
    return this.removePartialSlot(slotId);
  }

  get(planId: string): PlanDocument | undefined {
    return this.plans.get(planId);
  }

  reset(): void {
    this.abortSlotLoads();
    this.plans.clear();
    this.bootedPlanIds.clear();
    this.bootedComponents.clear();
    this.bootedTypes.clear();
    this.bootedValidationRules.clear();
    this.slots.clear();
  }

  private removePartialSlot(slotId: PartialSlotId): PlanId[] {
    const slotLoad = this.takeSlotLoad(slotId);
    const affectedPlanIds = new Set<PlanId>();
    if (slotLoad === undefined) return [];

    slotLoad.abortController.abort();
    for (const slotPlan of slotLoad.plans) {
      affectedPlanIds.add(slotPlan.planId);
      this.removePartialPlanEntries(slotPlan);
    }

    return [...affectedPlanIds];
  }

  private mergePartialPlan(
    abortController: AbortController,
    incoming: PlanDocument,
    wiring: BrowserPlanWiring,
  ): LoadedPartialPlan {
    const target = this.ensureTarget(incoming.planId);
    const typeContracts = this.mergeTypeContractsFrom(incoming, target);
    const components = this.mergeComponents(incoming, target);

    wiring.wireBehaviors(incoming.behaviors, target, abortController.signal);
    target.behaviors.push(...incoming.behaviors);
    wiring.wireContainerValidation(target, abortController.signal);

    return {
      planId: incoming.planId,
      behaviors: [...incoming.behaviors],
      components,
      typeContracts,
    };
  }

  private removePartialPlanEntries(slotPlan: LoadedPartialPlan): void {
    const plan = this.plans.get(slotPlan.planId)!;
    this.removeBehaviors(plan, slotPlan.behaviors);
    this.removeComponentSlotLoads(plan, slotPlan);
    this.removeTypes(plan, slotPlan);

    if (this.canRemovePlanDocument(slotPlan.planId, plan)) {
      this.plans.delete(slotPlan.planId);
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

  private mergeTypeContractsFrom(incoming: PlanDocument, target: PlanDocument): LoadedTypeContract[] {
    const typeContracts: LoadedTypeContract[] = [];
    for (const [typeKey, contract] of Object.entries(incoming.types)) {
      target.types[typeKey] = mergeObjectContracts(target.types[typeKey], contract);
      typeContracts.push({ typeKey, contract });
    }

    return typeContracts;
  }

  private mergeComponents(incoming: PlanDocument, target: PlanDocument): ComponentSlotLoad[] {
    const components: ComponentSlotLoad[] = [];
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

  private removeComponentSlotLoads(plan: PlanDocument, slotPlan: LoadedPartialPlan): void {
    for (const load of slotPlan.components) {
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
    load: Extract<ComponentSlotLoad, { kind: "component" }>,
  ): void {
    const component = plan.components[load.componentKey];
    if (component?.id !== load.componentId) return;

    unwireField(component.id);
    delete plan.components[load.componentKey];
  }

  private removeLayoutObject(
    plan: PlanDocument,
    planId: PlanId,
    load: Extract<ComponentSlotLoad, { kind: "layout-object" }>,
  ): void {
    if (this.componentWasBooted(planId, load.componentKey)) return;
    if (this.activeSlotsReferenceLayoutObject(planId, load.componentKey)) return;

    const component = plan.components[load.componentKey];
    if (component?.id !== load.componentId) return;

    unwireField(component.id);
    delete plan.components[load.componentKey];
  }

  private removeTypes(plan: PlanDocument, slotPlan: LoadedPartialPlan): void {
    const typeKeys = new Set(slotPlan.typeContracts.map(contract => contract.typeKey));
    for (const typeKey of typeKeys) {
      this.recomputeType(plan, slotPlan.planId, typeKey);
    }
  }

  private recomputeType(plan: PlanDocument, planId: PlanId, typeKey: string): void {
    const rootContract = this.bootedTypes.get(planTypeKey(planId, typeKey));
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
      if (slotPlan.components.some(load => load.kind === "layout-object" && load.componentKey === componentKey)) {
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

  private takeSlotLoad(slotId: PartialSlotId): PartialSlotLoad | undefined {
    const slotLoad = this.slots.get(slotId);
    this.slots.delete(slotId);
    return slotLoad;
  }

  private *activeSlotPlans(): Iterable<LoadedPartialPlan> {
    for (const slotLoad of this.slots.values()) {
      yield* slotLoad.plans;
    }
  }

  private *activeValidationRuleSets(
    planId: PlanId,
    containerKey: string,
  ): Iterable<ComponentValidation[]> {
    for (const slotPlan of this.activeSlotPlans()) {
      if (slotPlan.planId !== planId) continue;
      for (const load of slotPlan.components) {
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

function composeBootPlanInto(assembled: PlanDocument, incoming: PlanDocument): void {
  for (const [typeKey, contract] of Object.entries(incoming.types)) {
    assembled.types[typeKey] = mergeObjectContracts(assembled.types[typeKey], contract);
  }

  for (const [componentKey, component] of Object.entries(incoming.components)) {
    mergeBootComponent(assembled, { componentKey, component });
  }

  assembled.behaviors.push(...incoming.behaviors);
}

function planComponentKey(planId: PlanId, componentKey: string): string {
  return `${planId}:component:${componentKey}`;
}

function planTypeKey(planId: PlanId, typeKey: string): string {
  return `${planId}:type:${typeKey}`;
}

export function registerBootedPlan(plan: PlanDocument): void { browserPlans.register(plan); }
export function applyPartialSlotLoad(slotId: PartialSlotId, plans: PlanDocument[], wiring: BrowserPlanWiring): PlanId[] {
  return browserPlans.loadPartialSlot(slotId, plans, wiring);
}
export function applyPartialSlotUnload(slotId: PartialSlotId): PlanId[] {
  return browserPlans.unloadPartialSlot(slotId);
}
export function getBootedPlan(planId: string): PlanDocument | undefined { return browserPlans.get(planId); }
export function resetBrowserPlansForTests(): void { browserPlans.reset(); }
