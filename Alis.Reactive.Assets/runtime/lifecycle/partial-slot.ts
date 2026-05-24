import type { Behavior, Plan } from "../types";
import { layoutObjectKeysFrom, ValidationRuleContribution } from "./component-contribution";
import { type PartId, PartialPlanContributionSource, type PlanId } from "./plan-contribution-source";

class PartialSlotLifetime {
  private readonly abort = new AbortController();

  sourceFor(partId: PartId): PartialPlanContributionSource {
    return new PartialPlanContributionSource(partId, this.abort);
  }
}

export class PartialSlotLoad {
  private readonly lifetime = new PartialSlotLifetime();

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
      this.lifetime.sourceFor(this.partId),
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

interface TrackedPartialPlanSnapshot {
  readonly partId: PartId;
  readonly planId: PlanId;
  readonly abort: AbortController;
  readonly behaviors: Behavior[];
  readonly componentKeys: string[];
  readonly layoutObjectKeys: string[];
  readonly typeKeys: string[];
  readonly validationRuleContributions: ValidationRuleContribution[];
}

export class TrackedPartialPlan {
  private constructor(private readonly snapshot: TrackedPartialPlanSnapshot) {}

  static capture(source: PartialPlanContributionSource, incoming: Plan): TrackedPartialPlan {
    return new TrackedPartialPlan({
      partId: source.partId,
      planId: incoming.planId,
      abort: source.abortController,
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

  abortWiredBehaviors(): void {
    this.snapshot.abort.abort();
  }
}

export class PartialSlotRegistry {
  private readonly slots = new Map<PartId, TrackedPartialSlot>();

  contributions(partId: PartId): TrackedPartialPlan[] {
    return this.slots.get(partId)?.contributions() ?? [];
  }

  track(source: PartialPlanContributionSource, incoming: Plan): void {
    this.slotFor(source.partId).track(TrackedPartialPlan.capture(source, incoming));
  }

  clear(partId: PartId): void {
    this.slots.delete(partId);
  }

  reset(): void {
    for (const slot of this.slots.values()) slot.abortWiredBehaviors();
    this.slots.clear();
  }

  private slotFor(partId: PartId): TrackedPartialSlot {
    let slot = this.slots.get(partId);
    if (slot === undefined) {
      slot = new TrackedPartialSlot();
      this.slots.set(partId, slot);
    }

    return slot;
  }
}

class TrackedPartialSlot {
  private readonly trackedContributions: TrackedPartialPlan[] = [];

  track(contribution: TrackedPartialPlan): void {
    this.trackedContributions.push(contribution);
  }

  contributions(): TrackedPartialPlan[] {
    return [...this.trackedContributions];
  }

  abortWiredBehaviors(): void {
    for (const contribution of this.trackedContributions) contribution.abortWiredBehaviors();
  }
}
