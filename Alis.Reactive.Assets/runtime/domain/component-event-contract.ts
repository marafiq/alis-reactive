import type { Event as PlanEvent } from "../types";
import type { RuntimeComponent } from "./runtime-plan";

export interface ComponentEventChannel {
  readonly eventName: string;
  readonly channel: string;
  readonly payloadType: PlanEvent["payloadType"];
}

export function componentEventChannel(component: RuntimeComponent, eventName: string): ComponentEventChannel {
  const declaredEvent = component.objectContract().events[eventName]!;
  return {
    eventName,
    channel: declaredEvent.channel,
    payloadType: declaredEvent.payloadType,
  };
}
