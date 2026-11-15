import { pageInputs } from "../InputsAndIds/index";
import { holdSlots, placeOf, slotsNamedBy, type Owner } from "../OwnershipAndState/index";
import { assertNever, type Expression, type FnExpression, type Handler, type Locator, type PlanDocument, type Trace } from "../Schema/index";
import type { Traced } from "../Tracing/index";
import { pageForms } from "../Validation/index";
import { got, heldOrNull, Stop, value } from "../ValuesAndSources/index";
import { subscribe } from "../Vendors/index";
import { bind, isAnObject, within, type Context } from "./context";
import { applied, evaluate } from "./expressions";
import { Lane } from "./lane";
import { heldBy, located, locatorOf, type ElementFor, type Located, type PluginFor } from "./objects";

const payloadName = "e";
const argumentsName = "arguments";

export type TraceOf = (trace: Trace) => Traced;

export function start(plan: PlanDocument, owner: Owner, elementFor: ElementFor, pluginFor: PluginFor, traceOf: TraceOf): void {
  const traced = traceOf(plan.trace);
  const boot = traced.span("alis.boot", { "alis.plan_id": plan.planId });
  const context: Context = { plan, owner, scope: new Map(), signal: new AbortController().signal, trace: traced };
  try {
    pageInputs.join(plan, owner, placeOf(owner), (expression: Expression): unknown => heldOrNull(evaluate(expression, context)));
    pageForms.join(plan, owner, (fn: FnExpression, args: readonly unknown[]): unknown => applied(fn, args, context));
  } catch (stopped) {
    if (!(stopped instanceof Stop)) throw stopped;
    console.error(stopped.reason);
    boot.event("alis.stop", {});
    boot.end("stopped");
    return;
  }
  holdSlots(slotsNamedBy(plan), owner);
  for (const [index, handler] of plan.handlers.entries()) {
    try {
      listen(handler, within(context, traced.handler(index, handler.event)), elementFor, pluginFor);
    } catch (stopped) {
      if (!(stopped instanceof Stop)) throw stopped;
      console.error("The handler of '" + handler.event + "' on " + named(locatorOf(context, handler.on)) + " did not start: " + stopped.reason);
      boot.event("alis.error", {});
    }
  }
  boot.end("completed");
}

function listen(handler: Handler, context: Context, elementFor: ElementFor, pluginFor: PluginFor): void {
  const source = located(context, handler.on, elementFor, pluginFor);
  const raiser = got(value(source.object), handler.path, "required");
  if (raiser.kind === "nothing" || !isAnObject(raiser.value)) throw new Stop("The object that raises '" + handler.event + "' is not on the page.");
  const stillHeld = stillHeldOf(source, context);
  const lane = new Lane(handler.policy, handler.do, context.owner);
  const heard = { released: false };
  const unsubscribe = subscribe(raiser.value, source.listen, handler.event, (...args: unknown[]) => {
    if (heard.released) return;
    try {
      const payload = got(value(args), handler.payload, handler.members);
      lane.raise(bind(bind(context, payloadName, payload), argumentsName, value(args)));
    } catch (stopped) {
      if (!(stopped instanceof Stop)) throw stopped;
      console.error(stopped.reason);
    }
  });
  context.owner.runs(() => {
    lane.dispose();
  });
  context.owner.listens(() => {
    heard.released = true;
    if (stillHeld()) unsubscribe();
  });
}

function stillHeldOf(source: Located, context: Context): () => boolean {
  switch (source.kind) {
    case "element": {
      const holds = heldBy(source.element, source.vendor, context);
      const object = source.object;
      return () => holds(object);
    }
    case "plugin":
      return () => true;
    default:
      return assertNever(source);
  }
}

function named(locator: Locator): string {
  switch (locator.kind) {
    case "element":
      return "the element '" + locator.id + "' of the vendor '" + locator.vendor + "'";
    case "plugin":
      return "the plugin '" + locator.name + "'";
    default:
      return assertNever(locator);
  }
}
