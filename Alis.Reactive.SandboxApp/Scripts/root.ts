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
import {
  promoteTracingConfig,
  resolveInitialTracingConfig,
  type IncrementalTracingState,
} from "./tracing/context";
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
// plan elements. Plan JSON has not been parsed yet so only dataset
// attributes contribute to this initial configure call. Any plan
// element with an invalid body will emit `plan.parse.fail` at this
// initial level even if every later plan would have asked for more.
const preParseConfig = resolveInitialTracingConfig(
  planEls,
  planEls.map(() => ({})),
);
configure({ level: preParseConfig.level });

const rootTracer = tracer("root");
const plans: Plan[] = [];
const rejectedTraceparents: { index: number; value: string }[] = [];

// Incremental tracing state accumulator. Every successfully-parsed
// plan's level + traceparent folds in, and `configure()` is re-run
// BEFORE the next plan element is parsed, so a later `plan.parse.fail`
// event emits at the level the user asked for via `plan.traceLevel`
// on any earlier successful plan. Round 5 finding #2: without this
// incremental promotion, the error level was stuck at whatever only
// the DOM dataset attribute provided.
let tracingState: IncrementalTracingState = {
  level: preParseConfig.level,
  traceparent: undefined,
};

for (let i = 0; i < planEls.length; i++) {
  const el = planEls[i];
  let plan: Plan;
  try {
    const text = el.textContent?.trim();
    if (!text) throw new Error("[alis] empty plan element");
    plan = JSON.parse(text);
  } catch (e) {
    rootTracer.error(
      "plan.parse.fail",
      { elementId: el.id || undefined, planIndex: i },
      e instanceof Error ? e : new Error(String(e)),
    );
    throw new Error(
      `[alis] failed to parse plan JSON from [data-reactive-plan] element: ${(e as Error).message}`,
    );
  }

  plans.push(plan);

  // Promote tracing state with this plan's fields and re-configure
  // before the next iteration. If the next element is malformed,
  // rootTracer.error above will emit at the accumulated level.
  const promoted = promoteTracingConfig(tracingState, el, plan, i);
  tracingState = promoted.state;
  if (promoted.rejectedTraceparent) {
    rejectedTraceparents.push(promoted.rejectedTraceparent);
  }
  configure({
    level: tracingState.level,
    traceparent: tracingState.traceparent,
  });
}

// Surface rejected traceparent candidates now that every plan has been
// parsed and the final tracing level is in effect.
for (const rejected of rejectedTraceparents) {
  rootTracer.warn("plan.traceparent.invalid", {
    planIndex: rejected.index,
    value: rejected.value,
  });
}

for (const plan of composeInitialPlans(plans)) {
  boot(plan);
}
