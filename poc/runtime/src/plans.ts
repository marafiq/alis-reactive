import type { PlanDocument } from "./plan";
import { contextOf, listen } from "./objects";
import { Stop } from "./read";
import type { Owner } from "./owners";

const planVersion = 8;

export function startWithin(root: ParentNode, owner: Owner): void {
  for (const script of root.querySelectorAll<HTMLScriptElement>('script[type="application/json"][data-alis-plan]')) {
    start(JSON.parse(script.textContent) as PlanDocument, owner);
  }
}

function start(plan: PlanDocument, owner: Owner): void {
  if (plan.version !== planVersion) {
    console.error("[alis] the plan '" + plan.planId + "' is version " + String(plan.version) + "; this runtime reads version " + String(planVersion) + ".");
    return;
  }
  const context = contextOf(plan);
  for (const handler of plan.handlers) {
    try {
      listen(context, handler, owner);
    } catch (stopped) {
      if (!(stopped instanceof Stop)) throw stopped;
      console.error("[alis] the handler of '" + handler.event + "' did not start: " + stopped.reason);
    }
  }
}
