import type { PlanDocument } from "./plan";
import { contextOf, listen } from "./objects";
import { Stop } from "./read";

const planVersion = 7;

function boot(): void {
  document.dispatchEvent(new CustomEvent("alis:mount", { detail: { root: document.body } }));
  for (const script of document.querySelectorAll<HTMLScriptElement>('script[type="application/json"][data-alis-plan]')) {
    start(JSON.parse(script.textContent) as PlanDocument);
  }
  document.documentElement.dataset["alisBooted"] = "true";
}

function parsed(): boolean {
  const [navigation] = performance.getEntriesByType("navigation") as PerformanceNavigationTiming[];
  return document.readyState === "complete" || (navigation !== undefined && navigation.domContentLoadedEventStart > 0);
}

function start(plan: PlanDocument): void {
  if (plan.version !== planVersion) {
    console.error("[alis] the plan '" + plan.planId + "' is version " + String(plan.version) + "; this runtime reads version " + String(planVersion) + ".");
    return;
  }
  const context = contextOf(plan);
  for (const handler of plan.handlers) {
    try {
      listen(context, handler);
    } catch (stopped) {
      if (!(stopped instanceof Stop)) throw stopped;
      console.error("[alis] the handler of '" + handler.event + "' did not start: " + stopped.reason);
    }
  }
}

if (parsed()) boot();
else document.addEventListener("DOMContentLoaded", boot, { once: true });
