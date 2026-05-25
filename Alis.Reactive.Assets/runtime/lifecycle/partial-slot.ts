import type { Behavior, Plan } from "../types";
import {
  captureValidationRuleContributions,
  layoutObjectKeysFrom,
  type ValidationRuleContribution,
} from "./component-contribution";
import { type PartId, type PlanId } from "./plan-contribution-source";

export class AppliedSlotContribution {
  private constructor(
    readonly partId: PartId,
    readonly planId: PlanId,
    private readonly slotLoad: AbortController,
    readonly behaviors: Behavior[],
    readonly componentKeys: string[],
    readonly layoutObjectKeys: string[],
    readonly typeKeys: string[],
    readonly validationRuleContributions: ValidationRuleContribution[],
  ) {}

  static capture(partId: PartId, slotLoad: AbortController, incoming: Plan): AppliedSlotContribution {
    return new AppliedSlotContribution(
      partId,
      incoming.planId,
      slotLoad,
      [...incoming.behaviors],
      Object.keys(incoming.components),
      layoutObjectKeysFrom(incoming),
      Object.keys(incoming.types),
      captureValidationRuleContributions(incoming),
    );
  }

  abortSlotLoad(): void {
    this.slotLoad.abort();
  }
}

export class AppliedPartialSlots {
  private readonly slots = new Map<PartId, AppliedSlotContribution[]>();

  recordApplied(partId: PartId, slotLoad: AbortController, incoming: Plan): void {
    const contributions = this.slots.get(partId) ?? [];
    contributions.push(AppliedSlotContribution.capture(partId, slotLoad, incoming));
    this.slots.set(partId, contributions);
  }

  releaseAppliedContributions(partId: PartId): AppliedSlotContribution[] {
    const contributions = this.slots.get(partId) ?? [];
    this.slots.delete(partId);
    return [...contributions];
  }

  reset(): void {
    for (const contributions of this.slots.values()) {
      for (const contribution of contributions) contribution.abortSlotLoad();
    }
    this.slots.clear();
  }
}
