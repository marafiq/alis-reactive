import type { PlanDocument } from "./plan";
import { contextOf, listen } from "./objects";
import { Stop } from "./read";
import type { Owner } from "./owners";

const planVersion = 9;

export interface Found {
  readonly plan: PlanDocument;
  readonly script: HTMLScriptElement;
}

export function plansWithin(root: ParentNode): Found[] {
  return [...root.querySelectorAll<HTMLScriptElement>('script[type="application/json"][data-alis-plan]')].map((script) => ({
    plan: JSON.parse(script.textContent) as PlanDocument,
    script,
  }));
}

export function startWithin(root: ParentNode, owner: Owner): void {
  for (const { plan } of plansWithin(root)) start(plan, owner);
}

export function start(plan: PlanDocument, owner: Owner): void {
  if (plan.version !== planVersion) {
    console.error("[alis] the plan '" + plan.planId + "' is version " + String(plan.version) + "; this runtime reads version " + String(planVersion) + ".");
    return;
  }
  const context = contextOf(plan, owner);
  for (const handler of plan.handlers) {
    try {
      listen(context, handler);
    } catch (stopped) {
      if (!(stopped instanceof Stop)) throw stopped;
      console.error("[alis] the handler of '" + handler.event + "' did not start: " + stopped.reason);
    }
  }
}

export function slotsNamedBy(plan: PlanDocument): string[] {
  return Object.values(plan.objects).flatMap((locator) => (locator.kind === "plugin" && locator.name === "alis.slot" ? [locator.key] : []));
}
