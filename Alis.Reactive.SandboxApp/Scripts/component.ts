import type { Vendor } from "./types";
import { walk } from "./walk";
import { scope } from "./trace";

const log = scope("component");

/**
 * Resolves the vendor-specific root object for a component.
 *
 * This is the ONLY place in the runtime that maps vendor -> root.
 * All component interactions (reads, events) use this.
 *
 *   "native" -> el (the DOM element)
 *   "fusion" -> el.ej2_instances[0] (the Syncfusion component instance)
 */
export function resolveRoot(el: any, vendor: Vendor): unknown {
  switch (vendor) {
    case "native":
      return el;
    case "fusion":
      return el.ej2_instances?.[0];
  }
}

/**
 * Reads a component value: resolveRoot + dot-path walk.
 *
 * readExpr is a property path from the vendor-determined root.
 * Examples: "checked", "value", "selectedItems.0.text"
 */
export function evalRead(id: string, vendor: Vendor, readExpr: string): unknown {
  const el = document.getElementById(id) as any;
  if (!el) return undefined;

  const root = resolveRoot(el, vendor);
  if (root == null) {
    log.warn("no root", { id, vendor });
    return undefined;
  }

  return walk(root, readExpr);
}
