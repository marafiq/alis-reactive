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
import { resolveInitialTracingConfig } from "./tracing/context";
import { registerPlugin } from "./core/plugin-registry";

// Drain passive plugin queue — plugins push here from separate bundles before framework loads
const pendingPlugins = (window as any).__alisPlugins as Array<{ name: string; instance: unknown }> | undefined;
if (pendingPlugins) {
  for (const entry of pendingPlugins) registerPlugin(entry.name, entry.instance);
  delete (window as any).__alisPlugins;
}

initConfirm();
initNativeActionLinks();

const planEls = Array.from(
  document.querySelectorAll<HTMLElement>("[data-reactive-plan]"),
);

// Pre-parse configure: pick the most verbose `data-trace` across ALL
// plan elements so a parse error from ANY element emits at the level the
// page author asked for. Without this step, `plan.parse.fail` events
// would fire while `activeLevel` is still `off` and be silently dropped.
// Plan JSON hasn't been parsed yet, so we pass an empty plans array —
// only dataset attributes contribute to this first configure call.
const preParseConfig = resolveInitialTracingConfig(
  planEls,
  planEls.map(() => ({})),
);
configure({ level: preParseConfig.level });

const rootTracer = tracer("root");
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

// Re-configure with the full plan set now that parsing has succeeded.
// `resolveInitialTracingConfig` walks every plan element + parsed plan,
// preserves the historical dataset-over-plan precedence per-plan, and
// returns the most-verbose level across all plans plus the first plan
// carrying a server traceparent. Multi-plan pages (composeInitialPlans)
// therefore honor tracing config from every plan, not only plans[0].
if (plans.length > 0) {
  const finalConfig = resolveInitialTracingConfig(planEls, plans);
  configure({
    level: finalConfig.level,
    traceparent: finalConfig.traceparent,
  });
}

for (const plan of composeInitialPlans(plans)) {
  boot(plan);
}
