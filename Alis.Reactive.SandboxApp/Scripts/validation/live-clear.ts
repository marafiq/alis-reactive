// Live Clear — Per-field event wiring to auto-clear validation errors
//
// Vendor-agnostic: uses resolveRoot() for both native and fusion components.
// Native: listens for "input" + "change" on the DOM element.
// Fusion: listens for "change" on the ej2 instance (SF callback, not DOM event).
// Tracks wired fields by ID to prevent double-wiring on partial merge.

import type { ValidationDescriptor, ValidationField } from "../types";
import { resolveRoot } from "../resolution/component";
import { clearInline } from "./error-display";

/** Set of fieldIds already wired — prevents double-wiring on partial reload. */
const wiredFields = new Set<string>();

export function wireLiveClearing(desc: ValidationDescriptor): void {
  for (const field of desc.fields) {
    wireField(desc.formId, field);
  }
}

function wireField(formId: string, field: ValidationField): void {
  // Only wire enriched fields — unenriched fields have no component to listen on
  if (!field.fieldId || !field.vendor) return;

  // Already wired — skip (partial reload dedup)
  if (wiredFields.has(field.fieldId)) return;
  wiredFields.add(field.fieldId);

  const el = document.getElementById(field.fieldId);
  if (!el) return; // Element not in DOM yet (lazy partial)

  const clearHandler = () => clearInline(formId, field);

  if (field.vendor === "native") {
    // Native DOM elements fire standard input/change events
    el.addEventListener("input", clearHandler);
    el.addEventListener("change", clearHandler);
  } else {
    // Fusion (and future vendors): listen on the vendor root
    // SF ej2 instances implement addEventListener for their callbacks
    const root = resolveRoot(el, field.vendor);
    (root as EventTarget).addEventListener("change", clearHandler);
  }
}

/** Reset for tests — clears the wired set so tests start clean. */
export function resetLiveClearForTests(): void {
  wiredFields.clear();
}
