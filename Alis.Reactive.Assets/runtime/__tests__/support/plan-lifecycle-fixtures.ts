import { vi } from "vitest";
import type { MergeHooks } from "../../lifecycle/merge-plan";
import type { Behavior, Component, ComponentValidation, JsType, Plan, Shape } from "../../types";

export function jsType(): JsType {
  return {
    properties: {},
    methods: {},
    events: {},
  };
}

export function jsTypeWithReadableProperty(member: string): JsType {
  return jsTypeWithPropertyAccess(member, "read");
}

export function jsTypeWithWritableProperty(member: string): JsType {
  return jsTypeWithPropertyAccess(member, "write");
}

export function jsTypeWithPropertyAccess(
  member: string,
  access: JsType["properties"][string]["access"],
): JsType {
  return {
    properties: {
      [member]: {
        path: [{ kind: "property", name: member }],
        shape: { kind: "string" },
        access,
      },
    },
    methods: {},
    events: {},
  };
}

export function jsTypeWithPropertyShape(
  member: string,
  shape: JsType["properties"][string]["shape"],
): JsType {
  return {
    properties: {
      [member]: {
        path: [{ kind: "property", name: member }],
        shape,
        access: "read",
      },
    },
    methods: {},
    events: {},
  };
}

export function jsTypeWithMethodShape(
  member: string,
  argumentShape: Shape,
  returnShape: Shape,
): JsType {
  return {
    properties: {},
    methods: {
      [member]: {
        path: [{ kind: "property", name: member }],
        arguments: { kind: "exact", shapes: [argumentShape] },
        returns: returnShape,
      },
    },
    events: {},
  };
}

export function component(
  id: string,
  type = `native.element.${id}`,
): Component {
  return {
    id,
    vendor: "native",
    type,
    contribution: { kind: "object-target" },
    binding: { kind: "none" },
    container: { kind: "none" },
  };
}

export function registeredInputComponent(id: string, type = `native.component.${id}`): Component {
  return {
    id,
    vendor: "native",
    type,
    contribution: { kind: "owned-definition" },
    binding: {
      kind: "registered-input",
      bindingPath: "CareUnit",
      valueMember: "value",
    },
    container: { kind: "none" },
  };
}

export function layoutComponent(id: string, type = `native.component.${id}`): Component {
  return {
    id,
    vendor: "native",
    type,
    contribution: { kind: "layout-object" },
    binding: { kind: "none" },
    container: { kind: "none" },
  };
}

export function validationContainer(id: string, validationRules: ComponentValidation[]): Component {
  return {
    id,
    vendor: "native",
    type: `native.element.${id}`,
    contribution: { kind: "validation-container" },
    binding: { kind: "none" },
    container: {
      kind: "validation-container",
      components: [],
      validationRules,
    },
  };
}

export function validationRule(componentKey: string, message = `${componentKey} required`): ComponentValidation {
  return {
    component: componentKey,
    value: { kind: "none" },
    serverFieldName: componentKey,
    rules: [
      {
        name: "required",
        message,
        execution: {
          constraint: { kind: "none" },
          otherValue: { kind: "none" },
          activation: { kind: "always" },
          comparisonShape: { kind: "none" },
        },
      },
    ],
  };
}

export function behavior(): Behavior {
  return {
    startsWhen: { kind: "page-ready" },
    reaction: { kind: "sequence", steps: [] },
  };
}

export function rootPlan(planId: string): Plan {
  return {
    version: 3,
    planId,
    scope: { kind: "root" },
    types: {},
    components: {},
    behaviors: [],
  };
}

export function partialPlan(
  planId: string,
  partId: string,
  entries: {
    readonly types?: Record<string, JsType>;
    readonly components?: Record<string, Component>;
    readonly behaviors?: Behavior[];
  },
): Plan {
  return {
    version: 3,
    planId,
    scope: { kind: "partial", partId },
    types: entries.types ?? {},
    components: entries.components ?? {},
    behaviors: entries.behaviors ?? [],
  };
}

export function mergeHooks() {
  const behaviorSignals: (AbortSignal | undefined)[] = [];
  const hooks: MergeHooks = {
    wireBehaviors: vi.fn((_behaviors, _plan, signal) => behaviorSignals.push(signal)),
    wireContainerValidation: vi.fn(),
  };

  return { hooks, behaviorSignals };
}

export function validationComponents(plan: Plan, componentKey: string): string[] {
  const container = plan.components[componentKey]?.container;
  if (container?.kind !== "validation-container") {
    throw new Error(`Expected component "${componentKey}" to be a validation container`);
  }

  return container.validationRules.map(rule => rule.component);
}
