import { assertNever, type ElementLocator, type Listening, type Locator, type Vendor } from "../Schema/index";
import { Stop } from "../ValuesAndSources/index";
import { isAnObject, type Context } from "./context";
import { applied } from "./expressions";

export type ElementFor = (id: string) => object | null;
export type PluginFor = (name: string, key: string) => unknown;
export type StillHeld = (object: object) => boolean;

type Locate = (element: object) => unknown;

export type Located =
  | { readonly kind: "element"; readonly object: object; readonly listen: Listening; readonly element: object; readonly vendor: string }
  | { readonly kind: "plugin"; readonly object: unknown; readonly listen: Listening };

export function located(context: Context, id: string, elementFor: ElementFor, pluginFor: PluginFor): Located {
  const locator = locatorOf(context, id);
  switch (locator.kind) {
    case "element": {
      const element = elementOf(locator.id, elementFor);
      return { kind: "element", object: heldOn(element, locator, locateOf(context, locator.vendor)), listen: vendorOf(context, locator.vendor).listen, element, vendor: locator.vendor };
    }
    case "plugin":
      return { kind: "plugin", object: pluginFor(locator.name, locator.key), listen: locator.listen };
    default:
      return assertNever(locator);
  }
}

export function locatorOf(context: Context, id: string): Locator {
  const locator = context.plan.objects[id];
  if (locator === undefined) throw new Error("The plan names the object '" + id + "', which it does not declare.");
  return locator;
}

export function elementById(id: string): HTMLElement | null {
  return document.getElementById(id);
}

export function elementOf(id: string, elementFor: ElementFor): object {
  const element = elementFor(id);
  if (element === null) throw new Stop("No element has the id '" + id + "'.");
  return element;
}

export function heldBy(element: object, vendor: string, context: Context): StillHeld {
  const locate = locateOf(context, vendor);
  const weak = new WeakRef(element);
  return (object) => {
    const held = weak.deref();
    return held !== undefined && holdsAt(held, locate, object);
  };
}

function holdsAt(element: object, locate: Locate, object: object): boolean {
  try {
    return locate(element) === object;
  } catch (stopped) {
    if (!(stopped instanceof Stop)) throw stopped;
    return false;
  }
}

function vendorOf(context: Context, name: string): Vendor {
  const vendor = context.plan.vendors[name];
  if (vendor === undefined) throw new Error("The plan names the vendor '" + name + "', which it does not declare.");
  return vendor;
}

function locateOf(context: Context, vendor: string): Locate {
  const locate = vendorOf(context, vendor).locate;
  const unscoped: Context = { ...context, scope: new Map() };
  return (element) => applied(locate, [element], unscoped);
}

function heldOn(element: object, locator: ElementLocator, locate: Locate): object {
  const found = locate(element);
  if (!isAnObject(found)) throw new Stop("The element '" + locator.id + "' holds no object of the vendor '" + locator.vendor + "'.");
  return found;
}
