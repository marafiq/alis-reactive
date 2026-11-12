import { plugin } from "../Plugins/index";
import { assertNever, type Listening } from "../Schema/index";
import { Stop, type Read } from "../ValuesAndSources/index";
import { isAnObject, type Context } from "./context";
import { evaluate } from "./expressions";

export interface Located {
  readonly object: unknown;
  readonly listen: Listening;
}

export function located(context: Context, id: string): Located {
  const locator = context.plan.objects[id];
  if (locator === undefined) throw new Error("The plan names the object '" + id + "', which it does not declare.");
  switch (locator.kind) {
    case "element": {
      const vendor = context.plan.vendors[locator.vendor];
      if (vendor === undefined) throw new Error("The plan names the vendor '" + locator.vendor + "', which it does not declare.");
      return { object: heldOn(elementOf(locator.id), locator.vendor, evaluate(vendor.locate, context)), listen: vendor.listen };
    }
    case "plugin":
      return { object: plugin(locator.name, locator.key), listen: locator.listen };
    default:
      return assertNever(locator);
  }
}

function elementOf(id: string): HTMLElement {
  const element = document.getElementById(id);
  if (element === null) throw new Stop("No element has the id '" + id + "'.");
  return element;
}

function heldOn(element: HTMLElement, vendor: string, find: Read): object {
  if (find.kind === "nothing" || typeof find.value !== "function") throw new Error("The vendor '" + vendor + "' locates its object by no function.");
  const found: unknown = Reflect.apply(find.value, undefined, [element]);
  if (!isAnObject(found)) throw new Stop("The element '" + element.id + "' holds no object of the vendor '" + vendor + "'.");
  return found;
}
