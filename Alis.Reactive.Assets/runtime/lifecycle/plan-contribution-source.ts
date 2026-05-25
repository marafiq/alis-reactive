import type { Plan } from "../types";

export type PlanId = string;
export type PartId = string;

export const rootOwnerId = "$root";

export type PlanContributionSource = RootPlanContributionSource | PartialPlanContributionSource;

export function planContributionSourceFrom(plan: Plan): PlanContributionSource {
  const scope = plan.scope;
  if (scope.kind === "partial") return new PartialPlanContributionSource(scope.partId);

  return RootPlanContributionSource.instance;
}

export class RootPlanContributionSource {
  static readonly instance = new RootPlanContributionSource();

  readonly kind = "root";
  readonly label = "root";
  readonly description = "root plan contribution";
  readonly behaviorSignal: AbortSignal | undefined = undefined;

  private constructor() {}
}

export class PartialPlanContributionSource {
  readonly kind = "partial";

  constructor(
    readonly partId: PartId,
    readonly behaviorSignal: AbortSignal | undefined = undefined,
  ) {}

  get label(): string {
    return this.partId;
  }

  get description(): string {
    return `partial plan contribution "${this.partId}"`;
  }
}
