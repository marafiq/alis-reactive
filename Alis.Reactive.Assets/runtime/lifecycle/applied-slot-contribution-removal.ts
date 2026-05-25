import type { Plan } from "../types";
import { unwireField } from "../validation/live-clear";
import {
  ComponentOwnershipLedger,
  LayoutObjectReferenceLedger,
  validationRulesOf,
} from "./component-contribution";
import { BrowserObjectContractLedger } from "./object-contract-fragment";
import { type AppliedSlotContribution } from "./partial-slot";
import { type PlanId } from "./plan-contribution-source";

export class AppliedSlotContributionRemoval {
  constructor(
    private readonly plans: Map<PlanId, Plan>,
    private readonly rootPlanIds: ReadonlySet<PlanId>,
    private readonly componentOwnership: ComponentOwnershipLedger,
    private readonly layoutObjects: LayoutObjectReferenceLedger,
    private readonly typeOwnership: BrowserObjectContractLedger,
  ) {}

  remove(contribution: AppliedSlotContribution): void {
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
      const comp = plan.components[key];
      if (comp) unwireField(comp.id);
      delete plan.components[key];
      this.componentOwnership.release(contribution.planId, key);
      removed.add(key);
    }
    return removed;
  }

  private pruneOrphanedValidationRules(
    plan: Plan,
    contribution: AppliedSlotContribution,
    removedKeys: Set<string>,
  ): void {
    if (removedKeys.size === 0) return;
    for (const [compKey, comp] of Object.entries(plan.components)) {
      const validationRules = validationRulesOf(comp);
      if (validationRules === undefined) continue;
      if (!this.componentOwnership.isOwnedBy(contribution.planId, compKey, contribution.partId)) continue;
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

  private removeValidationRules(plan: Plan, contribution: AppliedSlotContribution): void {
    for (const validationRuleContribution of contribution.validationRuleContributions) {
      validationRuleContribution.removeFrom(plan);
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
