import type { PlanDocument } from "../types/index";
import { scope } from "../diagnostics/trace";
import { RuntimePlan } from "../browser-objects/runtime-plan";

const log = scope("resolver");

export function wireEvent(
  planDocument: PlanDocument,
  componentKey: string,
  channel: string,
  handler: (data: unknown) => void,
  eventOptions?: AddEventListenerOptions,
): void {
  const runtimePlan = RuntimePlan.from(planDocument);
  const component = runtimePlan.components.component(componentKey);
  component.runtime().wireEvent(
    component.root(),
    channel,
    handler,
    eventOptions,
  );
}

log.debug("module.loaded");
