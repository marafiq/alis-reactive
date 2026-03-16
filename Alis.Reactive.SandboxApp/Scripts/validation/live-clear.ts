// Live Clear — Wires input/change events to auto-clear field errors
//
// Single responsibility: one-time event wiring per form container.
// Delegates actual clearing to error-display module.
// Vendor-agnostic: Html.Field() wraps every input + error span in a div.
// The event bubbles from the input (native or fusion inner) through the
// field wrapper which contains data-valmsg-for. We match by finding
// the error span in the target's ancestry and looking up the field by name.

import type { ValidationDescriptor } from "../types";
import { clearInline } from "./error-display";

export function wireLiveClearing(desc: ValidationDescriptor): void {
  const container = document.getElementById(desc.formId);
  if (!container || container.dataset.alisValidated) return;
  container.dataset.alisValidated = "true";

  // Build name→field lookup
  const byName = new Map(desc.fields.map(f => [f.fieldName, f]));

  const handler = (e: Event) => {
    const target = e.target as HTMLElement;

    // Walk up from target to find the field wrapper's error span.
    // Html.Field renders: <div> <label/> <input/> <span data-valmsg-for="Name"/> </div>
    // The input and span are siblings inside the wrapper div.
    let node: HTMLElement | null = target.parentElement;
    while (node && node !== container) {
      const span = node.querySelector<HTMLElement>("[data-valmsg-for]");
      if (span) {
        const fieldName = span.getAttribute("data-valmsg-for");
        if (fieldName) {
          const field = byName.get(fieldName);
          if (field) {
            clearInline(desc.formId, field);
            return;
          }
        }
      }
      node = node.parentElement;
    }
  };

  container.addEventListener("input", handler);
  container.addEventListener("change", handler);
}
