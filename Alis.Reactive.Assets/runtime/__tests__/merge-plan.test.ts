import { describe, expect, it, vi } from "vitest";
import { composeInitialPlans, PlanRegistry, type MergeHooks } from "../lifecycle/merge-plan";
import type { Behavior, Component, ComponentValidation, JsType, Plan } from "../types";

function jsType(): JsType {
  return {
    properties: {},
    methods: {},
    events: {},
  };
}

function jsTypeWithReadableProperty(member: string): JsType {
  return {
    properties: {
      [member]: {
        path: [{ kind: "property", name: member }],
        shape: { kind: "string" },
        access: "read",
      },
    },
    methods: {},
    events: {},
  };
}

function jsTypeWithWritableProperty(member: string): JsType {
  return {
    properties: {
      [member]: {
        path: [{ kind: "property", name: member }],
        shape: { kind: "string" },
        access: "write",
      },
    },
    methods: {},
    events: {},
  };
}

function jsTypeWithPropertyAccess(
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

function jsTypeWithPropertyShape(member: string, shape: JsType["properties"][string]["shape"]): JsType {
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

function component(
  id: string,
  type = `native.element.${id}`,
  contribution: Component["contribution"] = { kind: "object-target" },
): Component {
  return {
    id,
    vendor: "native",
    type,
    contribution,
    binding: { kind: "none" },
    container: { kind: "none" },
  };
}

function registeredInputComponent(id: string, type = `native.component.${id}`): Component {
  return {
    ...component(id, type, { kind: "owned-definition" }),
    binding: {
      kind: "registered-input",
      bindingPath: "CareUnit",
      valueMember: "value",
    },
  };
}

function layoutComponent(id: string, type = `native.component.${id}`): Component {
  return component(id, type, { kind: "layout-object" });
}

function validationContainer(id: string, validationRules: ComponentValidation[]): Component {
  return {
    ...component(id, `native.element.${id}`, { kind: "validation-container" }),
    container: {
      kind: "validation-container",
      components: [],
      validationRules,
    },
  };
}

function validationRule(componentKey: string, message = `${componentKey} required`): ComponentValidation {
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

function behavior(): Behavior {
  return {
    startsWhen: { kind: "page-ready" },
    reaction: { kind: "sequence", steps: [] },
  };
}

function rootPlan(planId: string): Plan {
  return {
    version: 3,
    planId,
    scope: { kind: "root" },
    types: {},
    components: {},
    behaviors: [],
  };
}

function partialPlan(
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

function mergeHooks() {
  const behaviorSignals: (AbortSignal | undefined)[] = [];
  const hooks: MergeHooks = {
    wireBehaviors: vi.fn((_behaviors, _plan, signal) => behaviorSignals.push(signal)),
    wireContainerValidation: vi.fn(),
  };

  return { hooks, behaviorSignals };
}

describe("PlanRegistry partial merge", () => {
  it("replaces the previous contribution from the same partial scope", () => {
    const registry = new PlanRegistry();
    const { hooks, behaviorSignals } = mergeHooks();
    const planId = "Resident.Root";

    registry.register(rootPlan(planId));

    const oldBehavior = behavior();
    const first = registry.add(
      partialPlan(planId, "address-slot", {
        types: { "native.element.address-line": jsType() },
        components: { "address-line": component("address-line") },
        behaviors: [oldBehavior],
      }),
      hooks,
    );

    expect(first.components["address-line"]).toBeDefined();
    expect(first.types["native.element.address-line"]).toBeDefined();
    expect(first.behaviors).toContain(oldBehavior);
    expect(behaviorSignals[0]?.aborted).toBe(false);

    const newBehavior = behavior();
    const second = registry.add(
      partialPlan(planId, "address-slot", {
        types: { "native.element.zip-code": jsType() },
        components: { "zip-code": component("zip-code") },
        behaviors: [newBehavior],
      }),
      hooks,
    );

    expect(behaviorSignals[0]?.aborted).toBe(true);
    expect(second.components["address-line"]).toBeUndefined();
    expect(second.types["native.element.address-line"]).toBeUndefined();
    expect(second.behaviors).not.toContain(oldBehavior);
    expect(second.components["zip-code"]).toBeDefined();
    expect(second.types["native.element.zip-code"]).toBeDefined();
    expect(second.behaviors).toContain(newBehavior);
  });

  it("rejects a component key owned by a different partial scope", () => {
    const registry = new PlanRegistry();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";

    registry.register(rootPlan(planId));

    const firstComponent = component("shared-field", "native.element.shared-a");
    registry.add(
      partialPlan(planId, "first-slot", {
        types: { "native.element.shared-a": jsType() },
        components: { "shared-field": firstComponent },
      }),
      hooks,
    );

    expect(() => registry.add(
      partialPlan(planId, "second-slot", {
        types: { "native.element.shared-b": jsType() },
        components: { "shared-field": component("shared-field", "native.element.shared-b") },
      }),
      hooks,
    )).toThrow('partial plan contribution "second-slot" cannot declare component "shared-field"');
    expect(registry.get(planId)?.components["shared-field"]).toBe(firstComponent);
  });

  it("does not keep partial types from a contribution rejected during preflight", () => {
    const registry = new PlanRegistry();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";

    registry.register(rootPlan(planId));
    registry.add(
      partialPlan(planId, "first-slot", {
        components: { "shared-field": component("shared-field", "native.element.shared-a") },
      }),
      hooks,
    );

    expect(() => registry.add(
      partialPlan(planId, "second-slot", {
        types: { "native.element.second-only": jsType() },
        components: { "shared-field": component("shared-field", "native.element.shared-b") },
      }),
      hooks,
    )).toThrow('partial plan contribution "second-slot" cannot declare component "shared-field"');
    expect(registry.get(planId)?.types["native.element.second-only"]).toBeUndefined();
  });

  it("shares an identical type contract across partial scopes until the last owner unloads", () => {
    const registry = new PlanRegistry();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const sharedTypeKey = "plugin.address";

    registry.loadPartialSlot("first-slot", [
      partialPlan(planId, "server-first", {
        types: { [sharedTypeKey]: jsTypeWithReadableProperty("token") },
      }),
    ], hooks);
    registry.loadPartialSlot("second-slot", [
      partialPlan(planId, "server-second", {
        types: { [sharedTypeKey]: jsTypeWithReadableProperty("token") },
      }),
    ], hooks);

    registry.unloadPartialSlot("first-slot");

    expect(registry.get(planId)?.types[sharedTypeKey]).toBeDefined();
    expect(registry.get(planId)?.types[sharedTypeKey].properties.token).toBeDefined();

    registry.unloadPartialSlot("second-slot");

    expect(registry.get(planId)).toBeUndefined();
  });

  it("removes only the unloaded partial type fragment from a shared type key", () => {
    const registry = new PlanRegistry();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const sharedTypeKey = "native.component.shared-drawer";

    registry.loadPartialSlot("first-slot", [
      partialPlan(planId, "server-first", {
        types: { [sharedTypeKey]: jsTypeWithWritableProperty("classRemove") },
      }),
    ], hooks);
    registry.loadPartialSlot("second-slot", [
      partialPlan(planId, "server-second", {
        types: { [sharedTypeKey]: jsTypeWithWritableProperty("classToggle") },
      }),
    ], hooks);

    expect(Object.keys(registry.get(planId)?.types[sharedTypeKey].properties ?? {}))
      .toEqual(["classRemove", "classToggle"]);

    registry.unloadPartialSlot("first-slot");

    expect(Object.keys(registry.get(planId)?.types[sharedTypeKey].properties ?? {}))
      .toEqual(["classToggle"]);

    registry.unloadPartialSlot("second-slot");

    expect(registry.get(planId)).toBeUndefined();
  });

  it("rejects a shared type key when the same member has an incompatible contract", () => {
    const registry = new PlanRegistry();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const sharedTypeKey = "plugin.address";

    registry.loadPartialSlot("first-slot", [
      partialPlan(planId, "server-first", {
        types: { [sharedTypeKey]: jsTypeWithReadableProperty("token") },
      }),
    ], hooks);

    expect(() => registry.loadPartialSlot("second-slot", [
      partialPlan(planId, "server-second", {
        types: { [sharedTypeKey]: jsTypeWithPropertyShape("token", { kind: "number" }) },
      }),
    ], hooks)).toThrow('partial plan contribution "second-slot" cannot declare type "plugin.address"');
  });

  it("lets a later partial reuse a member differently after the previous fragment unloads", () => {
    const registry = new PlanRegistry();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const sharedTypeKey = "native.component.shared-drawer";

    registry.loadPartialSlot("first-slot", [
      partialPlan(planId, "server-first", {
        types: { [sharedTypeKey]: jsTypeWithReadableProperty("token") },
      }),
    ], hooks);

    registry.unloadPartialSlot("first-slot");

    registry.loadPartialSlot("second-slot", [
      partialPlan(planId, "server-second", {
        types: { [sharedTypeKey]: jsTypeWithPropertyShape("token", { kind: "number" }) },
      }),
    ], hooks);

    expect(registry.get(planId)?.types[sharedTypeKey].properties.token.shape)
      .toEqual({ kind: "number" });
  });

  it("merges compatible type fragments for a root-owned app-level component reference", () => {
    const registry = new PlanRegistry();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const drawerTypeKey = "native.component.alis-drawer";

    registry.register({
      ...rootPlan(planId),
      types: {
        [drawerTypeKey]: jsTypeWithWritableProperty("classAdd"),
      },
      components: {
        "alis-drawer": component("alis-drawer", drawerTypeKey),
      },
    });

    const merged = registry.loadPartialSlot("alis-drawer-content", [
      partialPlan(planId, "server-form", {
        types: {
          [drawerTypeKey]: jsTypeWithWritableProperty("classRemove"),
        },
        components: {
          "alis-drawer": component("alis-drawer", drawerTypeKey),
        },
        behaviors: [behavior()],
      }),
    ], hooks).loadedPlans[0];

    expect(merged.components["alis-drawer"]).toBeDefined();
    expect(Object.keys(merged.types[drawerTypeKey].properties)).toEqual(["classAdd", "classRemove"]);

    registry.unloadPartialSlot("alis-drawer-content");

    expect(registry.get(planId)?.components["alis-drawer"]).toBeDefined();
    expect(registry.get(planId)?.types[drawerTypeKey]).toBeDefined();
    expect(Object.keys(registry.get(planId)?.types[drawerTypeKey].properties ?? {}))
      .toEqual(["classAdd"]);
  });

  it("removes a partial type fragment from a root-owned app-level component reference", () => {
    const registry = new PlanRegistry();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const drawerTypeKey = "native.component.alis-drawer";

    registry.register({
      ...rootPlan(planId),
      types: {
        [drawerTypeKey]: jsTypeWithWritableProperty("classAdd"),
      },
      components: {
        "alis-drawer": component("alis-drawer", drawerTypeKey),
      },
    });

    registry.loadPartialSlot("first-drawer-content", [
      partialPlan(planId, "server-first", {
        types: {
          [drawerTypeKey]: jsTypeWithWritableProperty("classRemove"),
        },
        components: {
          "alis-drawer": component("alis-drawer", drawerTypeKey),
        },
      }),
    ], hooks);
    registry.loadPartialSlot("second-drawer-content", [
      partialPlan(planId, "server-second", {
        types: {
          [drawerTypeKey]: jsTypeWithWritableProperty("classToggle"),
        },
        components: {
          "alis-drawer": component("alis-drawer", drawerTypeKey),
        },
      }),
    ], hooks);

    expect(Object.keys(registry.get(planId)?.types[drawerTypeKey].properties ?? {}))
      .toEqual(["classAdd", "classRemove", "classToggle"]);

    registry.unloadPartialSlot("first-drawer-content");

    expect(registry.get(planId)?.components["alis-drawer"]).toBeDefined();
    expect(Object.keys(registry.get(planId)?.types[drawerTypeKey].properties ?? {}))
      .toEqual(["classAdd", "classToggle"]);

    registry.unloadPartialSlot("second-drawer-content");

    expect(Object.keys(registry.get(planId)?.types[drawerTypeKey].properties ?? {}))
      .toEqual(["classAdd"]);
  });

  it("lets multiple partial slots share one layout-owned app component", () => {
    const registry = new PlanRegistry();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const toastTypeKey = "fusion.component.alisFusionToast";

    registry.register(rootPlan(planId));

    registry.loadPartialSlot("first-toast-slot", [
      partialPlan(planId, "first-toast-plan", {
        types: {
          [toastTypeKey]: jsTypeWithWritableProperty("title"),
        },
        components: {
          alisFusionToast: layoutComponent("alisFusionToast", toastTypeKey),
        },
      }),
    ], hooks);
    registry.loadPartialSlot("second-toast-slot", [
      partialPlan(planId, "second-toast-plan", {
        types: {
          [toastTypeKey]: jsTypeWithWritableProperty("content"),
        },
        components: {
          alisFusionToast: layoutComponent("alisFusionToast", toastTypeKey),
        },
      }),
    ], hooks);

    expect(registry.get(planId)?.components.alisFusionToast).toBeDefined();
    expect(Object.keys(registry.get(planId)?.types[toastTypeKey].properties ?? {}))
      .toEqual(["title", "content"]);

    registry.unloadPartialSlot("first-toast-slot");

    expect(registry.get(planId)?.components.alisFusionToast).toBeDefined();
    expect(Object.keys(registry.get(planId)?.types[toastTypeKey].properties ?? {}))
      .toEqual(["content"]);

    registry.unloadPartialSlot("second-toast-slot");

    expect(registry.get(planId)?.components.alisFusionToast).toBeUndefined();
    expect(registry.get(planId)?.types[toastTypeKey]).toBeUndefined();
  });

  it("rejects owned component state for a layout-owned app component key", () => {
    const registry = new PlanRegistry();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const loaderTypeKey = "native.component.alis-loader";

    registry.register(rootPlan(planId));
    registry.loadPartialSlot("loader-slot", [
      partialPlan(planId, "loader-plan", {
        types: {
          [loaderTypeKey]: jsTypeWithWritableProperty("classAdd"),
        },
        components: {
          "alis-loader": layoutComponent("alis-loader", loaderTypeKey),
        },
      }),
    ], hooks);

    expect(() => registry.loadPartialSlot("owned-loader-slot", [
      partialPlan(planId, "owned-loader-plan", {
        types: {
          [loaderTypeKey]: jsTypeWithWritableProperty("classRemove"),
        },
        components: {
          "alis-loader": component("alis-loader", loaderTypeKey, { kind: "owned-definition" }),
        },
      }),
    ], hooks)).toThrow('partial plan contribution "owned-loader-slot" cannot declare component "alis-loader"');

    registry.unloadPartialSlot("loader-slot");

    expect(registry.get(planId)?.components["alis-loader"]).toBeUndefined();
    expect(registry.get(planId)?.types[loaderTypeKey]).toBeUndefined();
  });

  it("rejects a layout-object contribution that carries binding state", () => {
    const registry = new PlanRegistry();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const drawerTypeKey = "native.component.alis-drawer";
    const invalidLayoutObject = layoutComponent("alis-drawer", drawerTypeKey);
    invalidLayoutObject.binding = {
      kind: "registered-input",
      bindingPath: "Drawer",
      valueMember: "value",
    };

    registry.register(rootPlan(planId));

    expect(() => registry.loadPartialSlot("drawer-slot", [
      partialPlan(planId, "drawer-plan", {
        types: {
          [drawerTypeKey]: jsTypeWithWritableProperty("classAdd"),
        },
        components: {
          "alis-drawer": invalidLayoutObject,
        },
      }),
    ], hooks)).toThrow('partial plan contribution "drawer-slot" cannot declare component "alis-drawer"');

    expect(registry.get(planId)?.components["alis-drawer"]).toBeUndefined();
  });

  it("rejects a layout-object reference with a mismatched runtime identity", () => {
    const registry = new PlanRegistry();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const drawerTypeKey = "native.component.alis-drawer";

    registry.register({
      ...rootPlan(planId),
      types: {
        [drawerTypeKey]: jsTypeWithWritableProperty("classAdd"),
      },
      components: {
        "alis-drawer": layoutComponent("alis-drawer", drawerTypeKey),
      },
    });

    expect(() => registry.loadPartialSlot("drawer-slot", [
      partialPlan(planId, "drawer-plan", {
        types: {
          [drawerTypeKey]: jsTypeWithWritableProperty("classRemove"),
        },
        components: {
          "alis-drawer": layoutComponent("other-drawer", drawerTypeKey),
        },
      }),
    ], hooks)).toThrow('partial plan contribution "drawer-slot" cannot declare component "alis-drawer"');

    expect(Object.keys(registry.get(planId)?.types[drawerTypeKey].properties ?? {}))
      .toEqual(["classAdd"]);
  });

  it("keeps a layout object when the root declares it after a partial materializes it", () => {
    const registry = new PlanRegistry();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const toastTypeKey = "fusion.component.alisFusionToast";

    registry.register(rootPlan(planId));
    registry.loadPartialSlot("toast-slot", [
      partialPlan(planId, "toast-plan", {
        types: {
          [toastTypeKey]: jsTypeWithWritableProperty("title"),
        },
        components: {
          alisFusionToast: layoutComponent("alisFusionToast", toastTypeKey),
        },
      }),
    ], hooks);
    registry.add({
      ...rootPlan(planId),
      types: {
        [toastTypeKey]: jsTypeWithWritableProperty("content"),
      },
      components: {
        alisFusionToast: layoutComponent("alisFusionToast", toastTypeKey),
      },
    }, hooks);

    registry.unloadPartialSlot("toast-slot");

    expect(registry.get(planId)?.components.alisFusionToast).toBeDefined();
    expect(Object.keys(registry.get(planId)?.types[toastTypeKey].properties ?? {}))
      .toEqual(["content"]);
  });

  it("rejects a partial owned definition for a root-owned component key", () => {
    const registry = new PlanRegistry();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const drawerTypeKey = "native.component.alis-drawer";

    registry.register({
      ...rootPlan(planId),
      types: {
        [drawerTypeKey]: jsTypeWithWritableProperty("classAdd"),
      },
      components: {
        "alis-drawer": component("alis-drawer", drawerTypeKey),
      },
    });

    expect(() => registry.loadPartialSlot("alis-drawer-content", [
      partialPlan(planId, "server-form", {
        types: {
          [drawerTypeKey]: jsTypeWithWritableProperty("classRemove"),
        },
        components: {
          "alis-drawer": component("alis-drawer", drawerTypeKey, { kind: "owned-definition" }),
        },
      }),
    ], hooks)).toThrow('partial plan contribution "alis-drawer-content" cannot declare component "alis-drawer"');

    expect(Object.keys(registry.get(planId)?.types[drawerTypeKey].properties ?? {}))
      .toEqual(["classAdd"]);
  });

  it("rejects an object target contribution that carries owned binding state", () => {
    const registry = new PlanRegistry();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const componentId = "care-unit";
    const typeKey = "native.component.care-unit";
    const invalidObjectTarget = registeredInputComponent(componentId, typeKey);
    invalidObjectTarget.contribution = { kind: "object-target" };

    registry.register({
      ...rootPlan(planId),
      types: {
        [typeKey]: jsTypeWithReadableProperty("value"),
      },
      components: {
        [componentId]: component(componentId, typeKey),
      },
    });

    expect(() => registry.loadPartialSlot("care-unit-editor", [
      partialPlan(planId, "server-editor", {
        types: {
          [typeKey]: jsTypeWithWritableProperty("value"),
        },
        components: {
          [componentId]: invalidObjectTarget,
        },
      }),
    ], hooks)).toThrow('partial plan contribution "care-unit-editor" cannot declare component "care-unit"');

    expect(registry.get(planId)?.components[componentId].binding.kind).toBe("none");
  });

  it("recomputes merged property access when a partial type fragment unloads", () => {
    const registry = new PlanRegistry();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const typeKey = "native.component.care-unit";

    registry.register({
      ...rootPlan(planId),
      types: {
        [typeKey]: jsTypeWithPropertyAccess("value", "read"),
      },
      components: {
        "care-unit": component("care-unit", typeKey),
      },
    });

    registry.loadPartialSlot("care-unit-editor", [
      partialPlan(planId, "server-editor", {
        types: {
          [typeKey]: jsTypeWithPropertyAccess("value", "write"),
        },
        components: {
          "care-unit": component("care-unit", typeKey),
        },
      }),
    ], hooks);

    expect(registry.get(planId)?.types[typeKey].properties.value.access).toBe("readwrite");

    registry.unloadPartialSlot("care-unit-editor");

    expect(registry.get(planId)?.types[typeKey].properties.value.access).toBe("read");
  });

  it("allows a partial behavior to reference a root-owned injection host element", () => {
    const registry = new PlanRegistry();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const hostTypeKey = "native.element.step-container";

    registry.register({
      ...rootPlan(planId),
      types: {
        [hostTypeKey]: jsTypeWithWritableProperty("html"),
      },
      components: {
        "step-container": component("step-container", hostTypeKey),
      },
    });

    registry.loadPartialSlot("step-container", [
      partialPlan(planId, "server-step", {
        types: {
          [hostTypeKey]: jsTypeWithWritableProperty("hidden"),
        },
        components: {
          "step-container": component("step-container", hostTypeKey),
        },
        behaviors: [behavior()],
      }),
    ], hooks);

    expect(Object.keys(registry.get(planId)?.types[hostTypeKey].properties ?? {}))
      .toEqual(["html", "hidden"]);

    registry.unloadPartialSlot("step-container");

    expect(registry.get(planId)?.components["step-container"]).toBeDefined();
    expect(Object.keys(registry.get(planId)?.types[hostTypeKey].properties ?? {}))
      .toEqual(["html"]);
  });

  it("scopes component and type ownership to the runtime plan document", () => {
    const registry = new PlanRegistry();
    const { hooks } = mergeHooks();
    const rootPlanId = "Drawer.Root";
    const partialPlanId = "DrawerResident.Partial";
    const drawerTypeKey = "native.component.alis-drawer";

    registry.register({
      ...rootPlan(rootPlanId),
      types: {
        [drawerTypeKey]: jsTypeWithWritableProperty("classAdd"),
      },
      components: {
        "alis-drawer": component("alis-drawer", drawerTypeKey),
      },
    });

    const loaded = registry.loadPartialSlot("alis-drawer-content", [
      partialPlan(partialPlanId, "server-form", {
        types: {
          [drawerTypeKey]: jsTypeWithWritableProperty("classRemove"),
        },
        components: {
          "alis-drawer": component("alis-drawer", drawerTypeKey),
        },
        behaviors: [behavior()],
      }),
    ], hooks).loadedPlans[0];

    expect(loaded.planId).toBe(partialPlanId);
    expect(loaded.components["alis-drawer"]).toBeDefined();
    expect(registry.get(rootPlanId)?.components["alis-drawer"]).toBeDefined();

    registry.unloadPartialSlot("alis-drawer-content");

    expect(registry.get(partialPlanId)).toBeUndefined();
    expect(registry.get(rootPlanId)?.components["alis-drawer"]).toBeDefined();
  });

  it("merges validation rules on a shared container by component key", () => {
    const registry = new PlanRegistry();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";

    registry.register(rootPlan(planId));

    registry.add(
      {
        ...rootPlan(planId),
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("first-name"),
            validationRule("zip-code", "old zip required"),
          ]),
        },
      },
      hooks,
    );

    const merged = registry.add(
      {
        ...rootPlan(planId),
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("zip-code", "new zip required"),
            validationRule("city"),
          ]),
        },
      },
      hooks,
    );

    const container = merged.components["resident-form"].container;
    expect(container.kind).toBe("validation-container");
    if (container.kind !== "validation-container") return;

    expect(container.validationRules.map(rule => rule.component)).toEqual([
      "first-name",
      "zip-code",
      "city",
    ]);
    expect(container.validationRules.find(rule => rule.component === "zip-code")?.rules[0]?.message)
      .toBe("new zip required");
  });

  it("rejects a validation container extension with a mismatched runtime identity", () => {
    const registry = new PlanRegistry();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";

    registry.register({
      ...rootPlan(planId),
      components: {
        "resident-form": validationContainer("resident-form", [validationRule("first-name")]),
      },
    });

    expect(() => registry.loadPartialSlot("address-slot", [
      partialPlan(planId, "address-form", {
        components: {
          "resident-form": validationContainer("other-form", [validationRule("city")]),
        },
      }),
    ], hooks)).toThrow('partial plan contribution "address-slot" cannot declare component "resident-form"');

    const container = registry.get(planId)?.components["resident-form"].container;
    expect(container?.kind).toBe("validation-container");
    if (container?.kind !== "validation-container") return;
    expect(container.validationRules.map(rule => rule.component)).toEqual(["first-name"]);
  });

  it("rejects a validation container extension that carries registered input binding state", () => {
    const registry = new PlanRegistry();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const invalidExtension = validationContainer("resident-form", [validationRule("city")]);
    invalidExtension.binding = {
      kind: "registered-input",
      bindingPath: "ResidentForm",
      valueMember: "value",
    };

    registry.register({
      ...rootPlan(planId),
      components: {
        "resident-form": validationContainer("resident-form", [validationRule("first-name")]),
      },
    });

    expect(() => registry.loadPartialSlot("address-slot", [
      partialPlan(planId, "address-form", {
        components: {
          "resident-form": invalidExtension,
        },
      }),
    ], hooks)).toThrow('partial plan contribution "address-slot" cannot declare component "resident-form"');

    const container = registry.get(planId)?.components["resident-form"].container;
    expect(container?.kind).toBe("validation-container");
    if (container?.kind !== "validation-container") return;
    expect(container.validationRules.map(rule => rule.component)).toEqual(["first-name"]);
  });

  it("unloads a partial slot explicitly", () => {
    const registry = new PlanRegistry();
    const { hooks, behaviorSignals } = mergeHooks();
    const planId = "Resident.Root";
    const loadedBehavior = behavior();

    registry.add(
      partialPlan(planId, "address-slot", {
        types: { "native.element.address-line": jsType() },
        components: { "address-line": component("address-line") },
        behaviors: [loadedBehavior],
      }),
      hooks,
    );

    expect(registry.get(planId)?.components["address-line"]).toBeDefined();
    expect(behaviorSignals[0]?.aborted).toBe(false);

    const result = registry.unloadPartialSlot("address-slot");

    expect(result.affectedPlanIds).toEqual([planId]);
    expect(behaviorSignals[0]?.aborted).toBe(true);
    expect(registry.get(planId)).toBeUndefined();
  });

  it("preserves root-owned validation containers when unloading a partial slot", () => {
    const registry = new PlanRegistry();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";

    registry.register({
      ...rootPlan(planId),
      components: {
        "resident-form": validationContainer("resident-form", [
          validationRule("first-name"),
        ]),
      },
    });

    registry.loadPartialSlot("address-slot", [
      partialPlan(planId, "server-part-id", {
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("address-line"),
          ]),
        },
      }),
    ], hooks);

    const loadedContainer = registry.get(planId)?.components["resident-form"].container;
    expect(loadedContainer?.kind).toBe("validation-container");
    if (loadedContainer?.kind !== "validation-container") return;
    expect(loadedContainer.validationRules.map(rule => rule.component)).toEqual([
      "first-name",
      "address-line",
    ]);

    registry.unloadPartialSlot("address-slot");

    const unloadedContainer = registry.get(planId)?.components["resident-form"].container;
    expect(unloadedContainer?.kind).toBe("validation-container");
    if (unloadedContainer?.kind !== "validation-container") return;
    expect(unloadedContainer.validationRules.map(rule => rule.component)).toEqual(["first-name"]);
  });

  it("rejects an empty partial slot load", () => {
    const registry = new PlanRegistry();
    const { hooks } = mergeHooks();

    expect(() => registry.loadPartialSlot("address-slot", [], hooks))
      .toThrow("unload the slot explicitly");
  });

  it("replaces a partial slot as one load even when the slot contains multiple plan documents", () => {
    const registry = new PlanRegistry();
    const { hooks, behaviorSignals } = mergeHooks();
    const residentPlanId = "Resident.Root";
    const billingPlanId = "Billing.Root";
    const residentBehavior = behavior();
    const billingBehavior = behavior();

    const result = registry.loadPartialSlot("drawer-slot", [
      partialPlan(residentPlanId, "server-part-id", {
        types: { "native.element.resident-name": jsType() },
        components: { "resident-name": component("resident-name") },
        behaviors: [residentBehavior],
      }),
      partialPlan(billingPlanId, "server-part-id", {
        types: { "native.element.invoice-total": jsType() },
        components: { "invoice-total": component("invoice-total") },
        behaviors: [billingBehavior],
      }),
    ], hooks);

    expect(result.affectedPlanIds).toEqual([residentPlanId, billingPlanId]);
    expect(result.loadedPlans.map(plan => plan.planId)).toEqual([residentPlanId, billingPlanId]);
    expect(registry.get(residentPlanId)?.components["resident-name"]).toBeDefined();
    expect(registry.get(billingPlanId)?.components["invoice-total"]).toBeDefined();
    expect(behaviorSignals[0]).toBe(behaviorSignals[1]);

    registry.unloadPartialSlot("drawer-slot");

    expect(behaviorSignals[0]?.aborted).toBe(true);
    expect(registry.get(residentPlanId)).toBeUndefined();
    expect(registry.get(billingPlanId)).toBeUndefined();
  });

  it("keeps a non-root merged plan alive while another partial still owns types", () => {
    const registry = new PlanRegistry();
    const { hooks } = mergeHooks();
    const planId = "Resident.Dynamic";

    registry.loadPartialSlot("type-slot", [
      partialPlan(planId, "server-type-plan", {
        types: { "native.element.shared": jsType() },
      }),
    ], hooks);
    registry.loadPartialSlot("component-slot", [
      partialPlan(planId, "server-component-plan", {
        components: { "address-line": component("address-line", "native.element.shared") },
      }),
    ], hooks);

    registry.unloadPartialSlot("component-slot");

    expect(registry.get(planId)?.components["address-line"]).toBeUndefined();
    expect(registry.get(planId)?.types["native.element.shared"]).toBeDefined();

    registry.unloadPartialSlot("type-slot");

    expect(registry.get(planId)).toBeUndefined();
  });
});

describe("initial plan composition", () => {
  it("assembles one boot plan per plan id while preserving contribution order", () => {
    const residentPlanId = "Resident.Root";
    const residentReady = behavior();
    const addressReady = behavior();
    const billingPlan = rootPlan("Billing.Root");

    const composed = composeInitialPlans([
      {
        ...rootPlan(residentPlanId),
        types: { "native.element.resident-name": jsType() },
        components: { "resident-name": component("resident-name") },
        behaviors: [residentReady],
      },
      partialPlan(residentPlanId, "address-slot", {
        types: { "native.element.address-line": jsType() },
        components: { "address-line": component("address-line") },
        behaviors: [addressReady],
      }),
      billingPlan,
    ]);

    expect(composed.map(plan => plan.planId)).toEqual([residentPlanId, "Billing.Root"]);

    const resident = composed[0];
    expect(resident.scope).toEqual({ kind: "root" });
    expect(Object.keys(resident.types)).toEqual([
      "native.element.resident-name",
      "native.element.address-line",
    ]);
    expect(Object.keys(resident.components)).toEqual(["resident-name", "address-line"]);
    expect(resident.behaviors).toEqual([residentReady, addressReady]);
    expect(composed[1]).toEqual(billingPlan);
  });

  it("emits a root-scoped boot plan even when a partial contribution appears first", () => {
    const planId = "Resident.Root";
    const partialReady = behavior();
    const rootReady = behavior();

    const composed = composeInitialPlans([
      partialPlan(planId, "address-slot", {
        types: { "native.element.address-line": jsType() },
        components: { "address-line": component("address-line") },
        behaviors: [partialReady],
      }),
      {
        ...rootPlan(planId),
        types: { "native.element.resident-name": jsType() },
        components: { "resident-name": component("resident-name") },
        behaviors: [rootReady],
      },
    ]);

    expect(composed).toHaveLength(1);
    expect(composed[0].scope).toEqual({ kind: "root" });
    expect(Object.keys(composed[0].components)).toEqual(["address-line", "resident-name"]);
    expect(composed[0].behaviors).toEqual([partialReady, rootReady]);
  });

  it("merges initial type fragments instead of letting root overwrite partial write access", () => {
    const planId = "Resident.Step";
    const componentId = "care-unit";
    const typeKey = "native.component.care-unit";

    const composed = composeInitialPlans([
      partialPlan(planId, "cognitive-section", {
        types: { [typeKey]: jsTypeWithWritableProperty("value") },
        components: { [componentId]: component(componentId, typeKey) },
      }),
      {
        ...rootPlan(planId),
        types: { [typeKey]: jsTypeWithReadableProperty("value") },
        components: { [componentId]: registeredInputComponent(componentId, typeKey) },
      },
    ]);

    expect(composed[0].types[typeKey].properties.value.access).toBe("readwrite");
    expect(composed[0].components[componentId].binding.kind).toBe("registered-input");
  });

  it("does not let an initial reference-only contribution erase a registered input definition", () => {
    const planId = "Resident.Step";
    const componentId = "care-unit";
    const typeKey = "native.component.care-unit";

    const composed = composeInitialPlans([
      {
        ...rootPlan(planId),
        types: { [typeKey]: jsTypeWithReadableProperty("value") },
        components: { [componentId]: registeredInputComponent(componentId, typeKey) },
      },
      partialPlan(planId, "cognitive-section", {
        types: { [typeKey]: jsTypeWithWritableProperty("value") },
        components: { [componentId]: component(componentId, typeKey) },
      }),
    ]);

    expect(composed[0].types[typeKey].properties.value.access).toBe("readwrite");
    expect(composed[0].components[componentId].binding.kind).toBe("registered-input");
  });

  it("coalesces duplicate initial owned component definitions from the first DOM", () => {
    const planId = "Resident.Step";
    const componentId = "resident-name";
    const typeKey = "native.component.resident-name";

    const composed = composeInitialPlans([
      {
        ...rootPlan(planId),
        types: { [typeKey]: jsTypeWithReadableProperty("value") },
        components: { [componentId]: registeredInputComponent(componentId, typeKey) },
      },
      partialPlan(planId, "clinical-section", {
        types: { [typeKey]: jsTypeWithWritableProperty("value") },
        components: { [componentId]: registeredInputComponent(componentId, typeKey) },
      }),
    ]);

    expect(composed[0].types[typeKey].properties.value.access).toBe("readwrite");
    expect(composed[0].components[componentId]).toEqual(registeredInputComponent(componentId, typeKey));
  });

  it("rejects duplicate initial owned component definitions with different binding state", () => {
    const planId = "Resident.Step";
    const componentId = "resident-name";
    const typeKey = "native.component.resident-name";
    const conflictingDefinition = registeredInputComponent(componentId, typeKey);
    if (conflictingDefinition.binding.kind !== "registered-input") return;
    conflictingDefinition.binding = {
      ...conflictingDefinition.binding,
      bindingPath: "Clinical.ResidentName",
    };

    expect(() => composeInitialPlans([
      {
        ...rootPlan(planId),
        types: { [typeKey]: jsTypeWithReadableProperty("value") },
        components: { [componentId]: registeredInputComponent(componentId, typeKey) },
      },
      partialPlan(planId, "clinical-section", {
        types: { [typeKey]: jsTypeWithWritableProperty("value") },
        components: { [componentId]: conflictingDefinition },
      }),
    ])).toThrow('partial plan contribution "clinical-section" cannot declare component "resident-name"');
  });

  it("rejects an initial object-target contribution that carries binding state", () => {
    const planId = "Resident.Step";
    const componentId = "care-unit";
    const typeKey = "native.component.care-unit";
    const invalidObjectTarget = registeredInputComponent(componentId, typeKey);
    invalidObjectTarget.contribution = { kind: "object-target" };

    expect(() => composeInitialPlans([
      {
        ...rootPlan(planId),
        types: { [typeKey]: jsTypeWithReadableProperty("value") },
        components: { [componentId]: registeredInputComponent(componentId, typeKey) },
      },
      partialPlan(planId, "cognitive-section", {
        types: { [typeKey]: jsTypeWithWritableProperty("value") },
        components: { [componentId]: invalidObjectTarget },
      }),
    ])).toThrow('partial plan contribution "cognitive-section" cannot declare component "care-unit"');
  });

  it("rejects an initial layout-object contribution with a mismatched runtime identity", () => {
    const planId = "Resident.Step";
    const componentId = "alisFusionToast";
    const typeKey = "fusion.component.alisFusionToast";

    expect(() => composeInitialPlans([
      {
        ...rootPlan(planId),
        types: { [typeKey]: jsTypeWithWritableProperty("title") },
        components: { [componentId]: layoutComponent(componentId, typeKey) },
      },
      partialPlan(planId, "toast-section", {
        types: { [typeKey]: jsTypeWithWritableProperty("content") },
        components: { [componentId]: layoutComponent("otherToast", typeKey) },
      }),
    ])).toThrow('partial plan contribution "toast-section" cannot declare component "alisFusionToast"');
  });

  it("rejects an initial layout-object contribution that carries binding state", () => {
    const planId = "Resident.Step";
    const componentId = "alisFusionToast";
    const typeKey = "fusion.component.alisFusionToast";
    const invalidLayoutObject = layoutComponent(componentId, typeKey);
    invalidLayoutObject.binding = {
      kind: "registered-input",
      bindingPath: "Toast",
      valueMember: "value",
    };

    expect(() => composeInitialPlans([
      {
        ...rootPlan(planId),
        types: { [typeKey]: jsTypeWithWritableProperty("title") },
        components: { [componentId]: layoutComponent(componentId, typeKey) },
      },
      partialPlan(planId, "toast-section", {
        types: { [typeKey]: jsTypeWithWritableProperty("content") },
        components: { [componentId]: invalidLayoutObject },
      }),
    ])).toThrow('partial plan contribution "toast-section" cannot declare component "alisFusionToast"');
  });

  it("merges an initial layout-object reference without replacing the root component", () => {
    const planId = "Resident.Step";
    const componentId = "alisFusionToast";
    const typeKey = "fusion.component.alisFusionToast";

    const composed = composeInitialPlans([
      {
        ...rootPlan(planId),
        types: { [typeKey]: jsTypeWithWritableProperty("title") },
        components: { [componentId]: layoutComponent(componentId, typeKey) },
      },
      partialPlan(planId, "toast-section", {
        types: { [typeKey]: jsTypeWithWritableProperty("content") },
        components: { [componentId]: layoutComponent(componentId, typeKey) },
      }),
    ]);

    expect(composed[0].components[componentId]).toEqual(layoutComponent(componentId, typeKey));
    expect(Object.keys(composed[0].types[typeKey].properties)).toEqual(["title", "content"]);
  });

  it("lets an initial partial layout object appear before the root contribution", () => {
    const planId = "Resident.Step";
    const componentId = "alisFusionToast";
    const typeKey = "fusion.component.alisFusionToast";

    const composed = composeInitialPlans([
      partialPlan(planId, "toast-section", {
        types: { [typeKey]: jsTypeWithWritableProperty("content") },
        components: { [componentId]: layoutComponent(componentId, typeKey) },
      }),
      {
        ...rootPlan(planId),
        types: { [typeKey]: jsTypeWithWritableProperty("title") },
        components: { [componentId]: layoutComponent(componentId, typeKey) },
      },
    ]);

    expect(composed[0].scope).toEqual({ kind: "root" });
    expect(composed[0].components[componentId]).toEqual(layoutComponent(componentId, typeKey));
    expect(Object.keys(composed[0].types[typeKey].properties)).toEqual(["content", "title"]);
  });

  it("uses component merge semantics when composing validation containers", () => {
    const planId = "Resident.Root";
    const composed = composeInitialPlans([
      {
        ...rootPlan(planId),
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("first-name"),
            validationRule("zip-code", "old zip required"),
          ]),
        },
      },
      partialPlan(planId, "address-slot", {
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("zip-code", "new zip required"),
            validationRule("city"),
          ]),
        },
      }),
    ]);

    const container = composed[0].components["resident-form"].container;
    expect(container.kind).toBe("validation-container");
    if (container.kind !== "validation-container") return;

    expect(container.validationRules.map(rule => rule.component)).toEqual([
      "first-name",
      "zip-code",
      "city",
    ]);
    expect(container.validationRules.find(rule => rule.component === "zip-code")?.rules[0]?.message)
      .toBe("new zip required");
  });

  it("rejects an initial validation-container contribution with a mismatched runtime identity", () => {
    const planId = "Resident.Root";

    expect(() => composeInitialPlans([
      {
        ...rootPlan(planId),
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("first-name"),
          ]),
        },
      },
      partialPlan(planId, "address-slot", {
        components: {
          "resident-form": validationContainer("other-form", [
            validationRule("city"),
          ]),
        },
      }),
    ])).toThrow('partial plan contribution "address-slot" cannot declare component "resident-form"');
  });

  it("rejects an initial validation-container contribution that carries binding state", () => {
    const planId = "Resident.Root";
    const invalidContainer = validationContainer("resident-form", [
      validationRule("city"),
    ]);
    invalidContainer.binding = {
      kind: "registered-input",
      bindingPath: "Resident",
      valueMember: "value",
    };

    expect(() => composeInitialPlans([
      {
        ...rootPlan(planId),
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("first-name"),
          ]),
        },
      },
      partialPlan(planId, "address-slot", {
        components: {
          "resident-form": invalidContainer,
        },
      }),
    ])).toThrow('partial plan contribution "address-slot" cannot declare component "resident-form"');
  });
});
