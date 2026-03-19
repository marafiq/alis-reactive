import type { ComponentEntry, Entry, Plan } from "../types";

type EnrichEntries = (entries: Entry[], components: Record<string, ComponentEntry>) => void;
type WireEntries = (entries: Entry[], components: Record<string, ComponentEntry>, signal?: AbortSignal) => void;

export interface MergeHooks {
  enrichEntries: EnrichEntries;
  wireEntries: WireEntries;
}

export class PlanRegistry {
  private readonly plans = new Map<string, Plan>();
  private readonly rootPlanIds = new Set<string>();
  private readonly sourceOwners = new Map<string, string>();
  private readonly abortControllers = new Map<string, AbortController>();
  private readonly sourceEntries = new Map<string, Entry[]>();
  private readonly sourceComponentKeys = new Map<string, string[]>();

  register(plan: Plan): void {
    this.plans.set(plan.planId, plan);
    this.rootPlanIds.add(plan.planId);
  }

  add(incoming: Plan, hooks: MergeHooks): Plan {
    const sourceId = incoming.sourceId;
    const previousPlanId = sourceId ? this.sourceOwners.get(sourceId) : undefined;

    if (sourceId && previousPlanId) {
      this.removeSource(previousPlanId, sourceId);
    }

    let target = this.plans.get(incoming.planId);
    if (!target) {
      target = { planId: incoming.planId, components: {}, entries: [] };
      this.plans.set(incoming.planId, target);
    }

    Object.assign(target.components, incoming.components);

    if (target.entries.length > 0) {
      hooks.enrichEntries(target.entries, target.components);
    }

    const abort = sourceId ? new AbortController() : undefined;
    hooks.enrichEntries(incoming.entries, target.components);
    hooks.wireEntries(incoming.entries, target.components, abort?.signal);
    target.entries.push(...incoming.entries);

    if (sourceId && abort) {
      this.sourceOwners.set(sourceId, incoming.planId);
      this.abortControllers.set(sourceId, abort);
      this.sourceEntries.set(sourceId, [...incoming.entries]);
      this.sourceComponentKeys.set(sourceId, Object.keys(incoming.components));
    }

    return target;
  }

  get(planId: string): Plan | undefined {
    return this.plans.get(planId);
  }

  reset(): void {
    this.plans.clear();
    this.rootPlanIds.clear();
    this.sourceOwners.clear();
    for (const abort of this.abortControllers.values()) abort.abort();
    this.abortControllers.clear();
    this.sourceEntries.clear();
    this.sourceComponentKeys.clear();
  }

  private removeSource(planId: string, sourceId: string): void {
    const plan = this.plans.get(planId);
    if (!plan) {
      this.clearTracking(sourceId);
      return;
    }

    this.abortControllers.get(sourceId)?.abort();

    const oldEntries = this.sourceEntries.get(sourceId);
    if (oldEntries) {
      for (const entry of oldEntries) {
        const idx = plan.entries.indexOf(entry);
        if (idx >= 0) plan.entries.splice(idx, 1);
      }
    }

    const oldKeys = this.sourceComponentKeys.get(sourceId);
    if (oldKeys) {
      for (const key of oldKeys) delete plan.components[key];
    }

    this.clearTracking(sourceId);

    if (!this.rootPlanIds.has(planId) && plan.entries.length === 0 && Object.keys(plan.components).length === 0) {
      this.plans.delete(planId);
    }
  }

  private clearTracking(sourceId: string): void {
    this.sourceOwners.delete(sourceId);
    this.abortControllers.delete(sourceId);
    this.sourceEntries.delete(sourceId);
    this.sourceComponentKeys.delete(sourceId);
  }
}

// ── Singleton + delegating exports (backward-compatible API) ──

const registry = new PlanRegistry();

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

export function registerBootedPlan(plan: Plan): void { registry.register(plan); }
export function applyMergedPlan(incoming: Plan, hooks: MergeHooks): Plan { return registry.add(incoming, hooks); }
export function getBootedPlan(planId: string): Plan | undefined { return registry.get(planId); }
export function resetMergePlanState(): void { registry.reset(); }
