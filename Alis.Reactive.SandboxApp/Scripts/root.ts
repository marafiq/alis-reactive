// root.ts — ESM entry point for alis-reactive runtime
// esbuild bundles from here. Auto-discovers [data-reactive-plan] elements on page load.
// V3: plans have version, planId, types, components, behaviors.

import { boot, trace } from "./lifecycle/boot";
import { init as initConfirm } from "./components/fusion/confirm";
import { initNativeActionLinks } from "./components/native/native-action-link";
import "./components/native/drawer";  // side-effect: wires close button + Escape key
import "./components/native/loader";  // side-effect: handles target positioning + timeout
import { composeInitialPlans } from "./lifecycle/merge-plan";
import type { Plan } from "./types";
import type { TraceLevel } from "./core/trace";

initConfirm();
initNativeActionLinks();

const planEls = document.querySelectorAll<HTMLElement>("[data-reactive-plan]");
const plans: Plan[] = [];

for (const el of planEls) {
  const traceLevel = el.dataset.trace as TraceLevel | undefined;
  if (traceLevel) trace.setLevel(traceLevel);

  try {
    const text = el.textContent?.trim();
    if (!text) throw new Error("[alis] empty plan element");
    plans.push(JSON.parse(text));
  } catch (e) {
    throw new Error(`[alis] failed to parse plan JSON from [data-reactive-plan] element: ${(e as Error).message}`);
  }
}

for (const plan of composeInitialPlans(plans)) {
  boot(plan);
}
