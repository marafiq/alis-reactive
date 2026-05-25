// merge-plan.ts — applied browser plans and partial slot replacement.

import type { Plan, Behavior } from "../types";
import {
  ComponentOwnershipLedger,
  LayoutObjectReferenceLedger,
  assertComponentCanComposeInitialPlan,
  assertComponentCanMerge,
  composeInitialComponentIntoPlan,
  mergeComponentIntoPlan,
} from "./component-contribution";
import { BrowserObjectContractLedger, mergeJsTypes } from "./object-contract-fragment";
import { AppliedSlotContributionRemoval } from "./applied-slot-contribution-removal";
import {
  planContributionSourceFrom,
  type PartId,
  PartialPlanContributionSource,
  type PlanContributionSource,
  type PlanId,
} from "./plan-contribution-source";
import { AppliedPartialSlots } from "./partial-slot";

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
  private readonly contributionRemoval = new AppliedSlotContributionRemoval(
    this.plans,
    this.rootPlanIds,
    this.componentOwnership,
    this.layoutObjects,
    this.typeOwnership,
  );

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
      this.contributionRemoval.remove(contribution);
    }

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
      const claim = this.typeOwnership.request(incoming.planId, key, type);
      if (!claim.canBeHeldBy(source)) throw claim.collisionError(source);
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
