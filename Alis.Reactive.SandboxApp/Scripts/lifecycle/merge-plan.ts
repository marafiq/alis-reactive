// merge-plan.ts — Plan registry and merge logic.
// Plans: types, components, behaviors. Merge logic for partial plan injection.

import type { Plan, Behavior } from "../types";
import { unwireField } from "../validation/live-clear";
import { scope } from "../core/trace";

const log = scope("merge");

type WireBehaviors = (behaviors: Behavior[], plan: Plan, signal?: AbortSignal) => void;
type WireContainerValidation = (plan: Plan) => void;

export interface MergeHooks {
  wireBehaviors: WireBehaviors;
  wireContainerValidation: WireContainerValidation;
}

export class PlanRegistry {
  private readonly plans = new Map<string, Plan>();
  private readonly rootPlanIds = new Set<string>();
  private readonly sourceOwners = new Map<string, string>();
  private readonly abortControllers = new Map<string, AbortController>();
  private readonly sourceBehaviors = new Map<string, Behavior[]>();
  private readonly sourceComponentKeys = new Map<string, string[]>();
  private readonly sourceTypeKeys = new Map<string, string[]>();
  // Per-key ownership: tracks which partId owns each component/type key.
  // Prevents cross-source collisions and ownership-aware unmerge.
  private readonly keyOwners = new Map<string, string>();

  register(plan: Plan): void {
    this.plans.set(plan.planId, plan);
    this.rootPlanIds.add(plan.planId);
  }

  add(incoming: Plan, hooks: MergeHooks): Plan {
    const partId = incoming.partId;
    const previousPlanId = partId ? this.sourceOwners.get(partId) : undefined;

    if (partId && previousPlanId) {
      this.removeSource(previousPlanId, partId);
    }

    let target = this.plans.get(incoming.planId);
    if (!target) {
      target = {
        version: 3,
        planId: incoming.planId,
        types: {},
        components: {},
        behaviors: [],
      };
      this.plans.set(incoming.planId, target);
    }

    // Merge types — ownership-aware: block cross-source collisions
    for (const [key, type] of Object.entries(incoming.types)) {
      const owner = this.keyOwners.get(`t:${key}`);
      if (owner && owner !== partId && partId) {
        log.error("cross-source type collision", { key, owner, incoming: partId });
        continue;
      }
      target.types[key] = type;
      if (partId) this.keyOwners.set(`t:${key}`, partId);
    }

    // Merge components — ownership-aware with deep-merge for validation rules.
    for (const [key, comp] of Object.entries(incoming.components)) {
      const owner = this.keyOwners.get(`c:${key}`);
      if (owner && owner !== partId && partId) {
        log.error("cross-source component collision", { key, owner, incoming: partId });
        continue;
      }
      const existing = target.components[key];
      if (existing?.container?.validationRules && comp.container?.validationRules) {
        const ruleMap = new Map(existing.container.validationRules.map(cv => [cv.component, cv]));
        for (const cv of comp.container.validationRules) ruleMap.set(cv.component, cv);
        comp.container.validationRules = [...ruleMap.values()];
      }
      target.components[key] = comp;
      if (partId) this.keyOwners.set(`c:${key}`, partId);
    }

    // Wire and merge behaviors
    const abort = partId ? new AbortController() : undefined;
    hooks.wireBehaviors(incoming.behaviors, target, abort?.signal);
    target.behaviors.push(...incoming.behaviors);

    // Wire container validation for new components
    hooks.wireContainerValidation(target);

    if (partId && abort) {
      this.sourceOwners.set(partId, incoming.planId);
      this.abortControllers.set(partId, abort);
      this.sourceBehaviors.set(partId, [...incoming.behaviors]);
      this.sourceComponentKeys.set(partId, Object.keys(incoming.components));
      this.sourceTypeKeys.set(partId, Object.keys(incoming.types));
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
    this.sourceBehaviors.clear();
    this.sourceComponentKeys.clear();
    this.sourceTypeKeys.clear();
    this.keyOwners.clear();
  }

  private removeSource(planId: string, partId: string): void {
    const plan = this.plans.get(planId);
    if (!plan) {
      this.clearTracking(partId);
      return;
    }

    this.abortControllers.get(partId)?.abort();

    // Remove behaviors from this source
    const oldBehaviors = this.sourceBehaviors.get(partId);
    if (oldBehaviors) {
      for (const behavior of oldBehaviors) {
        const idx = plan.behaviors.indexOf(behavior);
        if (idx >= 0) plan.behaviors.splice(idx, 1);
      }
    }

    // Remove components owned by this source. Ownership check prevents
    // deleting keys that were taken over by a different source.
    const removedComponentKeys = new Set<string>();
    const oldKeys = this.sourceComponentKeys.get(partId);
    if (oldKeys) {
      for (const key of oldKeys) {
        if (this.keyOwners.get(`c:${key}`) !== partId) continue;
        const comp = plan.components[key];
        if (comp) unwireField(comp.id);
        delete plan.components[key];
        this.keyOwners.delete(`c:${key}`);
        removedComponentKeys.add(key);
      }
    }

    // Remove orphaned validation rules for deleted components — but ONLY
    // from containers that this source also owns. If the container belongs
    // to the root plan, its rules were set at C# build time and the partial
    // doesn't re-supply them on re-merge.
    if (removedComponentKeys.size > 0) {
      for (const [compKey, comp] of Object.entries(plan.components)) {
        if (!comp.container?.validationRules) continue;
        const containerOwner = this.keyOwners.get(`c:${compKey}`);
        if (containerOwner !== partId) continue;  // Not our container — don't touch its rules
        comp.container.validationRules = comp.container.validationRules
          .filter(cv => !removedComponentKeys.has(cv.component));
      }
    }

    // Remove types owned by this source
    const oldTypeKeys = this.sourceTypeKeys.get(partId);
    if (oldTypeKeys) {
      for (const key of oldTypeKeys) {
        if (this.keyOwners.get(`t:${key}`) !== partId) continue;
        delete plan.types[key];
        this.keyOwners.delete(`t:${key}`);
      }
    }

    this.clearTracking(partId);

    if (!this.rootPlanIds.has(planId) && plan.behaviors.length === 0 && Object.keys(plan.components).length === 0) {
      this.plans.delete(planId);
    }
  }

  private clearTracking(partId: string): void {
    this.sourceOwners.delete(partId);
    this.abortControllers.delete(partId);
    this.sourceBehaviors.delete(partId);
    this.sourceComponentKeys.delete(partId);
    this.sourceTypeKeys.delete(partId);
  }
}

// -- Singleton + delegating exports --

const registry = new PlanRegistry();

export function composeInitialPlans(plans: Plan[]): Plan[] {
  const byPlanId = new Map<string, Plan>();
  for (const plan of plans) {
    const existing = byPlanId.get(plan.planId);
    if (!existing) {
      byPlanId.set(plan.planId, {
        version: 3,
        planId: plan.planId,
        types: { ...plan.types },
        components: { ...plan.components },
        behaviors: [...plan.behaviors],
      });
      continue;
    }
    Object.assign(existing.types, plan.types);
    // Deep-merge components — same logic as PlanRegistry.add()
    for (const [key, comp] of Object.entries(plan.components)) {
      const prev = existing.components[key];
      if (prev?.container?.validationRules && comp.container?.validationRules) {
        const ruleMap = new Map(prev.container.validationRules.map(cv => [cv.component, cv]));
        for (const cv of comp.container.validationRules) ruleMap.set(cv.component, cv);
        comp.container.validationRules = [...ruleMap.values()];
      }
      existing.components[key] = comp;
    }
    existing.behaviors.push(...plan.behaviors);
  }
  return Array.from(byPlanId.values());
}

export function registerBootedPlan(plan: Plan): void { registry.register(plan); }
export function applyMergedPlan(incoming: Plan, hooks: MergeHooks): Plan { return registry.add(incoming, hooks); }
export function getBootedPlan(planId: string): Plan | undefined { return registry.get(planId); }
export function resetMergePlanState(): void { registry.reset(); }
