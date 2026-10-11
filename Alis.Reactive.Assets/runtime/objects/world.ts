// The located objects of one plan: an element through its vendor's data, or a plugin through the
// module registered under its name (grammar.md 1.1). Elements are looked up at every use and never
// cached, so a slot's content can change underneath a plan (grammar.md E1).

import type { ElementLocator, ObjectLocator, PlanDocument } from "../types/index";
import type { World } from "../grammar/evaluate";
import { moduleNamed } from "./modules";
import type { Owner } from "./owner";
import { subscribeVendor, vendorObject, type Listener } from "./vendor-driver";

export class PlanWorld implements World {
  constructor(readonly plan: PlanDocument) {}

  object(id: string): unknown {
    const locator = this.locator(id);
    if (locator.kind === "plugin") return moduleNamed(locator.name).object(locator.key);
    return vendorObject(this.requireElement(locator), locator.vendor, this.vendor(locator));
  }

  /** The element behind an element locator, or null when it is not on the page. */
  element(id: string): HTMLElement | null {
    const locator = this.locator(id);
    return locator.kind === "element" ? document.getElementById(locator.id) : null;
  }

  subscribe(id: string, event: string, listener: Listener, owner: Owner): void {
    const locator = this.locator(id);
    if (locator.kind === "plugin") {
      const module = moduleNamed(locator.name);
      if (!module.subscribe) throw new Error(`[alis] plugin "${locator.name}" has no events`);
      module.subscribe(locator.key, event, listener, owner);
      return;
    }

    const vendor = this.vendor(locator);
    const root = vendorObject(this.requireElement(locator), locator.vendor, vendor);
    owner.onDispose(subscribeVendor(root, locator.vendor, vendor, event, listener));
  }

  private locator(id: string): ObjectLocator {
    const locator = this.plan.objects[id];
    if (!locator) throw new Error(`[alis] plan "${this.plan.planId}" has no object "${id}"`);
    return locator;
  }

  private vendor(locator: ElementLocator) {
    const vendor = this.plan.vendors[locator.vendor];
    if (!vendor) throw new Error(`[alis] plan "${this.plan.planId}" has no vendor "${locator.vendor}"`);
    return vendor;
  }

  private requireElement(locator: ElementLocator): HTMLElement {
    const element = document.getElementById(locator.id);
    if (!element) throw new Error(`[alis] #${locator.id} is not on the page`);
    return element;
  }
}
