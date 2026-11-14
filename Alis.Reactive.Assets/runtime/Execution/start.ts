import { holdSlots, slotsNamedBy, type Owner } from "../OwnershipAndState/index";
import { notBuilt, type Handler, type PlanDocument } from "../Schema/index";
import { Stop, value } from "../ValuesAndSources/index";
import { subscribe } from "../Vendors/index";
import { bind, isAnObject, type Context } from "./context";
import { Lane } from "./lane";
import { located } from "./objects";

const payloadName = "e";
const argumentsName = "arguments";

export function start(plan: PlanDocument, owner: Owner): void {
  holdSlots(slotsNamedBy(plan), owner);
  const context: Context = { plan, scope: new Map(), signal: new AbortController().signal };
  for (const handler of plan.handlers) {
    try {
      listen(handler, context, owner);
    } catch (stopped) {
      if (!(stopped instanceof Stop)) throw stopped;
      console.error("The handler of '" + handler.event + "' did not start: " + stopped.reason);
    }
  }
}

function listen(handler: Handler, context: Context, owner: Owner): void {
  if (handler.path.length > 0) throw notBuilt("a handler on a path below its object");
  if (handler.payload.length > 0) throw notBuilt("a handler that reads its payload");
  const source = located(context, handler.on);
  if (!isAnObject(source.object)) throw new Stop("The object that raises '" + handler.event + "' is not on the page.");
  const lane = new Lane(handler.policy, handler.do);
  const unsubscribe = subscribe(source.object, source.listen, handler.event, (...args: unknown[]) => {
    lane.raise(bind(bind(context, payloadName, value(args)), argumentsName, value(args)));
  });
  owner.runs(() => {
    lane.dispose();
  });
  owner.listens(unsubscribe);
}
