import type { Behavior, Plan } from "../types";
import { layoutObjectKeysFrom, ValidationRuleContribution } from "./component-contribution";
import {
  type PartId,
  PartialListenerLifetime,
  PartialPlanContributionSource,
  type PlanId,
} from "./plan-contribution-source";

export class AppliedSlotContribution {
  private constructor(
    readonly partId: PartId,
    readonly planId: PlanId,
    private readonly listenerLifetime: PartialListenerLifetime,
    readonly behaviors: Behavior[],
    readonly componentKeys: string[],
    readonly layoutObjectKeys: string[],
    readonly typeKeys: string[],
    readonly validationRuleContributions: ValidationRuleContribution[],
  ) {}

  static capture(source: PartialPlanContributionSource, incoming: Plan): AppliedSlotContribution {
    return new AppliedSlotContribution(
      source.partId,
      incoming.planId,
      source.listenerLifetime,
      [...incoming.behaviors],
      Object.keys(incoming.components),
      layoutObjectKeysFrom(incoming),
      Object.keys(incoming.types),
      ValidationRuleContribution.captureFrom(incoming),
    );
  }

  revokeListenerLifetime(): void {
    this.listenerLifetime.revoke();
  }
}

export class AppliedPartialSlots {
  private readonly slots = new Map<PartId, AppliedSlotContribution[]>();

  recordApplied(source: PartialPlanContributionSource, incoming: Plan): void {
    const contributions = this.slots.get(source.partId) ?? [];
    contributions.push(AppliedSlotContribution.capture(source, incoming));
    this.slots.set(source.partId, contributions);
  }

  releaseAppliedContributions(partId: PartId): AppliedSlotContribution[] {
    const contributions = this.slots.get(partId) ?? [];
    this.slots.delete(partId);
    return [...contributions];
  }

  reset(): void {
    for (const contributions of this.slots.values()) {
      for (const contribution of contributions) contribution.revokeListenerLifetime();
    }
    this.slots.clear();
  }
}
