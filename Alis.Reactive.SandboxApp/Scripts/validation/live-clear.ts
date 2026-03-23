// Live Validation — Per-field event wiring for interactive validation
//
// After the first form submit shows errors:
//   input  → clear the error (user is typing, positive feedback)
//   blur   → re-validate this field (user left the field, check if fix is valid)
//   change → re-validate this field (radio/checkbox/select/SF — selection IS the action)
//
// Standard pattern: jQuery Unobtrusive, React Hook Form, Angular Forms.
// Vendor-agnostic: uses resolveRoot() for both native and fusion components.
// Tracks wired fields by ID to prevent double-wiring on partial merge.

import type { ValidationDescriptor, ValidationField } from "../types";
import { resolveRoot } from "../resolution/component";
import { clearInline } from "./error-display";
import { revalidateField } from "./orchestrator";
import { scope } from "../core/trace";

const log = scope("live-clear");

/** Set of fieldIds already wired — prevents double-wiring on partial reload. */
const wiredFields = new Set<string>();

export function wireLiveValidation(desc: ValidationDescriptor): void {
  for (const field of desc.fields) {
    wireField(desc, field);
  }
}

function wireField(desc: ValidationDescriptor, field: ValidationField): void {
  // Only wire enriched fields — unenriched fields have no component to listen on
  if (!field.fieldId || !field.vendor) return;

  // Already wired — skip (partial reload dedup)
  if (wiredFields.has(field.fieldId)) return;
  wiredFields.add(field.fieldId);

  const el = document.getElementById(field.fieldId);
  if (!el) return; // Element not in DOM yet (lazy partial)

  const clearHandler = () => clearInline(desc.formId, field);
  const revalidateHandler = () => revalidateField(desc, field);

  if (field.vendor === "native") {
    // input → clear error (typing feedback)
    // blur → re-validate (left the field)
    // change → re-validate (radio/checkbox/select — selection is the action)
    el.addEventListener("input", clearHandler);
    el.addEventListener("blur", revalidateHandler);
    el.addEventListener("change", revalidateHandler);
  } else {
    // Fusion (and future vendors): listen on the vendor root
    // SF ej2 instances implement addEventListener for their callbacks
    // SF "change" fires when user makes a selection — equivalent to blur
    const root = resolveRoot(el, field.vendor);
    (root as EventTarget).addEventListener("change", revalidateHandler);
  }
}

/** Remove field IDs from the wired set — called when a source is removed during partial reload. */
export function unwireFields(fieldIds: string[]): void {
  for (const id of fieldIds) wiredFields.delete(id);
  if (fieldIds.length > 0) log.debug("unwired", { count: fieldIds.length, fieldIds });
}

/** Reset for tests — clears the wired set so tests start clean. */
export function resetLiveClearForTests(): void {
  wiredFields.clear();
}
