import type { Handler, Listening, PlanDocument } from "./plan";
import { evaluate, walk, type Context } from "./evaluate";
import { execute } from "./execute";
import { assertNever, Stop, value, type Read } from "./read";

type Listener = (...args: unknown[]) => void;

export function contextOf(plan: PlanDocument): Context {
  const context: Context = { plan, scope: new Map<string, Read>(), locate: (id) => locate(context, id) };
  return context;
}

function locate(context: Context, id: string): object {
  const locator = context.plan.objects[id];
  if (locator === undefined) throw new Error("The plan reads the object '" + id + "', which it does not declare.");
  switch (locator.kind) {
    case "element":
      return onElement(context, locator.id, locator.vendor);
    default:
      return assertNever(locator.kind);
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

export function listen(context: Context, handler: Handler): void {
  const raiser = walk(value(context.locate(handler.on)), handler.path, "required");
  if (raiser.kind === "nothing" || raiser.value === null || typeof raiser.value !== "object") {
    throw new Stop("The object that raises '" + handler.event + "' is not on the page.");
  }
  const locator = context.plan.objects[handler.on];
  if (locator === undefined) throw new Error("The plan listens on the object '" + handler.on + "', which it does not declare.");
  const vendor = context.plan.vendors[locator.vendor];
  if (vendor === undefined) throw new Error("The plan names the vendor '" + locator.vendor + "', which it does not declare.");
  subscribe(raiser.value, vendor.listen, handler.event, (...args) => run(context, handler, args));
}

function subscribe(raiser: object, listening: Listening, event: string, listener: Listener): void {
  switch (listening.kind) {
    case "method":
      Reflect.apply(method(raiser, listening.subscribe), raiser, [event, listener]);
      return;
    case "assignment":
      Reflect.set(raiser, event, listener);
      return;
    case "returned":
      Reflect.apply(method(raiser, listening.subscribe), raiser, [event, listener]);
      return;
    default:
      assertNever(listening);
  }
}

function method(holder: object, name: string): (...args: unknown[]) => unknown {
  const found: unknown = Reflect.get(holder, name);
  if (typeof found !== "function") throw new Stop("The method '" + name + "' is not on the object that should hold it.");
  return found as (...args: unknown[]) => unknown;
}

function run(context: Context, handler: Handler, args: unknown[]): void {
  const payload = walk(value(args), handler.payload, "optional");
  try {
    const running = execute(handler.do, { ...context, scope: new Map([["e", payload]]) });
    if (running.kind === "awaiting") running.settled.catch((stopped: unknown) => report(handler, stopped));
  } catch (stopped) {
    report(handler, stopped);
  }
}

function report(handler: Handler, stopped: unknown): void {
  if (!(stopped instanceof Stop)) throw stopped;
  console.error("[alis] the pipeline of '" + handler.event + "' stopped: " + stopped.reason);
}
