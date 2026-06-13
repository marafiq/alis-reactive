import { vi } from "vitest";
import type { ActivePlanWiring } from "../../lifecycle/applied-plans";
import type { Behavior, ComponentObject, ComponentValidation, BrowserObjectContract, PathSegment, PlanDocument, Shape, StructuredPath } from "../../types/index";

export function objectContract(): BrowserObjectContract {
  return {
    properties: {},
    methods: {},
    events: {},
  };
}

export function objectContractWithReadableProperty(member: string): BrowserObjectContract {
  return objectContractWithPropertyAccess(member, "read");
}

export function objectContractWithWritableProperty(member: string): BrowserObjectContract {
  return objectContractWithPropertyAccess(member, "write");
}

export function objectContractWithPropertyAccess(
  member: string,
  access: BrowserObjectContract["properties"][string]["access"],
): BrowserObjectContract {
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

export function objectContractWithPropertyShape(
  member: string,
  shape: BrowserObjectContract["properties"][string]["shape"],
): BrowserObjectContract {
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

export function objectContractWithMethodShape(
  member: string,
  argumentShape: Shape,
  returnShape: Shape,
): BrowserObjectContract {
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
): ComponentObject {
  return {
    id,
    vendor: "native",
    type,
    role: { kind: "object-target" },
    binding: { kind: "none" },
    container: { kind: "none" },
  };
}

export function registeredInputComponent(id: string, type = `native.component.${id}`): ComponentObject {
  return {
    id,
    vendor: "native",
    type,
    role: { kind: "plan-input" },
    binding: {
      kind: "registered-input",
      bindingPath: "CareUnit",
      path: structuredPath("CareUnit"),
      valueMember: "value",
    },
    container: { kind: "none" },
  };
}

export function structuredPath(name: string): StructuredPath {
  const [first, ...rest] = name.split(".").map(pathSegment);
  if (first === undefined) throw new Error(`Expected path for ${name}`);

  return [first, ...rest];
}

function pathSegment(part: string): PathSegment {
  return { kind: "property", name: part };
}

export function layoutComponent(id: string, type = `native.component.${id}`): ComponentObject {
  return {
    id,
    vendor: "native",
    type,
    role: { kind: "layout-object" },
    binding: { kind: "none" },
    container: { kind: "none" },
  };
}

export function validationContainer(id: string, validationRules: ComponentValidation[]): ComponentObject {
  return {
    id,
    vendor: "native",
    type: `native.element.${id}`,
    role: { kind: "validation-container" },
    binding: { kind: "none" },
    container: {
      kind: "validation-container",
      validationRules,
    },
  };
}

export function validationRule(componentKey: string, message = `${componentKey} required`): ComponentValidation {
  return {
    component: componentKey,
    value: {
      kind: "read",
      from: { kind: "component", component: componentKey },
      member: "value",
      path: [],
      shape: { kind: "string" },
      access: { kind: "property" },
    },
    serverFieldName: componentKey,
    rules: [
      {
        name: "required",
        message,
        execution: {
          kind: "none",
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

export function rootPlan(planId: string): PlanDocument {
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
  planParts: {
    readonly types?: Record<string, BrowserObjectContract>;
    readonly components?: Record<string, ComponentObject>;
    readonly behaviors?: Behavior[];
  },
): PlanDocument {
  return {
    version: 3,
    planId,
    scope: { kind: "partial" },
    types: planParts.types ?? {},
    components: planParts.components ?? {},
    behaviors: planParts.behaviors ?? [],
  };
}

export function testPlanWiring() {
  const behaviorSignals: (AbortSignal | undefined)[] = [];
  const wiring: ActivePlanWiring = {
    wireBehaviors: vi.fn((_behaviors, _plan, signal) => behaviorSignals.push(signal)),
    wireContainerValidation: vi.fn(),
  };

  return { wiring, behaviorSignals };
}

export function validationComponents(plan: PlanDocument, componentKey: string): string[] {
  const container = plan.components[componentKey]?.container;
  if (container?.kind !== "validation-container") {
    throw new Error(`Expected component "${componentKey}" to be a validation container`);
  }

  return container.validationRules.map(rule => rule.component);
}
