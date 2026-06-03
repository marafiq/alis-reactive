import type { PlanDocument } from "../types/index";
import { scope } from "../diagnostics/trace";
import { RuntimePlan } from "../browser-objects/runtime-plan";

const log = scope("resolver");

export function wireEvent(
  plan: PlanDocument,
  componentKey: string,
  channel: string,
  handler: (data: unknown) => void,
  opts?: AddEventListenerOptions,
): void {
  const runtimePlan = RuntimePlan.from(plan);
  const component = runtimePlan.components.component(componentKey);
  component.runtime().wireEvent(
    component.root(),
    channel,
    handler,
    opts,
  );
}

log.debug("module.loaded");
