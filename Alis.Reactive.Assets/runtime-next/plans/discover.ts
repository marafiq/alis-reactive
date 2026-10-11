// Plan discovery is one of the two justified wide queries: the runtime finds the plan scripts in HTML
// it did not author, at boot and in injected partials.

import type { PlanDocument } from "../types/index";
import { setLevel, type TraceLevel } from "../diagnostics/trace";

const PLAN_SELECTOR = "[data-reactive-plan]";

/** Parses every plan script under a root. With `remove`, the scripts leave the markup. */
export function discoverPlans(root: ParentNode, remove: boolean): PlanDocument[] {
  const plans: PlanDocument[] = [];
  for (const script of Array.from(root.querySelectorAll<HTMLElement>(PLAN_SELECTOR))) {
    const traceLevel = script.dataset.trace as TraceLevel | undefined;
    if (traceLevel) setLevel(traceLevel);
    plans.push(parsePlan(script));
    if (remove) script.remove();
  }
  return plans;
}

function parsePlan(script: HTMLElement): PlanDocument {
  const json = script.textContent?.trim();
  if (!json) throw new Error("[alis] a plan script is empty");
  try {
    return JSON.parse(json) as PlanDocument;
  } catch (error) {
    throw new Error(`[alis] a plan script is not valid JSON: ${(error as Error).message}`);
  }
}
