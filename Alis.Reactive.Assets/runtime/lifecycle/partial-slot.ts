import type { Behavior, Plan } from "../types";
import { layoutObjectKeysFrom, ValidationRuleContribution } from "./component-contribution";
import {
  type PartId,
  PartialListenerLifetime,
  PartialPlanContributionSource,
  type PlanId,
} from "./plan-contribution-source";

export class PartialSlotLoad {
  private readonly listenerLifetime = PartialListenerLifetime.create();

  private constructor(
    readonly partId: PartId,
    private readonly plans: Plan[],
  ) {}

  static containing(partId: PartId, plans: Plan[]): PartialSlotLoad {
    return new PartialSlotLoad(partId, plans);
  }

  contributions(): PartialSlotContribution[] {
    return this.plans.map(plan => new PartialSlotContribution(
      this.scopedPlan(plan),
      new PartialPlanContributionSource(this.partId, this.listenerLifetime),
    ));
  }

  private scopedPlan(plan: Plan): Plan {
    return {
      ...plan,
      scope: { kind: "partial", partId: this.partId },
    };
  }
}

export class PartialSlotContribution {
  constructor(
    readonly plan: Plan,
    readonly source: PartialPlanContributionSource,
  ) {}
}

interface AppliedSlotContributionSnapshot {
  readonly partId: PartId;
  readonly planId: PlanId;
  readonly listenerLifetime: PartialListenerLifetime;
  readonly behaviors: Behavior[];
  readonly componentKeys: string[];
  readonly layoutObjectKeys: string[];
  readonly typeKeys: string[];
  readonly validationRuleContributions: ValidationRuleContribution[];
}

export class AppliedSlotContribution {
  private constructor(private readonly snapshot: AppliedSlotContributionSnapshot) {}

  static capture(source: PartialPlanContributionSource, incoming: Plan): AppliedSlotContribution {
    return new AppliedSlotContribution({
      partId: source.partId,
      planId: incoming.planId,
      listenerLifetime: source.listenerLifetime,
      behaviors: [...incoming.behaviors],
      componentKeys: Object.keys(incoming.components),
      layoutObjectKeys: layoutObjectKeysFrom(incoming),
      typeKeys: Object.keys(incoming.types),
      validationRuleContributions: ValidationRuleContribution.captureFrom(incoming),
    });
  }

  get partId(): PartId {
    return this.snapshot.partId;
  }

  get planId(): PlanId {
    return this.snapshot.planId;
  }

  get behaviors(): Behavior[] {
    return this.snapshot.behaviors;
  }

  get componentKeys(): string[] {
    return this.snapshot.componentKeys;
  }

  get layoutObjectKeys(): string[] {
    return this.snapshot.layoutObjectKeys;
  }

  get typeKeys(): string[] {
    return this.snapshot.typeKeys;
  }

  get validationRuleContributions(): ValidationRuleContribution[] {
    return this.snapshot.validationRuleContributions;
  }

  revokeListenerLifetime(): void {
    this.snapshot.listenerLifetime.revoke();
  }
}

export class PartialSlotRegistry {
  private readonly slots = new Map<PartId, AppliedPartialSlot>();

  recordApplied(source: PartialPlanContributionSource, incoming: Plan): void {
    this.slotFor(source.partId).record(AppliedSlotContribution.capture(source, incoming));
  }

  releaseAppliedContributions(partId: PartId): AppliedSlotContribution[] {
    const contributions = this.slots.get(partId)?.contributions() ?? [];
    this.slots.delete(partId);
    return contributions;
  }

  reset(): void {
    for (const slot of this.slots.values()) slot.revokeListenerLifetimes();
    this.slots.clear();
  }

  private slotFor(partId: PartId): AppliedPartialSlot {
    let slot = this.slots.get(partId);
    if (slot === undefined) {
      slot = new AppliedPartialSlot();
      this.slots.set(partId, slot);
    }

    return slot;
  }
}

class AppliedPartialSlot {
  private readonly appliedContributions: AppliedSlotContribution[] = [];

  record(contribution: AppliedSlotContribution): void {
    this.appliedContributions.push(contribution);
  }

  contributions(): AppliedSlotContribution[] {
    return [...this.appliedContributions];
  }

  revokeListenerLifetimes(): void {
    for (const contribution of this.appliedContributions) contribution.revokeListenerLifetime();
  }
}
