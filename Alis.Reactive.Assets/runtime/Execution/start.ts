import { holdSlots, slotsNamedBy, type Owner } from "../OwnershipAndState/index";
import { assertNever, notBuilt, type Handler, type PlanDocument, type Policy } from "../Schema/index";
import { Stop, value } from "../ValuesAndSources/index";
import { subscribe } from "../Vendors/index";
import { bind, isAnObject, type Context } from "./context";
import { located } from "./objects";
import { run } from "./run";

const payloadName = "e";
const argumentsName = "arguments";

export function start(plan: PlanDocument, owner: Owner): void {
  holdSlots(slotsNamedBy(plan), owner);
  const context: Context = { plan, owner, scope: new Map() };
  for (const handler of plan.handlers) {
    try {
      listen(handler, context);
    } catch (stopped) {
      if (!(stopped instanceof Stop)) throw stopped;
      console.error("The handler of '" + handler.event + "' did not start: " + stopped.reason);
    }
  }
}

function listen(handler: Handler, context: Context): void {
  if (handler.path.length > 0) throw notBuilt("a handler on a path below its object");
  if (handler.payload.length > 0) throw notBuilt("a handler that reads its payload");
  requireBuilt(handler.policy);
  const source = located(context, handler.on);
  if (!isAnObject(source.object)) throw new Stop("The object that raises '" + handler.event + "' is not on the page.");
  const unsubscribe = subscribe(source.object, source.listen, handler.event, (...args: unknown[]) => {
    run(handler.do, bind(bind(context, payloadName, value(args)), argumentsName, value(args)));
  });
  context.owner.listens(unsubscribe);
}

function requireBuilt(policy: Policy): void {
  switch (policy) {
    case "overlap":
      return;
    case "replace":
    case "skip":
      throw notBuilt("the policy '" + policy + "'");
    default:
      assertNever(policy);
  }
}
