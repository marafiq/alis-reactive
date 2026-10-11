import type { RuntimeComponent } from "./runtime-plan";

export interface ComponentEventChannel {
  readonly eventName: string;
  readonly channel: string;
}

export function componentEventChannel(component: RuntimeComponent, eventName: string): ComponentEventChannel {
  const declaredEvent = component.objectContract().events[eventName]!;
  return {
    eventName,
    channel: declaredEvent.channel,
  };
}
