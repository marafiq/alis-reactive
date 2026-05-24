// merge-plan.ts — Plan registry and partial contribution lifecycle orchestration.

import type { Plan, Behavior } from "../types";
import { unwireField } from "../validation/live-clear";
import {
  ComponentContribution,
  ComponentOwnershipLedger,
  ComponentValidationRules,
  LayoutObjectReferenceLedger,
  unionSets,
} from "./component-contribution";
import { BrowserObjectContractLedger, mergeJsTypes } from "./object-contract-fragment";
import {
  planContributionSourceFrom,
  type PartId,
  type PlanContributionSource,
  type PlanId,
} from "./plan-contribution-source";
import { PartialSlotLoad, PartialSlotRegistry, type TrackedPartialPlan } from "./partial-slot";

type WireBehaviors = (behaviors: Behavior[], plan: Plan, signal?: AbortSignal) => void;
type WireContainerValidation = (plan: Plan, signal?: AbortSignal) => void;

export interface MergeHooks {
  wireBehaviors: WireBehaviors;
  wireContainerValidation: WireContainerValidation;
}

export interface PartialSlotLoadResult {
  readonly loadedPlans: Plan[];
  readonly affectedPlanIds: PlanId[];
}

export interface PartialSlotUnloadResult {
  readonly affectedPlanIds: PlanId[];
}

class InitialPlanComposition {
  private readonly assemblies = new Map<PlanId, BootPlanAssembly>();

  static from(plans: Plan[]): InitialPlanComposition {
    const composition = new InitialPlanComposition();
    for (const plan of plans) composition.accept(plan);
    return composition;
  }

  bootPlans(): Plan[] {
    return Array.from(this.assemblies.values()).map(assembly => assembly.toPlan());
  }

  private accept(plan: Plan): void {
    const existing = this.assemblies.get(plan.planId);
    if (existing) {
      existing.accept(plan);
      return;
    }

    this.assemblies.set(plan.planId, BootPlanAssembly.seed(plan));
  }
}

class BootPlanAssembly {
  private readonly componentOwnership = new ComponentOwnershipLedger();
  private readonly layoutObjects = new LayoutObjectReferenceLedger();

  private constructor(private readonly plan: Plan) {}

  static seed(plan: Plan): BootPlanAssembly {
    const assembly = new BootPlanAssembly({
      version: 3,
      planId: plan.planId,
      scope: { kind: "root" },
      types: { ...plan.types },
      components: { ...plan.components },
      behaviors: [...plan.behaviors],
    });
    assembly.claimBootRootComponents();
    return assembly;
  }

  accept(contribution: Plan): void {
    const source = planContributionSourceFrom(contribution);
    this.assertComponentsCanCompose(contribution, source);

    for (const [key, type] of Object.entries(contribution.types)) {
      this.plan.types[key] = mergeJsTypes(this.plan.types[key], type);
    }

    this.mergeComponents(contribution, source);
    this.plan.behaviors.push(...contribution.behaviors);
  }

  private assertComponentsCanCompose(contribution: Plan, source: PlanContributionSource): void {
    for (const [key, component] of Object.entries(contribution.components)) {
      const claim = this.componentOwnership.request(contribution.planId, key);
      ComponentContribution.from(this.plan, key, component, source, claim).assertInitialComposable();
    }
  }

  private mergeComponents(contribution: Plan, source: PlanContributionSource): void {
    for (const [key, component] of Object.entries(contribution.components)) {
      const claim = this.componentOwnership.request(contribution.planId, key);
      ComponentContribution.from(this.plan, key, component, source, claim)
        .composeIntoInitialPlan(this.componentOwnership, this.layoutObjects);
    }
  }

  private claimBootRootComponents(): void {
    for (const key of Object.keys(this.plan.components)) {
      this.componentOwnership.claimRoot(this.plan.planId, key);
    }
  }

  toPlan(): Plan {
    return this.plan;
  }
}

export class PlanRegistry {
  private readonly plans = new Map<string, Plan>();
  private readonly rootPlanIds = new Set<string>();
  private readonly slots = new PartialSlotRegistry();
  private readonly componentOwnership = new ComponentOwnershipLedger();
  private readonly layoutObjects = new LayoutObjectReferenceLedger();
  private readonly typeOwnership = new BrowserObjectContractLedger();

  register(plan: Plan): void {
    this.plans.set(plan.planId, plan);
    this.rootPlanIds.add(plan.planId);
    this.claimRootKeys(plan);
  }

  add(incoming: Plan, hooks: MergeHooks): Plan {
    const source = planContributionSourceFrom(incoming);
    if (source.kind === "partial") this.removePartialSlot(source.partId);

    return this.mergeContribution(incoming, hooks, source);
  }

  loadPartialSlot(partId: PartId, plans: Plan[], hooks: MergeHooks): PartialSlotLoadResult {
    if (plans.length === 0) {
      throw new Error("[alis] partial slot load requires at least one plan; unload the slot explicitly instead");
    }

    const affectedPlanIds = new Set(this.removePartialSlot(partId));
    const loadedPlans: Plan[] = [];

    const load = PartialSlotLoad.containing(partId, plans);
    for (const contribution of load.contributions()) {
      const merged = this.mergeContribution(contribution.plan, hooks, contribution.source);
      loadedPlans.push(merged);
      affectedPlanIds.add(merged.planId);
    }

    return {
      loadedPlans,
      affectedPlanIds: [...affectedPlanIds],
    };
  }

  unloadPartialSlot(partId: PartId): PartialSlotUnloadResult {
    return {
      affectedPlanIds: this.removePartialSlot(partId),
    };
  }

  get(planId: string): Plan | undefined {
    return this.plans.get(planId);
  }

  reset(): void {
    this.plans.clear();
    this.rootPlanIds.clear();
    this.slots.reset();
    this.componentOwnership.reset();
    this.layoutObjects.reset();
    this.typeOwnership.reset();
  }

  private removePartialSlot(partId: PartId): PlanId[] {
    const contributions = this.slots.contributions(partId);
    const affectedPlanIds = new Set<PlanId>();

    for (const contribution of contributions) {
      affectedPlanIds.add(contribution.planId);
      this.removeContribution(contribution);
    }

    this.slots.clear(partId);
    return [...affectedPlanIds];
  }

  private mergeContribution(incoming: Plan, hooks: MergeHooks, source: PlanContributionSource): Plan {
    const target = this.ensureTarget(incoming.planId);
    this.assertTypesCanMerge(incoming, source);
    this.assertComponentsCanMerge(incoming, target, source);
    this.mergeTypes(incoming, target, source);
    this.mergeComponents(incoming, target, source);

    hooks.wireBehaviors(incoming.behaviors, target, source.behaviorSignal);
    target.behaviors.push(...incoming.behaviors);
    hooks.wireContainerValidation(target, source.behaviorSignal);
    this.trackMergedContribution(source, incoming);

    return target;
  }

  private assertTypesCanMerge(incoming: Plan, source: PlanContributionSource): void {
    for (const [key, type] of Object.entries(incoming.types)) {
      const claim = this.typeOwnership.request(incoming.planId, key, type);
      if (!claim.canBeHeldBy(source)) throw claim.collisionError(source);
    }
  }

  private assertComponentsCanMerge(incoming: Plan, target: Plan, source: PlanContributionSource): void {
    for (const [key, comp] of Object.entries(incoming.components)) {
      const claim = this.componentOwnership.request(incoming.planId, key);
      ComponentContribution.from(target, key, comp, source, claim).assertMergeable();
    }
  }

  private ensureTarget(planId: string): Plan {
    let target = this.plans.get(planId);
    if (target === undefined) {
      target = { version: 3, planId, scope: { kind: "root" }, types: {}, components: {}, behaviors: [] };
      this.plans.set(planId, target);
    }
    return target;
  }

  private mergeTypes(incoming: Plan, target: Plan, source: PlanContributionSource): void {
    for (const [key, type] of Object.entries(incoming.types)) {
      const claim = this.typeOwnership.request(incoming.planId, key, type);
      if (!claim.canBeHeldBy(source)) throw claim.collisionError(source);
      target.types[key] = mergeJsTypes(target.types[key], type);
      this.typeOwnership.claim(incoming.planId, key, type, source);
    }
  }

  private mergeComponents(incoming: Plan, target: Plan, source: PlanContributionSource): void {
    for (const [key, comp] of Object.entries(incoming.components)) {
      const claim = this.componentOwnership.request(incoming.planId, key);
      ComponentContribution.from(target, key, comp, source, claim)
        .mergeInto(this.componentOwnership, this.layoutObjects);
    }
  }

  private removeContribution(source: TrackedPartialPlan): void {
    const plan = this.plans.get(source.planId);
    if (plan === undefined) {
      source.abortWiredBehaviors();
      return;
    }

    source.abortWiredBehaviors();
    this.removeSourceBehaviors(plan, source);
    const removedLayoutObjectKeys = this.removeSourceLayoutObjects(plan, source);
    const removedComponentKeys = this.removeSourceComponents(plan, source);
    this.removeSourceValidationRules(plan, source);
    this.pruneOrphanedValidationRules(
      plan,
      source,
      unionSets(removedComponentKeys, removedLayoutObjectKeys));
    this.removeSourceTypes(plan, source);

    if (this.canPruneMergedPlan(source.planId, plan)) {
      this.plans.delete(source.planId);
    }
  }

  private removeSourceBehaviors(plan: Plan, source: TrackedPartialPlan): void {
    for (const behavior of source.behaviors) {
      const idx = plan.behaviors.indexOf(behavior);
      if (idx >= 0) plan.behaviors.splice(idx, 1);
    }
  }

  private removeSourceLayoutObjects(plan: Plan, source: TrackedPartialPlan): Set<string> {
    const removed = new Set<string>();
    for (const key of source.layoutObjectKeys) {
      const release = this.layoutObjects.release(source.planId, key, source.partId);
      if (!release.shouldRemoveMaterializedObject) continue;

      const component = plan.components[key];
      if (component) unwireField(component.id);
      delete plan.components[key];
      this.componentOwnership.release(source.planId, key);
      removed.add(key);
    }

    return removed;
  }

  private removeSourceComponents(plan: Plan, source: TrackedPartialPlan): Set<string> {
    const removed = new Set<string>();
    for (const key of source.componentKeys) {
      if (!this.componentOwnership.isOwnedBy(source.planId, key, source.partId)) continue;
      const comp = plan.components[key];
      if (comp) unwireField(comp.id);
      delete plan.components[key];
      this.componentOwnership.release(source.planId, key);
      removed.add(key);
    }
    return removed;
  }

  private pruneOrphanedValidationRules(
    plan: Plan,
    source: TrackedPartialPlan,
    removedKeys: Set<string>,
  ): void {
    if (removedKeys.size === 0) return;
    for (const [compKey, comp] of Object.entries(plan.components)) {
      const validationRules = ComponentValidationRules.from(comp);
      if (validationRules === undefined) continue;
      if (!this.componentOwnership.isOwnedBy(source.planId, compKey, source.partId)) continue;
      validationRules.removeRulesForComponents(removedKeys);
    }
  }

  private removeSourceTypes(plan: Plan, source: TrackedPartialPlan): void {
    for (const key of source.typeKeys) {
      const remainingContract = this.typeOwnership.releasePartial(source.planId, key, source.partId);
      if (remainingContract === undefined) {
        delete plan.types[key];
        continue;
      }

      plan.types[key] = remainingContract.toJsType();
    }
  }

  private removeSourceValidationRules(plan: Plan, source: TrackedPartialPlan): void {
    for (const contribution of source.validationRuleContributions) {
      contribution.removeFrom(plan);
    }
  }

  private canPruneMergedPlan(planId: PlanId, plan: Plan): boolean {
    const planWasNotBootedAsRoot = !this.rootPlanIds.has(planId);
    const planHasNoBehaviors = plan.behaviors.length === 0;
    const planHasNoComponents = Object.keys(plan.components).length === 0;
    const planHasNoTypes = Object.keys(plan.types).length === 0;

    return planWasNotBootedAsRoot && planHasNoBehaviors && planHasNoComponents && planHasNoTypes;
  }

  private trackMergedContribution(source: PlanContributionSource, incoming: Plan): void {
    if (source.kind === "root") return;

    this.slots.track(source, incoming);
  }

  private claimRootKeys(plan: Plan): void {
    for (const [key, type] of Object.entries(plan.types)) {
      this.typeOwnership.claimRoot(plan.planId, key, type);
    }
    for (const key of Object.keys(plan.components)) {
      this.componentOwnership.claimRoot(plan.planId, key);
    }
  }
}

const registry = new PlanRegistry();

export function composeInitialPlans(plans: Plan[]): Plan[] {
  return InitialPlanComposition.from(plans).bootPlans();
}

export function registerBootedPlan(plan: Plan): void { registry.register(plan); }
export function applyMergedPlan(incoming: Plan, hooks: MergeHooks): Plan { return registry.add(incoming, hooks); }
export function applyPartialSlotLoad(partId: PartId, plans: Plan[], hooks: MergeHooks): PartialSlotLoadResult {
  return registry.loadPartialSlot(partId, plans, hooks);
}
export function applyPartialSlotUnload(partId: PartId): PartialSlotUnloadResult {
  return registry.unloadPartialSlot(partId);
}
export function getBootedPlan(planId: string): Plan | undefined { return registry.get(planId); }
export function resetMergePlanState(): void { registry.reset(); }
