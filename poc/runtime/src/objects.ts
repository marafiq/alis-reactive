import type { Handler, Listening, PlanDocument } from "./plan";
import { evaluate, walk, type Bound, type Context } from "./evaluate";
import { execute } from "./execute";
import { assertNever, Stop, value } from "./read";
import type { Owner, Removal } from "./owners";
import { plugin } from "./plugins";

type Listener = (...args: unknown[]) => void;

export function contextOf(plan: PlanDocument): Context {
  const context: Context = { plan, scope: new Map<string, Bound>(), locate: (id) => locate(context, id) };
  return context;
}

function locate(context: Context, id: string): unknown {
  const locator = context.plan.objects[id];
  if (locator === undefined) throw new Error("The plan reads the object '" + id + "', which it does not declare.");
  switch (locator.kind) {
    case "element":
      return onElement(context, locator.id, locator.vendor);
    case "plugin":
      return plugin(locator.name, locator.key);
    default:
      return assertNever(locator);
  }
}

function onElement(context: Context, id: string, vendorName: string): object {
  const element = document.getElementById(id);
  if (element === null) throw new Stop("No element has the id '" + id + "'.");
  const vendor = context.plan.vendors[vendorName];
  if (vendor === undefined) throw new Error("The plan names the vendor '" + vendorName + "', which it does not declare.");
  const find = evaluate(vendor.locate, context);
  if (find.kind === "nothing" || typeof find.value !== "function") throw new Error("The vendor '" + vendorName + "' locates its object by no function.");
  const found: unknown = Reflect.apply(find.value, undefined, [element]);
  if (found === null || found === undefined || (typeof found !== "object" && typeof found !== "function")) {
    throw new Stop("The element '" + id + "' holds no object of the vendor '" + vendorName + "'.");
  }
  return found;
}

export function listen(context: Context, handler: Handler, owner: Owner): void {
  const raiser = walk(value(context.locate(handler.on)), handler.path, "required");
  if (raiser.kind === "nothing" || raiser.value === null || typeof raiser.value !== "object") {
    throw new Stop("The object that raises '" + handler.event + "' is not on the page.");
  }
  let refused = false;
  const listener: Listener = (...args) => {
    if (!refused) run(context, handler, args);
  };
  try {
    owner.holds(subscribe(raiser.value, listeningOf(context, handler.on), handler.event, listener));
  } catch (stopped) {
    refused = true;
    throw stopped;
  }
}

function listeningOf(context: Context, id: string): Listening {
  const locator = context.plan.objects[id];
  if (locator === undefined) throw new Error("The plan listens on the object '" + id + "', which it does not declare.");
  switch (locator.kind) {
    case "element": {
      const vendor = context.plan.vendors[locator.vendor];
      if (vendor === undefined) throw new Error("The plan names the vendor '" + locator.vendor + "', which it does not declare.");
      return vendor.listen;
    }
    case "plugin":
      return locator.listen;
    default:
      return assertNever(locator);
  }
}

function subscribe(raiser: object, listening: Listening, event: string, listener: Listener): Removal {
  switch (listening.kind) {
    case "method": {
      const add = method(raiser, listening.subscribe);
      const remove = method(raiser, listening.unsubscribe);
      Reflect.apply(add, raiser, [event, listener]);
      return () => Reflect.apply(remove, raiser, [event, listener]);
    }
    case "assignment": {
      const replaced: unknown = Reflect.get(raiser, event);
      Reflect.set(raiser, event, listener);
      return () => {
        if (Reflect.get(raiser, event) === listener) Reflect.set(raiser, event, replaced);
      };
    }
    case "returned": {
      const removal: unknown = Reflect.apply(method(raiser, listening.subscribe), raiser, [event, listener]);
      if (typeof removal !== "function") {
        throw new Stop("The method '" + listening.subscribe + "' returned no function that removes the listener it added.");
      }
      return () => Reflect.apply(removal, undefined, []);
    }
    default:
      return assertNever(listening);
  }
}

function method(holder: object, name: string): (...args: unknown[]) => unknown {
  const found: unknown = Reflect.get(holder, name);
  if (typeof found !== "function") throw new Stop("The method '" + name + "' is not on the object that should hold it.");
  return found as (...args: unknown[]) => unknown;
}

function run(context: Context, handler: Handler, args: unknown[]): void {
  try {
    const payload = walk(value(args), handler.payload, handler.members);
    const running = execute(handler.do, { ...context, scope: new Map([["e", payload], ["arguments", value(args)]]) });
    if (running.kind === "awaiting") running.settled.catch((stopped: unknown) => report(handler, stopped));
  } catch (stopped) {
    report(handler, stopped);
  }
}

function report(handler: Handler, stopped: unknown): void {
  if (!(stopped instanceof Stop)) throw stopped;
  console.error("[alis] the pipeline of '" + handler.event + "' stopped: " + stopped.reason);
}
