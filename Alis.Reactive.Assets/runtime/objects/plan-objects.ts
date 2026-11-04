import type { ElementObjectLocator, ObjectLocator, PlanDocument, PluginObjectLocator, Vendor } from "../types/index";
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
      case "plugin":  return pluginNamed(locator.name).object(locator.key, locator.data);
      case "element": return vendorObject(this.requireElement(locator), locator.vendor, this.vendor(locator));
      default:        return assertNever(locator, "object locator");
    }
  }

  element(id: string): HTMLElement | null {
    return document.getElementById((this.locator(id) as ElementObjectLocator).id);
  }

  subscribe(id: string, path: readonly string[], event: string, listener: Listener, owner: Owner): void {
    const locator = this.locator(id);
    switch (locator.kind) {
      case "plugin":  return subscribePlugin(locator, event, listener, owner);
      case "element": return this.subscribeElement(locator, path, event, listener, owner);
      default:        return assertNever(locator, "object locator");
    }
  }

  private subscribeElement(locator: ElementObjectLocator, path: readonly string[], event: string, listener: Listener, owner: Owner): void {
    const vendor = this.vendor(locator);
    const root = vendorObject(this.requireElement(locator), locator.vendor, vendor);
    const target = path.length === 0 ? root : read(root, path, "required", `#${locator.id}`);
    owner.onDispose(subscribeVendor(target, locator.vendor, vendor, event, listener));
  }

  private locator(id: string): ObjectLocator {
    return this.plan.objects[id] as ObjectLocator;
  }

  private vendor(locator: ElementObjectLocator): Vendor {
    return this.plan.vendors[locator.vendor] as Vendor;
  }

  private requireElement(locator: ElementObjectLocator): HTMLElement {
    const element = document.getElementById(locator.id);
    if (!element) throw new Error(`[alis] #${locator.id} is not on the page`);
    return element;
  }
}

function subscribePlugin(locator: PluginObjectLocator, event: string, listener: Listener, owner: Owner): void {
  pluginNamed(locator.name).subscribe!(locator.key, event, listener, owner);
}
