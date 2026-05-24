// resolver.ts — the ONE shared resolution module.
// Every module uses this. No other resolution path exists.
//
// Resolution flow:
//   1. Resolve a RuntimeComponent from RuntimePlan.
//   2. Use its vendor to resolve JS root: native -> getElementById, fusion -> ej2_instances[0].
//   3. Use its type contract for properties/methods/events.

import type { Plan, JsType } from "../types";
import { scope } from "../core/trace";
import { RuntimePlan } from "../domain/runtime-plan";

const log = scope("resolver");

/** Resolve a component's raw DOM element (not vendor root). For inject, validation, error display. */
export function resolveElement(plan: Plan, componentKey: string): HTMLElement {
  return RuntimePlan.from(plan).components.element(componentKey);
}

// ── JsType lookup ──────────────────────────────────────────

/** Get JsType for a component by key. */
export function getJsType(plan: Plan, componentKey: string): JsType {
  return RuntimePlan.from(plan).components.jsType(componentKey);
}

// ── Event wiring ──────────────────────────────────────────

/** Wire an event listener on a component — dispatches to vendor-specific module. */
export function wireEvent(
  plan: Plan,
  componentKey: string,
  channel: string,
  handler: (data: unknown) => void,
  opts?: AddEventListenerOptions,
): void {
  const runtimePlan = RuntimePlan.from(plan);
  const component = runtimePlan.components.requireComponent(componentKey);
  component.runtime().wireEvent(
    component.root(),
    channel,
    handler,
    opts,
  );
}

log.debug("module.loaded");
