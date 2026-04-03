import type {
  CapabilityContract,
  MethodMember,
  PathSegment,
  Plan,
  PropertyMember,
  ValueShape,
} from "../../types";

export function scalar(type: "string" | "number" | "boolean" | "date" | "raw"): ValueShape {
  return { kind: "scalar", type };
}

export function arrayOf(item: ValueShape): ValueShape {
  return { kind: "array", item };
}

export function property(
  path: string | string[],
  shape: ValueShape,
  access: "read" | "write" | "readwrite" = "readwrite"
): PropertyMember {
  return {
    kind: "property",
    path: toSegments(path),
    shape,
    access,
  };
}

export function method(path: string | string[], args?: ValueShape[]): MethodMember {
  return {
    kind: "method",
    path: toSegments(path),
    ...(args ? { args } : {}),
    returns: "void",
  };
}

export function createPlan(partial: Omit<Partial<Plan>, "version"> = {}): Plan {
  return {
    version: 2,
    planId: partial.planId ?? "Test.Plan",
    sourceId: partial.sourceId,
    contracts: partial.contracts ?? {},
    objects: partial.objects ?? {},
    bindings: partial.bindings ?? {},
    workflows: partial.workflows ?? [],
  };
}

export function contextObjectContract(members: CapabilityContract["members"]): CapabilityContract {
  return {
    kind: "service",
    resolver: "context-object",
    members,
  };
}

export const htmlBlockContract: CapabilityContract = {
  kind: "element",
  resolver: "native-element",
  members: {
    text: property("textContent", scalar("string")),
    html: property("innerHTML", scalar("string")),
    hidden: property("hidden", scalar("boolean")),
    classAdd: method(["classList", "add"], [scalar("string")]),
    classRemove: method(["classList", "remove"], [scalar("string")]),
  },
};

export const nativeTextContract: CapabilityContract = {
  kind: "component",
  resolver: "native-element",
  members: {
    value: property("value", scalar("string")),
    checked: property("checked", scalar("boolean")),
    focus: method("focus"),
  },
  events: {
    changed: { channel: "change" },
  },
};

export const nativeButtonContract: CapabilityContract = {
  kind: "component",
  resolver: "native-element",
  members: {
    disabled: property("disabled", scalar("boolean")),
    text: property("textContent", scalar("string")),
  },
  events: {
    clicked: { channel: "click" },
  },
};

export function flushAsync(): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, 0));
}

function toSegments(path: string | string[]): PathSegment[] {
  const parts = Array.isArray(path) ? path : [path];
  return parts.map(prop => ({ prop }));
}
