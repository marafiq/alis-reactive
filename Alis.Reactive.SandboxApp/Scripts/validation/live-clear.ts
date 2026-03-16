// Live Clear — Wires input/change events to auto-clear field errors
//
// Single responsibility: one-time event wiring per form container.
// Delegates actual clearing to error-display module.

import type { ValidationDescriptor } from "../types";
import { clearInline } from "./error-display";

export function wireLiveClearing(desc: ValidationDescriptor): void {
  const container = document.getElementById(desc.formId);
  if (!container || container.dataset.alisValidated) return;
  container.dataset.alisValidated = "true";

  const handler = (e: Event) => {
    const target = e.target as HTMLElement;
    const field = desc.fields.find(f => f.fieldId === target.id);
    if (field) clearInline(desc.formId, field);
  };

  container.addEventListener("input", handler);
  container.addEventListener("change", handler);
}
