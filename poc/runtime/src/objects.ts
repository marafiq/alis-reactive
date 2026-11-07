import type { Handler, Listening, ListenStatement, PathStep, PlanDocument, ReadPresence, Statement } from "./plan";
import { evaluate, walk, type Bound, type Context } from "./evaluate";
import { execute } from "./execute";
import { assertNever, Stop, value } from "./read";
import type { Owner } from "./owners";
import { plugin } from "./plugins";

type Listener = (...args: unknown[]) => void;

type Unsubscribe = (raiser: object, listener: Listener) => void;

interface Raised {
  readonly event: string;
  readonly payload: readonly PathStep[];
  readonly members: ReadPresence;
  readonly do: readonly Statement[];
}

interface Names {
  readonly payload: string;
  readonly arguments: string;
}

const handlerNames: Names = { payload: "e", arguments: "arguments" };

const listened = new WeakMap<ListenStatement, WeakSet<object>>();

export function contextOf(plan: PlanDocument, owner: Owner): Context {
  const context: Context = { plan, scope: new Map<string, Bound>(), locate: (id) => locate(context, id), owner };
  return context;
}

function locate(context: Context, id: string): unknown {
  const locator = context.plan.objects[id];
  if (locator === undefined) throw new Error("The plan reads the object '" + id + "', which it does not declare.");
  switch (locator.kind) {
    case "element":
      return heldOn(context, elementOf(locator.id), locator.vendor);
    case "plugin":
      return plugin(locator.name, locator.key);
    default:
      return assertNever(locator);
  }
}

interface Source {
  readonly object: unknown;
  readonly stillHeld: () => boolean;
}

function sourceOf(context: Context, id: string): Source {
  const locator = context.plan.objects[id];
  if (locator === undefined) throw new Error("The plan listens on the object '" + id + "', which it does not declare.");
  switch (locator.kind) {
    case "element": {
      const element = elementOf(locator.id);
      const object = heldOn(context, element, locator.vendor);
      return { object, stillHeld: () => holdsStill(context, element, locator.vendor, object) };
    }
    case "plugin":
      return { object: plugin(locator.name, locator.key), stillHeld: () => true };
    default:
      return assertNever(locator);
  }
}

function elementOf(id: string): HTMLElement {
  const element = document.getElementById(id);
  if (element === null) throw new Stop("No element has the id '" + id + "'.");
  return element;
}

function heldOn(context: Context, element: HTMLElement, vendorName: string): object {
  const vendor = context.plan.vendors[vendorName];
  if (vendor === undefined) throw new Error("The plan names the vendor '" + vendorName + "', which it does not declare.");
  const find = evaluate(vendor.locate, context);
  if (find.kind === "nothing" || typeof find.value !== "function") throw new Error("The vendor '" + vendorName + "' locates its object by no function.");
  const found: unknown = Reflect.apply(find.value, undefined, [element]);
  if (found === null || found === undefined || (typeof found !== "object" && typeof found !== "function")) {
    throw new Stop("The element '" + element.id + "' holds no object of the vendor '" + vendorName + "'.");
  }
  return found;
}

function holdsStill(context: Context, element: HTMLElement, vendorName: string, object: unknown): boolean {
  try {
    return heldOn(context, element, vendorName) === object;
  } catch (stopped) {
    if (stopped instanceof Stop) return false;
    throw stopped;
  }
}

export function listen(context: Context, handler: Handler): void {
  const source = sourceOf(context, handler.on);
  const raiser = walk(value(source.object), handler.path, "required");
  if (raiser.kind === "nothing" || raiser.value === null || typeof raiser.value !== "object") {
    throw new Stop("The object that raises '" + handler.event + "' is not on the page.");
  }
  const object = raiser.value;
  let refused = false;
  const listener: Listener = (...args) => {
    if (!refused) run(context, handler, handlerNames, args);
  };
  try {
    const unsubscribe = subscribe(object, listeningOf(context, handler.on), handler.event, listener);
    context.owner.holds(() => {
      if (source.stillHeld()) unsubscribe(object, listener);
    });
  } catch (stopped) {
    refused = true;
    throw stopped;
  }
}

export function listenTo(statement: ListenStatement, context: Context): void {
  const raiser = evaluate(statement.object, context);
  if (raiser.kind === "nothing" || raiser.value === null || typeof raiser.value !== "object") {
    throw new Stop("The object that raises '" + statement.event + "' is not on the page.");
  }
  const object = raiser.value;
  const heard = heardBy(statement);
  if (heard.has(object)) return;
  let refused = false;
  const listener: Listener = (...args) => {
    if (!refused) run(context, statement, { payload: statement.payloadName, arguments: statement.argumentsName }, args);
  };
  try {
    const unsubscribe = subscribe(object, statement.listen, statement.event, listener);
    const heldObject = new WeakRef(object);
    const heldListener = new WeakRef(listener);
    context.owner.holds(() => {
      const living = heldObject.deref();
      const subscribed = heldListener.deref();
      if (living !== undefined && subscribed !== undefined) unsubscribe(living, subscribed);
    });
    heard.add(object);
  } catch (stopped) {
    refused = true;
    throw stopped;
  }
}

function heardBy(statement: ListenStatement): WeakSet<object> {
  const known = listened.get(statement);
  if (known !== undefined) return known;
  const heard = new WeakSet<object>();
  listened.set(statement, heard);
  return heard;
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

function subscribe(raiser: object, listening: Listening, event: string, listener: Listener): Unsubscribe {
  switch (listening.kind) {
    case "method": {
      const add = method(raiser, listening.subscribe);
      const remove = method(raiser, listening.unsubscribe);
      Reflect.apply(add, raiser, [event, listener]);
      return (held, added) => Reflect.apply(remove, held, [event, added]);
    }
    case "assignment": {
      const replaced: unknown = Reflect.get(raiser, event);
      Reflect.set(raiser, event, listener);
      return (held, added) => {
        if (Reflect.get(held, event) === added) Reflect.set(held, event, replaced);
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

function run(context: Context, raised: Raised, names: Names, args: unknown[]): void {
  try {
    const scope = new Map(context.scope);
    scope.set(names.payload, walk(value(args), raised.payload, raised.members));
    scope.set(names.arguments, value(args));
    const running = execute(raised.do, { ...context, scope });
    if (running.kind === "awaiting") running.settled.catch((stopped: unknown) => report(raised, stopped));
  } catch (stopped) {
    report(raised, stopped);
  }
}

function report(raised: Raised, stopped: unknown): void {
  if (!(stopped instanceof Stop)) throw stopped;
  console.error("[alis] the pipeline of '" + raised.event + "' stopped: " + stopped.reason);
}
