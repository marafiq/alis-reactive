// Plan discovery is one of the two justified wide queries: the runtime finds the plan scripts in HTML
// it did not author, at boot and in injected partials.

import type { PlanDocument } from "../types/index";

const PLAN_SELECTOR = "[data-reactive-plan]";

/** A plan and the script it was read from, which says where the server rendered the plan. */
export interface DiscoveredPlan {
  readonly plan: PlanDocument;
  readonly script: HTMLElement;
}

/** Parses every plan script under a root, in document order. With `remove`, the scripts leave the markup. */
export function discoverPlans(root: ParentNode, remove: boolean): DiscoveredPlan[] {
  const plans: DiscoveredPlan[] = [];
  for (const script of Array.from(root.querySelectorAll<HTMLElement>(PLAN_SELECTOR))) {
    plans.push({ plan: parsePlan(script), script });
    if (remove) script.remove();
  }
  return plans;
}

// An element's text is never null; an empty script fails to parse, as any text the framework did not write.
function parsePlan(script: HTMLElement): PlanDocument {
  try {
    return JSON.parse(script.textContent!) as PlanDocument;
  } catch (error) {
    throw new Error(`[alis] a plan script is not valid JSON: ${(error as Error).message}`);
  }
}
