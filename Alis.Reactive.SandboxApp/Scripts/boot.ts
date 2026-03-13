import type { Plan, Entry, ComponentEntry, Reaction, ValidationDescriptor } from "./types";
import { setLevel } from "./trace";
import { scope } from "./trace";
import { wireTrigger } from "./trigger";

const log = scope("boot");

/** Booted plans keyed by planId — used by mergePlan for AJAX partial injection. */
const bootedPlans = new Map<string, Plan>();

/** AbortControllers for merged partials — keyed by sourceId. Aborted on re-merge to remove old listeners. */
const mergedAbort = new Map<string, AbortController>();

/** Entries added via mergePlan — keyed by sourceId. Removed on re-merge before adding new ones. */
const mergedEntries = new Map<string, Entry[]>();

export function boot(plan: Plan): void {
  log.info("booting", { entries: plan.entries.length });

  enrichEntries(plan.entries, plan.components);
  wireEntries(plan.entries, plan.components);

  bootedPlans.set(plan.planId, plan);
  log.info("booted");
}

/**
 * Two-phase wiring: wire all non-dom-ready listeners first, then execute dom-ready.
 * This ensures custom-event listeners exist before dom-ready dispatches into them.
 */
function wireEntries(entries: Entry[], components: Record<string, ComponentEntry>, signal?: AbortSignal): void {
  const deferred: Entry[] = [];
  for (const entry of entries) {
    if (entry.trigger.kind === "dom-ready") {
      deferred.push(entry);
    } else {
      wireTrigger(entry.trigger, entry.reaction, components, signal);
    }
  }
  for (const entry of deferred) {
    wireTrigger(entry.trigger, entry.reaction, components, signal);
  }
}

/**
 * Merge an AJAX-injected partial plan into an already-booted plan.
 * Merges components, enriches validation, wires new triggers (two-phase).
 *
 * When the incoming plan has a sourceId (set by inject.ts from container ID),
 * re-merging the same source aborts old listeners and replaces old entries,
 * preventing accumulation on partial reload.
 */
export function mergePlan(incoming: Plan): void {
  const existing = bootedPlans.get(incoming.planId);
  if (existing) {
    Object.assign(existing.components, incoming.components);
    log.info("merge", { planId: incoming.planId, newComponents: Object.keys(incoming.components).length });
  }

  // Use merged components for enrichment and wiring
  const components = existing?.components ?? incoming.components;

  // Clean up previous merge from same source (prevents listener/entry accumulation)
  const sourceId = incoming.sourceId;
  if (sourceId) {
    const oldAbort = mergedAbort.get(sourceId);
    if (oldAbort) {
      oldAbort.abort();
      log.info("re-merge: aborted old listeners", { sourceId });
    }

    const oldEntries = mergedEntries.get(sourceId);
    if (oldEntries && existing) {
      for (const old of oldEntries) {
        const idx = existing.entries.indexOf(old);
        if (idx >= 0) existing.entries.splice(idx, 1);
      }
    }
  }

  // Create new abort controller for this merge
  const abort = sourceId ? new AbortController() : undefined;
  if (sourceId && abort) {
    mergedAbort.set(sourceId, abort);
  }

  // Re-enrich existing entries — parent plan may have validation descriptors
  // that reference components only now available from the merged partial.
  if (existing) {
    enrichEntries(existing.entries, components);
  }
  enrichEntries(incoming.entries, components);
  wireEntries(incoming.entries, components, abort?.signal);

  if (existing) {
    existing.entries.push(...incoming.entries);
  } else {
    bootedPlans.set(incoming.planId, incoming);
  }

  // Track merged entries for cleanup on re-merge
  if (sourceId) {
    mergedEntries.set(sourceId, [...incoming.entries]);
  }
}

export const trace = { setLevel };

// -- Validation enrichment -----------------------------------------------

/** Walk entries and enrich validation fields from plan.components. */
function enrichEntries(entries: Entry[], components: Record<string, ComponentEntry>): void {
  for (const entry of entries) {
    enrichReaction(entry.reaction, components);
  }
}

function enrichReaction(reaction: Reaction, components: Record<string, ComponentEntry>): void {
  switch (reaction.kind) {
    case "http":
      enrichRequest(reaction.request, components);
      break;
    case "parallel-http":
      for (const req of reaction.requests) {
        enrichRequest(req, components);
      }
      break;
    case "conditional":
      for (const branch of reaction.branches) {
        enrichReaction(branch.reaction, components);
      }
      break;
  }
}

function enrichRequest(
  req: { validation?: ValidationDescriptor; chained?: typeof req },
  components: Record<string, ComponentEntry>
): void {
  if (req.validation) {
    enrichValidationFields(req.validation, components);
  }
  if (req.chained) {
    enrichRequest(req.chained, components);
  }
}

function enrichValidationFields(
  desc: ValidationDescriptor,
  components: Record<string, ComponentEntry>
): void {
  for (const f of desc.fields) {
    const comp = components[f.fieldName];
    if (comp) {
      f.fieldId = comp.id;
      f.vendor = comp.vendor;
      f.readExpr = comp.readExpr;
    } else {
      log.warn("validation field not in components", { fieldName: f.fieldName });
    }
  }
}
