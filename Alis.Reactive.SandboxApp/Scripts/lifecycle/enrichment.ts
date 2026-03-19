// Enrichment — Enriches validation fields from plan.components.
// No DOM, no wiring, no side effects beyond mutation of validation field objects.

import type { Entry, ComponentEntry, ValidationDescriptor } from "../types";
import { scope } from "../core/trace";
import { walkValidationDescriptors } from "./walk-reactions";

const log = scope("enrichment");

export function enrichEntries(entries: Entry[], components: Record<string, ComponentEntry>): void {
  walkValidationDescriptors(entries, desc => enrichValidationFields(desc, components));
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
      if (f.fieldId) {
        log.warn("clearing enrichment — component removed", { fieldName: f.fieldName });
      }
      f.fieldId = undefined;
      f.vendor = undefined;
      f.readExpr = undefined;
    }
  }
}
