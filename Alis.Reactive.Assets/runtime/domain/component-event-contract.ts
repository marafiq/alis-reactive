import type { Event as PlanEvent } from "../types";
import type { RuntimeComponent } from "./runtime-plan";

export class ComponentEventContractError extends Error {
  private constructor(
    readonly componentKey: string,
    readonly typeKey: string,
    readonly eventName: string,
    message: string,
  ) {
    super(message);
    this.name = "ComponentEventContractError";
  }

  static missing(
    component: RuntimeComponent,
    eventName: string,
    declaredEventNames: readonly string[],
  ): ComponentEventContractError {
    const declaredEvents = declaredEventNames.length === 0
      ? "none"
      : declaredEventNames.join(", ");

    return new ComponentEventContractError(
      component.key,
      component.definition.type,
      eventName,
      `[alis] event "${eventName}" is not declared on component "${component.key}" (type: ${component.definition.type}; declared events: ${declaredEvents})`,
    );
  }
}

export class ComponentEventContract {
  private constructor(
    readonly eventName: string,
    readonly channel: string,
    readonly payloadType: PlanEvent["payloadType"],
  ) {}

  static declaredBy(component: RuntimeComponent, eventName: string): ComponentEventContract {
    const events = component.objectContract().events;
    const declaredEvent = events[eventName];
    if (declaredEvent === undefined) {
      throw ComponentEventContractError.missing(component, eventName, Object.keys(events));
    }

    return new ComponentEventContract(eventName, declaredEvent.channel, declaredEvent.payloadType);
  }
}
