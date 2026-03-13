import type { Plan, Entry, ComponentEntry, Reaction, ValidationDescriptor } from "./types";
import { setLevel } from "./trace";
import { scope } from "./trace";
import { wireTrigger } from "./trigger";

const log = scope("boot");

/** Booted plans keyed by planId — used by mergePlan for AJAX partial injection. */
const bootedPlans = new Map<string, Plan>();

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
function wireEntries(entries: Entry[], components: Record<string, ComponentEntry>): void {
  const deferred: Entry[] = [];
  for (const entry of entries) {
    if (entry.trigger.kind === "dom-ready") {
      deferred.push(entry);
    } else {
      wireTrigger(entry.trigger, entry.reaction, components);
    }
  }
  for (const entry of deferred) {
    wireTrigger(entry.trigger, entry.reaction, components);
  }
}

/**
 * Merge an AJAX-injected partial plan into an already-booted plan.
 * Merges components, enriches validation, wires new triggers (two-phase).
 */
export function mergePlan(incoming: Plan): void {
  const existing = bootedPlans.get(incoming.planId);
  if (existing) {
    Object.assign(existing.components, incoming.components);
    log.info("merge", { planId: incoming.planId, newComponents: Object.keys(incoming.components).length });
  }

  // Use merged components for enrichment and wiring
  const components = existing?.components ?? incoming.components;

  // Re-enrich existing entries — parent plan may have validation descriptors
  // that reference components only now available from the merged partial.
  if (existing) {
    enrichEntries(existing.entries, components);
  }
  enrichEntries(incoming.entries, components);
  wireEntries(incoming.entries, components);

  if (existing) {
    existing.entries.push(...incoming.entries);
  } else {
    bootedPlans.set(incoming.planId, incoming);
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
