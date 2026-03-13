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

import type { Plan } from "./types";
import { mergePlan } from "./boot";

/**
 * Inject HTML into a container, using ej.base.append when available (SF component init).
 * Extracts any <script data-alis-plan> elements first and merges them into the booted plan.
 */
export function injectHtml(container: HTMLElement, html: string): void {
  const temp = document.createElement("div");
  temp.innerHTML = html;

  // Extract plan elements before injection (ej.base.append can't handle script tags with JSON)
  const planEls = temp.querySelectorAll<HTMLElement>("[data-alis-plan]");
  const plans: Plan[] = [];
  for (const el of planEls) {
    plans.push(JSON.parse(el.textContent!));
    el.remove();
  }

  container.innerHTML = "";
  const ej = (globalThis as any).ej;
  if (ej?.base?.append) {
    ej.base.append(Array.from(temp.childNodes), container, true);
  } else {
    container.append(...Array.from(temp.childNodes));
  }

  // Merge extracted plans into booted plans
  for (const plan of plans) {
    mergePlan(plan);
  }
}
