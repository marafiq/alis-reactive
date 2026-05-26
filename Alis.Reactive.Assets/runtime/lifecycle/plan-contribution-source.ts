import type { Plan } from "../types";

export type PlanId = string;
export type ContributionId = string;

export const rootOwnerId = "$root";

export type PlanContributionSource = RootPlanContributionSource | PartialPlanContributionSource;

export function planContributionSourceFrom(plan: Plan): PlanContributionSource {
  const scope = plan.scope;
  if (scope.kind === "partial") return partialPlanContribution(plan.planId);

  return rootPlanContribution;
}

export interface RootPlanContributionSource {
  readonly kind: "root";
  readonly behaviorSignal: AbortSignal | undefined;
}

export interface PartialPlanContributionSource {
  readonly kind: "partial";
  readonly contributionId: ContributionId;
  readonly behaviorSignal: AbortSignal | undefined;
}

const rootPlanContribution: RootPlanContributionSource = {
  kind: "root",
  behaviorSignal: undefined,
};

export function partialPlanContribution(
  contributionId: ContributionId,
  behaviorSignal: AbortSignal | undefined = undefined,
): PartialPlanContributionSource {
  return {
    kind: "partial",
    contributionId,
    behaviorSignal,
  };
}

export function describePlanContribution(source: PlanContributionSource): string {
  if (source.kind === "root") return "root plan contribution";

  return `partial plan contribution "${source.contributionId}"`;
}
