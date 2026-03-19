import type { Vendor } from "../types";
import { walk } from "../core/walk";
import { scope } from "../core/trace";

const _log = scope("component");

/**
 * Resolves the vendor-specific root object for a component.
 *
 * This is the ONLY place in the runtime that maps vendor -> root.
 * All component interactions (reads, events) use this.
 *
 *   "native" -> el (the DOM element)
 *   "fusion" -> el.ej2_instances[0] (the Syncfusion component instance)
 */
export function resolveRoot(el: HTMLElement, vendor: Vendor): unknown {
  switch (vendor) {
    case "native":
      return el;
    case "fusion": {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any -- ej2_instances is a Syncfusion runtime property not on HTMLElement
      const root = (el as any).ej2_instances?.[0];
      if (root == null) throw new Error(`[alis] no vendor root for "${el.id}" (vendor: ${vendor}) — is the component initialized?`);
      return root;
    }
    default:
      throw new Error(`[alis] unknown vendor: "${vendor}"`);
  }
}

/**
 * Reads a component value: resolveRoot + dot-path walk.
 *
 * readExpr is a property path from the vendor-determined root.
 * Examples: "checked", "value", "selectedItems.0.text"
 */
export function evalRead(id: string, vendor: Vendor, readExpr: string): unknown {
  const el = document.getElementById(id);
  if (!el) throw new Error(`[alis] element not found: ${id}`);

  const root = resolveRoot(el, vendor);
  return walk(root, readExpr);
}

