// The located objects of one plan: an element through its vendor's data, or a plugin through the
// plugin registered under its name (grammar.md 1.1). Elements are looked up at every use and never
// cached, so a slot's content can change underneath a plan (grammar.md E1).

import type { ElementLocator, ObjectLocator, PlanDocument, PluginLocator, Vendor } from "../types/index";
import type { Objects } from "../grammar/evaluate";
import { read } from "../grammar/paths";
import { assertNever } from "../shared/assert-never";
import { pluginNamed } from "./plugins";
import type { Owner } from "./owner";
import { subscribeVendor, vendorObject, type Listener } from "./vendor-driver";

export class PlanObjects implements Objects {
  constructor(readonly plan: PlanDocument) {}

  object(id: string): unknown {
    const locator = this.locator(id);
    switch (locator.kind) {
      case "plugin":  return pluginNamed(locator.name).object(locator.key);
      case "element": return vendorObject(this.requireElement(locator), locator.vendor, this.vendor(locator));
      default:        return assertNever(locator, "object locator");
    }
  }

  /** The element of an input, or null when it is not on the page. An input is always an element. */
  element(id: string): HTMLElement | null {
    return document.getElementById((this.locator(id) as ElementLocator).id);
  }

  /** Subscribes on the object, or on the object at `path` under it: a nested object's own event. */
  subscribe(id: string, path: readonly string[], event: string, listener: Listener, owner: Owner): void {
    const locator = this.locator(id);
    switch (locator.kind) {
      case "plugin":  return subscribePlugin(locator, event, listener, owner);
      case "element": return this.subscribeElement(locator, path, event, listener, owner);
      default:        return assertNever(locator, "object locator");
    }
  }

  private subscribeElement(locator: ElementLocator, path: readonly string[], event: string, listener: Listener, owner: Owner): void {
    const vendor = this.vendor(locator);
    const root = vendorObject(this.requireElement(locator), locator.vendor, vendor);
    const target = path.length === 0 ? root : read(root, path, "checked", `#${locator.id}`);
    owner.onDispose(subscribeVendor(target, locator.vendor, vendor, event, listener));
  }

  // The plan declares every object and vendor its handlers and expressions name (the C# object table).
  private locator(id: string): ObjectLocator {
    return this.plan.objects[id] as ObjectLocator;
  }

  private vendor(locator: ElementLocator): Vendor {
    return this.plan.vendors[locator.vendor] as Vendor;
  }

  private requireElement(locator: ElementLocator): HTMLElement {
    const element = document.getElementById(locator.id);
    if (!element) throw new Error(`[alis] #${locator.id} is not on the page`);
    return element;
  }
}

// A handler subscribes to the plugin itself, never to an object it holds (the C# refuses one), and only
// the framework's own plugins raise events: alis.document, alis.stream and alis.hub, each with subscribe.
function subscribePlugin(locator: PluginLocator, event: string, listener: Listener, owner: Owner): void {
  pluginNamed(locator.name).subscribe!(locator.key, event, listener, owner);
}
