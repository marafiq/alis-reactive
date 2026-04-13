// root.ts — ESM entry point for alis-reactive runtime
// esbuild bundles from here. Auto-discovers [data-reactive-plan] elements on page load.
// V3: plans have version, planId, types, components, behaviors.

import { boot } from "./lifecycle/boot";
import { init as initConfirm } from "./components/fusion/confirm";
import { initNativeActionLinks } from "./components/native/native-action-link";
import "./components/native/drawer";  // side-effect: wires close button + Escape key
import "./components/native/loader";  // side-effect: handles target positioning + timeout
import { composeInitialPlans } from "./lifecycle/merge-plan";
import type { Plan } from "./types";
import { configure, tracer } from "./tracing";
import { resolveLevel } from "./tracing/context";
import { registerPlugin } from "./core/plugin-registry";

// Drain passive plugin queue — plugins push here from separate bundles before framework loads
const pendingPlugins = (window as any).__alisPlugins as Array<{ name: string; instance: unknown }> | undefined;
if (pendingPlugins) {
  for (const entry of pendingPlugins) registerPlugin(entry.name, entry.instance);
  delete (window as any).__alisPlugins;
}

initConfirm();
initNativeActionLinks();

const rootTracer = tracer("root");
const planEls = document.querySelectorAll<HTMLElement>("[data-reactive-plan]");
const plans: Plan[] = [];

for (const el of planEls) {
  try {
    const text = el.textContent?.trim();
    if (!text) throw new Error("[alis] empty plan element");
    plans.push(JSON.parse(text));
  } catch (e) {
    rootTracer.error(
      "plan.parse.fail",
      { elementId: el.id || undefined },
      e instanceof Error ? e : new Error(String(e)),
    );
    throw new Error(`[alis] failed to parse plan JSON from [data-reactive-plan] element: ${(e as Error).message}`);
  }
}

// Configure tracing from the first discovered plan + its data-trace attribute.
// resolveLevel gives precedence to data-trace over plan.traceLevel, preserving
// the historical override path from the pre-tracing root.ts.
if (plans.length > 0) {
  const firstEl = planEls[0];
  configure({
    level: resolveLevel(plans[0].traceLevel, firstEl?.dataset.trace),
    traceparent: plans[0].traceparent,
  });
}

for (const plan of composeInitialPlans(plans)) {
  boot(plan);
}
