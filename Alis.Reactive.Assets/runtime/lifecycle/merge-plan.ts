// merge-plan.ts — applied browser plans and partial slot replacement.

import type { Plan, Behavior } from "../types";
import { unwireField } from "../validation/live-clear";
import {
  ComponentOwnershipLedger,
  LayoutObjectReferenceLedger,
  assertComponentCanComposeInitialPlan,
  assertComponentCanMerge,
  composeInitialComponentIntoPlan,
  mergeComponentIntoPlan,
  validationRulesOf,
} from "./component-contribution";
import { BrowserObjectContractLedger, mergeJsTypes } from "./object-contract-fragment";
import {
  planContributionSourceFrom,
  type PartId,
  PartialPlanContributionSource,
  type PlanContributionSource,
  type PlanId,
} from "./plan-contribution-source";
import { AppliedPartialSlots, type AppliedSlotContribution } from "./partial-slot";

type WireBehaviors = (behaviors: Behavior[], plan: Plan, signal?: AbortSignal) => void;
type WireContainerValidation = (plan: Plan, signal?: AbortSignal) => void;

export interface MergeHooks {
  wireBehaviors: WireBehaviors;
  wireContainerValidation: WireContainerValidation;
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
      assertComponentCanComposeInitialPlan(
        this.plan,
        { planId: contribution.planId, key, component, source },
        { ownership: this.componentOwnership, layoutObjects: this.layoutObjects },
      );
    }
  }

  private mergeComponents(contribution: Plan, source: PlanContributionSource): void {
    for (const [key, component] of Object.entries(contribution.components)) {
      composeInitialComponentIntoPlan(
        this.plan,
        { planId: contribution.planId, key, component, source },
        { ownership: this.componentOwnership, layoutObjects: this.layoutObjects },
      );
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

export class AppliedBrowserPlans {
  private readonly plans = new Map<string, Plan>();
  private readonly rootPlanIds = new Set<string>();
  private readonly slots = new AppliedPartialSlots();
  private readonly componentOwnership = new ComponentOwnershipLedger();
  private readonly layoutObjects = new LayoutObjectReferenceLedger();
  private readonly typeOwnership = new BrowserObjectContractLedger();

  register(plan: Plan): void {
    this.plans.set(plan.planId, plan);
    this.rootPlanIds.add(plan.planId);
    this.claimRootKeys(plan);
  }

  loadPartialSlot(partId: PartId, plans: Plan[], hooks: MergeHooks): PlanId[] {
    const affectedPlanIds = new Set(this.unapplyPartialSlot(partId));
    if (plans.length === 0) return [...affectedPlanIds];

    const slotLoad = new AbortController();
    for (const plan of plans) {
      const incoming = scopedToPartialSlot(plan, partId);
      const source = new PartialPlanContributionSource(partId, slotLoad.signal);
      const merged = this.mergeContribution(incoming, hooks, source);
      this.slots.recordApplied(partId, slotLoad, incoming);
      affectedPlanIds.add(merged.planId);
    }

    return [...affectedPlanIds];
  }

  unloadPartialSlot(partId: PartId): PlanId[] {
    return this.unapplyPartialSlot(partId);
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

  private unapplyPartialSlot(partId: PartId): PlanId[] {
    const contributions = this.slots.releaseAppliedContributions(partId);
    const affectedPlanIds = new Set<PlanId>();

    for (const contribution of contributions) {
      affectedPlanIds.add(contribution.planId);
      this.removeContribution(contribution);
    }

    return [...affectedPlanIds];
  }

  private removeContribution(contribution: AppliedSlotContribution): void {
    const plan = this.plans.get(contribution.planId)!;
    contribution.abortSlotLoad();
    this.removeBehaviors(plan, contribution);
    const removedLayoutObjectKeys = this.removeLayoutObjects(plan, contribution);
    const removedComponentKeys = this.removeComponents(plan, contribution);
    this.removeValidationRules(plan, contribution);
    this.pruneOrphanedValidationRules(
      plan,
      contribution,
      new Set([...removedComponentKeys, ...removedLayoutObjectKeys]));
    this.removeTypes(plan, contribution);

    if (this.canPruneMergedPlan(contribution.planId, plan)) {
      this.plans.delete(contribution.planId);
    }
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
      assertComponentCanMerge(
        target,
        { planId: incoming.planId, key, component: comp, source },
        { ownership: this.componentOwnership, layoutObjects: this.layoutObjects },
      );
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
      target.types[key] = mergeJsTypes(target.types[key], type);
      this.typeOwnership.claim(incoming.planId, key, type, source);
    }
  }

  private mergeComponents(incoming: Plan, target: Plan, source: PlanContributionSource): void {
    for (const [key, comp] of Object.entries(incoming.components)) {
      mergeComponentIntoPlan(
        target,
        { planId: incoming.planId, key, component: comp, source },
        { ownership: this.componentOwnership, layoutObjects: this.layoutObjects },
      );
    }
  }

  private claimRootKeys(plan: Plan): void {
    for (const [key, type] of Object.entries(plan.types)) {
      this.typeOwnership.claimRoot(plan.planId, key, type);
    }
    for (const key of Object.keys(plan.components)) {
      this.componentOwnership.claimRoot(plan.planId, key);
    }
  }

  private removeBehaviors(plan: Plan, contribution: AppliedSlotContribution): void {
    for (const behavior of contribution.behaviors) {
      const idx = plan.behaviors.indexOf(behavior);
      plan.behaviors.splice(idx, 1);
    }
  }

  private removeLayoutObjects(plan: Plan, contribution: AppliedSlotContribution): Set<string> {
    const removed = new Set<string>();
    for (const key of contribution.layoutObjectKeys) {
      if (!this.layoutObjects.releaseMaterializedBy(contribution.planId, key, contribution.partId)) continue;

      const component = plan.components[key];
      if (component) unwireField(component.id);
      delete plan.components[key];
      this.componentOwnership.release(contribution.planId, key);
      removed.add(key);
    }

    return removed;
  }

  private removeComponents(plan: Plan, contribution: AppliedSlotContribution): Set<string> {
    const removed = new Set<string>();
    for (const key of contribution.componentKeys) {
      if (!this.componentOwnership.isOwnedBy(contribution.planId, key, contribution.partId)) continue;

      const component = plan.components[key];
      if (component) unwireField(component.id);
      delete plan.components[key];
      this.componentOwnership.release(contribution.planId, key);
      removed.add(key);
    }

    return removed;
  }

  private removeValidationRules(plan: Plan, contribution: AppliedSlotContribution): void {
    for (const validationRuleContribution of contribution.validationRuleContributions) {
      validationRuleContribution.removeFrom(plan);
    }
  }

  private pruneOrphanedValidationRules(
    plan: Plan,
    contribution: AppliedSlotContribution,
    removedKeys: Set<string>,
  ): void {
    if (removedKeys.size === 0) return;

    for (const [componentKey, component] of Object.entries(plan.components)) {
      const validationRules = validationRulesOf(component);
      if (validationRules === undefined) continue;
      if (!this.componentOwnership.isOwnedBy(contribution.planId, componentKey, contribution.partId)) continue;
      validationRules.removeRulesForComponents(removedKeys);
    }
  }

  private removeTypes(plan: Plan, contribution: AppliedSlotContribution): void {
    for (const key of contribution.typeKeys) {
      const remainingContract = this.typeOwnership.releasePartial(contribution.planId, key, contribution.partId);
      if (remainingContract === undefined) {
        delete plan.types[key];
        continue;
      }

      plan.types[key] = remainingContract.toJsType();
    }
  }

  private canPruneMergedPlan(planId: PlanId, plan: Plan): boolean {
    const planWasNotBootedAsRoot = !this.rootPlanIds.has(planId);
    const planHasNoBehaviors = plan.behaviors.length === 0;
    const planHasNoComponents = Object.keys(plan.components).length === 0;
    const planHasNoTypes = Object.keys(plan.types).length === 0;

    return planWasNotBootedAsRoot && planHasNoBehaviors && planHasNoComponents && planHasNoTypes;
  }
}

function scopedToPartialSlot(plan: Plan, partId: PartId): Plan {
  return {
    ...plan,
    scope: { kind: "partial", partId },
  };
}

const browserPlans = new AppliedBrowserPlans();

export function composeInitialPlans(plans: Plan[]): Plan[] {
  return InitialPlanComposition.from(plans).bootPlans();
}

export function registerBootedPlan(plan: Plan): void { browserPlans.register(plan); }
export function applyPartialSlotLoad(partId: PartId, plans: Plan[], hooks: MergeHooks): PlanId[] {
  return browserPlans.loadPartialSlot(partId, plans, hooks);
}
export function applyPartialSlotUnload(partId: PartId): PlanId[] {
  return browserPlans.unloadPartialSlot(partId);
}
export function getBootedPlan(planId: string): Plan | undefined { return browserPlans.get(planId); }
export function resetMergePlanState(): void { browserPlans.reset(); }
