// Enrichment — Pure reaction tree walker
//
// Walks the entry/reaction graph and enriches validation fields
// from plan.components. No DOM, no wiring, no side effects beyond mutation
// of the validation field objects.

import type { Entry, ComponentEntry, Reaction, ValidationDescriptor } from "../types";
import { scope } from "../core/trace";

const log = scope("enrichment");

export function enrichEntries(entries: Entry[], components: Record<string, ComponentEntry>): void {
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
      for (const req of reaction.requests) enrichRequest(req, components);
      break;
    case "conditional":
      for (const branch of reaction.branches) enrichReaction(branch.reaction, components);
      break;
  }
}

function enrichRequest(
  req: { validation?: ValidationDescriptor; chained?: typeof req },
  components: Record<string, ComponentEntry>
): void {
  if (req.validation) enrichValidationFields(req.validation, components);
  if (req.chained) enrichRequest(req.chained, components);
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
      // Only warn when clearing a previously enriched field (partial removed)
      // Not on initial boot where unenriched is expected for partial-owned fields
      if (f.fieldId) {
        log.warn("clearing enrichment — component removed", { fieldName: f.fieldName });
      }
      f.fieldId = undefined;
      f.vendor = undefined;
      f.readExpr = undefined;
    }
  }
}
