import { describe, expect, it } from "vitest";
import { PlanRegistry } from "../../lifecycle/merge-plan";
import {
  component,
  jsType,
  jsTypeWithWritableProperty,
  layoutComponent,
  mergeHooks,
  partialPlan,
  registeredInputComponent,
  rootPlan,
} from "../support/plan-lifecycle-fixtures";

describe("component contribution ownership", () => {
  it("rejects a component key owned by a different partial slot", () => {
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

  it("keeps type fragments out when component preflight rejects the contribution", () => {
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

  it("rejects an object-target contribution that carries owned binding state", () => {
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
        [typeKey]: jsTypeWithWritableProperty("value"),
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
});

describe("layout object contributions", () => {
  it("lets multiple slots share one layout-owned app component", () => {
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
});
