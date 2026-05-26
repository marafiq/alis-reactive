// merge-plan.ts — applied browser plans and partial slot replacement.

import type { Plan, Behavior } from "../types";
import { unwireField } from "../validation/live-clear";
import {
  ComponentOwnership,
  LayoutObjectReferences,
  captureValidationRuleContributions,
  composeInitialComponentIntoPlan,
  layoutObjectKeysFrom,
  mergeComponentIntoPlan,
  removeValidationRuleContribution,
  type ValidationRuleContribution,
} from "./component-contribution";
import { BrowserObjectContracts, mergeJsTypes } from "./object-contract-fragment";
import {
  planContributionSourceFrom,
  type ContributionId,
  partialPlanContribution,
  type PlanContributionSource,
  type PlanId,
} from "./plan-contribution-source";

type WireBehaviors = (behaviors: Behavior[], plan: Plan, signal?: AbortSignal) => void;
type WireContainerValidation = (plan: Plan, signal?: AbortSignal) => void;

export interface MergeHooks {
  wireBehaviors: WireBehaviors;
  wireContainerValidation: WireContainerValidation;
}

interface AppliedSlotContribution {
  readonly slotId: ContributionId;
  readonly planId: PlanId;
  readonly slotLoad: AbortController;
  readonly behaviors: Behavior[];
  readonly componentKeys: string[];
  readonly layoutObjectKeys: string[];
  readonly typeKeys: string[];
  readonly validationRuleContributions: ValidationRuleContribution[];
}

class BootPlanAssembly {
  private readonly componentOwnership = new ComponentOwnership();
  private readonly layoutObjects = new LayoutObjectReferences();

  private constructor(private readonly plan: Plan) {}

  static forPlanId(planId: PlanId): BootPlanAssembly {
    return new BootPlanAssembly({
      version: 3,
      planId,
      scope: { kind: "root" },
      types: {},
      components: {},
      behaviors: [],
    });
  }

  accept(contribution: Plan): void {
    const source = planContributionSourceFrom(contribution);

    for (const [key, type] of Object.entries(contribution.types)) {
      this.plan.types[key] = mergeJsTypes(this.plan.types[key], type);
    }

    this.mergeComponents(contribution, source);
    this.plan.behaviors.push(...contribution.behaviors);
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

  toPlan(): Plan {
    return this.plan;
  }
}

export class AppliedBrowserPlans {
  private readonly plans = new Map<string, Plan>();
  private readonly rootPlanIds = new Set<string>();
  private readonly partialSlots = new Map<ContributionId, AppliedSlotContribution[]>();
  private readonly componentOwnership = new ComponentOwnership();
  private readonly layoutObjects = new LayoutObjectReferences();
  private readonly typeOwnership = new BrowserObjectContracts();

  register(plan: Plan): void {
    this.plans.set(plan.planId, plan);
    this.rootPlanIds.add(plan.planId);
    this.recordRootKeys(plan);
  }

  loadPartialSlot(slotId: ContributionId, plans: Plan[], hooks: MergeHooks): PlanId[] {
    const affectedPlanIds = new Set(this.unapplyPartialSlot(slotId));
    if (plans.length === 0) return [...affectedPlanIds];

    const slotLoad = new AbortController();
    for (const plan of plans) {
      const source = partialPlanContribution(slotId, slotLoad.signal);
      const merged = this.mergeContribution(plan, hooks, source);
      this.recordAppliedPartialSlot(slotId, slotLoad, plan);
      affectedPlanIds.add(merged.planId);
    }

    return [...affectedPlanIds];
  }

  unloadPartialSlot(slotId: ContributionId): PlanId[] {
    return this.unapplyPartialSlot(slotId);
  }

  get(planId: string): Plan | undefined {
    return this.plans.get(planId);
  }

  reset(): void {
    this.plans.clear();
    this.rootPlanIds.clear();
    this.abortPartialSlotLoads();
    this.partialSlots.clear();
    this.componentOwnership.reset();
    this.layoutObjects.reset();
    this.typeOwnership.reset();
  }

  private unapplyPartialSlot(slotId: ContributionId): PlanId[] {
    const contributions = this.releaseAppliedPartialSlot(slotId);
    const affectedPlanIds = new Set<PlanId>();

    for (const contribution of contributions) {
      affectedPlanIds.add(contribution.planId);
      this.removeContribution(contribution);
    }

    return [...affectedPlanIds];
  }

  private removeContribution(contribution: AppliedSlotContribution): void {
    const plan = this.plans.get(contribution.planId)!;
    contribution.slotLoad.abort();
    this.removeBehaviors(plan, contribution);
    this.removeLayoutObjects(plan, contribution);
    this.removeComponents(plan, contribution);
    this.removeValidationRules(plan, contribution);
    this.removeTypes(plan, contribution);

    if (this.canPruneMergedPlan(contribution.planId, plan)) {
      this.plans.delete(contribution.planId);
    }
  }

  private mergeContribution(incoming: Plan, hooks: MergeHooks, source: PlanContributionSource): Plan {
    const target = this.ensureTarget(incoming.planId);
    this.mergeTypes(incoming, target, source);
    this.mergeComponents(incoming, target, source);

    hooks.wireBehaviors(incoming.behaviors, target, source.behaviorSignal);
    target.behaviors.push(...incoming.behaviors);
    hooks.wireContainerValidation(target, source.behaviorSignal);

    return target;
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
      this.typeOwnership.record(incoming.planId, key, type, source);
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

  private recordRootKeys(plan: Plan): void {
    for (const [key, type] of Object.entries(plan.types)) {
      this.typeOwnership.recordRoot(plan.planId, key, type);
    }
    for (const key of Object.keys(plan.components)) {
      this.componentOwnership.recordRoot(plan.planId, key);
    }
  }

  private removeBehaviors(plan: Plan, contribution: AppliedSlotContribution): void {
    const removed = new Set(contribution.behaviors);
    plan.behaviors = plan.behaviors.filter(behavior => !removed.has(behavior));
  }

  private removeLayoutObjects(plan: Plan, contribution: AppliedSlotContribution): void {
    for (const key of contribution.layoutObjectKeys) {
      if (!this.layoutObjects.releaseMaterializedBy(contribution.planId, key, contribution.slotId)) continue;

      const component = plan.components[key];
      if (component) unwireField(component.id);
      delete plan.components[key];
      this.componentOwnership.release(contribution.planId, key);
    }
  }

  private removeComponents(plan: Plan, contribution: AppliedSlotContribution): void {
    for (const key of contribution.componentKeys) {
      if (!this.componentOwnership.isOwnedBy(contribution.planId, key, contribution.slotId)) continue;

      const component = plan.components[key];
      if (component) unwireField(component.id);
      delete plan.components[key];
      this.componentOwnership.release(contribution.planId, key);
    }
  }

  private removeValidationRules(plan: Plan, contribution: AppliedSlotContribution): void {
    for (const validationRuleContribution of contribution.validationRuleContributions) {
      removeValidationRuleContribution(plan, validationRuleContribution);
    }
  }

  private removeTypes(plan: Plan, contribution: AppliedSlotContribution): void {
    for (const key of contribution.typeKeys) {
      const remainingContract = this.typeOwnership.releasePartial(contribution.planId, key, contribution.slotId);
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

  private recordAppliedPartialSlot(slotId: ContributionId, slotLoad: AbortController, incoming: Plan): void {
    const contributions = this.partialSlots.get(slotId) ?? [];
    contributions.push(captureAppliedSlotContribution(slotId, slotLoad, incoming));
    this.partialSlots.set(slotId, contributions);
  }

  private releaseAppliedPartialSlot(slotId: ContributionId): AppliedSlotContribution[] {
    const contributions = this.partialSlots.get(slotId) ?? [];
    this.partialSlots.delete(slotId);
    return [...contributions];
  }

  private abortPartialSlotLoads(): void {
    for (const contributions of this.partialSlots.values()) {
      for (const contribution of contributions) contribution.slotLoad.abort();
    }
  }
}

function captureAppliedSlotContribution(
  slotId: ContributionId,
  slotLoad: AbortController,
  incoming: Plan,
): AppliedSlotContribution {
  return {
    slotId,
    planId: incoming.planId,
    slotLoad,
    behaviors: [...incoming.behaviors],
    componentKeys: Object.keys(incoming.components),
    layoutObjectKeys: layoutObjectKeysFrom(incoming),
    typeKeys: Object.keys(incoming.types),
    validationRuleContributions: captureValidationRuleContributions(incoming),
  };
}

const browserPlans = new AppliedBrowserPlans();

export function composeInitialPlans(plans: Plan[]): Plan[] {
  const assemblies = new Map<PlanId, BootPlanAssembly>();
  for (const plan of plans) {
    let assembly = assemblies.get(plan.planId);
    if (assembly === undefined) {
      assembly = BootPlanAssembly.forPlanId(plan.planId);
      assemblies.set(plan.planId, assembly);
    }

    assembly.accept(plan);
  }

  return Array.from(assemblies.values()).map(assembly => assembly.toPlan());
}

export function registerBootedPlan(plan: Plan): void { browserPlans.register(plan); }
export function applyPartialSlotLoad(slotId: ContributionId, plans: Plan[], hooks: MergeHooks): PlanId[] {
  return browserPlans.loadPartialSlot(slotId, plans, hooks);
}
export function applyPartialSlotUnload(slotId: ContributionId): PlanId[] {
  return browserPlans.unloadPartialSlot(slotId);
}
export function getBootedPlan(planId: string): Plan | undefined { return browserPlans.get(planId); }
export function resetMergePlanState(): void { browserPlans.reset(); }
