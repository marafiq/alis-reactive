// root.ts — ESM entry point for alis-reactive runtime
// esbuild bundles from here. Auto-discovers [data-alis-plan] elements on page load.
// Lives at Scripts/ root by design — everything else is organized in subdirectories.

import { boot, trace } from "./lifecycle/boot";
import { init as initConfirm } from "./components/fusion/confirm";
import { initNativeActionLinks } from "./components/native/native-action-link";
import "./components/native/drawer";  // side-effect: wires close button + Escape key
import "./components/native/loader";  // side-effect: handles target positioning + timeout
import "./components/native/checklist";  // side-effect: syncs checkbox values to hidden input
import { composeInitialPlans } from "./lifecycle/merge-plan";
import type { Plan } from "./types";
import type { TraceLevel } from "./core/trace";

initConfirm();
initNativeActionLinks();

const planEls = document.querySelectorAll<HTMLElement>("[data-alis-plan]");
const plans: Plan[] = [];

for (const el of planEls) {
  const traceLevel = el.getAttribute("data-trace") as TraceLevel | null;
  if (traceLevel) trace.setLevel(traceLevel);

  try {
    plans.push(JSON.parse(el.textContent!));
  } catch (e) {
    throw new Error(`[alis] failed to parse plan JSON from [data-alis-plan] element: ${(e as Error).message}`);
  }
}

for (const plan of composeInitialPlans(plans)) {
  boot(plan);
}
