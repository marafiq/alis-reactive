import type { ComponentEntry, Entry, Plan } from "../types";

type EnrichEntries = (entries: Entry[], components: Record<string, ComponentEntry>) => void;
type WireEntries = (entries: Entry[], components: Record<string, ComponentEntry>, signal?: AbortSignal) => void;
type UnwireFields = (fieldIds: string[]) => void;

export interface MergeHooks {
  enrichEntries: EnrichEntries;
  wireEntries: WireEntries;
  unwireFields: UnwireFields;
}

export class PlanRegistry {
  private readonly plans = new Map<string, Plan>();
  private readonly rootPlanIds = new Set<string>();
  private readonly abortControllers = new Map<string, AbortController>();
  private readonly sourceEntries = new Map<string, Entry[]>();
  private readonly sourceComponentKeys = new Map<string, string[]>();

  register(plan: Plan): void {
    this.plans.set(plan.planId, plan);
    this.rootPlanIds.add(plan.planId);
  }

  add(incoming: Plan, hooks: MergeHooks): Plan {
    const sourceId = incoming.sourceId;

    if (sourceId) {
      this.removeSource(incoming.planId, sourceId, hooks.unwireFields);
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
      const key = sourceKey(incoming.planId, sourceId);
      this.abortControllers.set(key, abort);
      this.sourceEntries.set(key, [...incoming.entries]);
      this.sourceComponentKeys.set(key, Object.keys(incoming.components));
    }

    return target;
  }

  get(planId: string): Plan | undefined {
    return this.plans.get(planId);
  }

  reset(): void {
    this.plans.clear();
    this.rootPlanIds.clear();
    for (const abort of this.abortControllers.values()) abort.abort();
    this.abortControllers.clear();
    this.sourceEntries.clear();
    this.sourceComponentKeys.clear();
  }

  private removeSource(planId: string, sourceId: string, unwireFields: UnwireFields): void {
    const key = sourceKey(planId, sourceId);
    const plan = this.plans.get(planId);
    if (!plan) {
      this.clearTracking(key);
      return;
    }

    this.abortControllers.get(key)?.abort();

    const oldEntries = this.sourceEntries.get(key);
    if (oldEntries) {
      for (const entry of oldEntries) {
        const idx = plan.entries.indexOf(entry);
        if (idx >= 0) plan.entries.splice(idx, 1);
      }
    }

    const oldKeys = this.sourceComponentKeys.get(key);
    if (oldKeys) {
      // Unwire live-clear for components being removed — their element IDs
      // must be cleared from wiredFields so re-loaded partials get fresh wiring.
      const fieldIds = oldKeys.map(key => plan.components[key]?.id).filter((id): id is string => !!id);
      if (fieldIds.length > 0) unwireFields(fieldIds);

      for (const key of oldKeys) delete plan.components[key];
    }

    this.clearTracking(key);

    if (!this.rootPlanIds.has(planId) && plan.entries.length === 0 && Object.keys(plan.components).length === 0) {
      this.plans.delete(planId);
    }
  }

  private clearTracking(key: string): void {
    this.abortControllers.delete(key);
    this.sourceEntries.delete(key);
    this.sourceComponentKeys.delete(key);
  }
}

function sourceKey(planId: string, sourceId: string): string {
  return `${planId}::${sourceId}`;
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
