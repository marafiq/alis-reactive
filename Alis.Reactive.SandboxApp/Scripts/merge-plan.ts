import type { ComponentEntry, Entry, Plan } from "./types";

type EnrichEntries = (entries: Entry[], components: Record<string, ComponentEntry>) => void;
type WireEntries = (entries: Entry[], components: Record<string, ComponentEntry>, signal?: AbortSignal) => void;

interface MergeHooks {
  enrichEntries: EnrichEntries;
  wireEntries: WireEntries;
}

const bootedPlans = new Map<string, Plan>();
const rootPlanIds = new Set<string>();
const sourceOwners = new Map<string, string>();
const mergedAbort = new Map<string, AbortController>();
const mergedEntries = new Map<string, Entry[]>();
const mergedComponentKeys = new Map<string, string[]>();

export function composeInitialPlans(plans: Plan[]): Plan[] {
  const byPlanId = new Map<string, Plan>();

  for (const plan of plans) {
    const existing = byPlanId.get(plan.planId);
    if (!existing) {
      byPlanId.set(plan.planId, {
        planId: plan.planId,
        components: { ...plan.components },
        entries: [...plan.entries],
      });
      continue;
    }

    Object.assign(existing.components, plan.components);
    existing.entries.push(...plan.entries);
  }

  return Array.from(byPlanId.values());
}

export function registerBootedPlan(plan: Plan): void {
  bootedPlans.set(plan.planId, plan);
  rootPlanIds.add(plan.planId);
}

export function applyMergedPlan(incoming: Plan, hooks: MergeHooks): Plan {
  const sourceId = incoming.sourceId;
  const previousPlanId = sourceId ? sourceOwners.get(sourceId) : undefined;

  if (sourceId && previousPlanId) {
    removeSourceContribution(previousPlanId, sourceId);
  }

  let targetPlan = bootedPlans.get(incoming.planId);
  if (!targetPlan) {
    targetPlan = {
      planId: incoming.planId,
      components: {},
      entries: [],
    };
    bootedPlans.set(incoming.planId, targetPlan);
  }

  Object.assign(targetPlan.components, incoming.components);

  if (targetPlan.entries.length > 0) {
    hooks.enrichEntries(targetPlan.entries, targetPlan.components);
  }

  const abort = sourceId ? new AbortController() : undefined;
  hooks.enrichEntries(incoming.entries, targetPlan.components);
  hooks.wireEntries(incoming.entries, targetPlan.components, abort?.signal);
  targetPlan.entries.push(...incoming.entries);

  if (sourceId && abort) {
    sourceOwners.set(sourceId, incoming.planId);
    mergedAbort.set(sourceId, abort);
    mergedEntries.set(sourceId, [...incoming.entries]);
    mergedComponentKeys.set(sourceId, Object.keys(incoming.components));
  }

  return targetPlan;
}

export function getBootedPlan(planId: string): Plan | undefined {
  return bootedPlans.get(planId);
}

export function resetMergePlanState(): void {
  bootedPlans.clear();
  rootPlanIds.clear();
  sourceOwners.clear();

  for (const abort of mergedAbort.values()) {
    abort.abort();
  }

  mergedAbort.clear();
  mergedEntries.clear();
  mergedComponentKeys.clear();
}

function removeSourceContribution(planId: string, sourceId: string): void {
  const plan = bootedPlans.get(planId);
  if (!plan) {
    clearSourceTracking(sourceId);
    return;
  }

  const oldAbort = mergedAbort.get(sourceId);
  if (oldAbort) {
    oldAbort.abort();
  }

  const oldEntries = mergedEntries.get(sourceId);
  if (oldEntries) {
    for (const oldEntry of oldEntries) {
      const index = plan.entries.indexOf(oldEntry);
      if (index >= 0) {
        plan.entries.splice(index, 1);
      }
    }
  }

  const oldKeys = mergedComponentKeys.get(sourceId);
  if (oldKeys) {
    for (const key of oldKeys) {
      delete plan.components[key];
    }
  }

  clearSourceTracking(sourceId);

  if (!rootPlanIds.has(planId) && plan.entries.length === 0 && Object.keys(plan.components).length === 0) {
    bootedPlans.delete(planId);
  }
}

function clearSourceTracking(sourceId: string): void {
  sourceOwners.delete(sourceId);
  mergedAbort.delete(sourceId);
  mergedEntries.delete(sourceId);
  mergedComponentKeys.delete(sourceId);
}
