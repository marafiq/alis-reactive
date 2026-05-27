// resolver.ts — semantic component event wiring.

import type { PlanDocument } from "../types";
import { scope } from "../core/trace";
import { RuntimePlan } from "../domain/runtime-plan";

const log = scope("resolver");

/** Wire an event listener on a component — dispatches to vendor-specific module. */
export function wireEvent(
  plan: PlanDocument,
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
