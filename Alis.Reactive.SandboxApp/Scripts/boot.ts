import type { Plan, Entry, ComponentEntry, Reaction, ValidationDescriptor } from "./types";
import { setLevel } from "./trace";
import { scope } from "./trace";
import { wireTrigger } from "./trigger";

const log = scope("boot");

export function boot(plan: Plan): void {
  log.info("booting", { entries: plan.entries.length });

  // Enrich validation fields from plan.components before wiring triggers
  enrichValidation(plan);

  // Two-phase boot: wire all listeners first, then execute dom-ready reactions.
  // This ensures custom-event listeners exist before dom-ready dispatches into them.
  const deferred: Entry[] = [];

  for (const entry of plan.entries) {
    if (entry.trigger.kind === "dom-ready") {
      deferred.push(entry);
    } else {
      wireTrigger(entry.trigger, entry.reaction, plan.components);
    }
  }

  for (const entry of deferred) {
    wireTrigger(entry.trigger, entry.reaction, plan.components);
  }

  log.info("booted");
}

export const trace = { setLevel };

// -- Validation enrichment -----------------------------------------------

/** Walk the reaction tree and enrich validation fields from plan.components. */
function enrichValidation(plan: Plan): void {
  for (const entry of plan.entries) {
    enrichReaction(entry.reaction, plan.components);
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
    }
  }
}
