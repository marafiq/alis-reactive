import { describe, expect, it } from "vitest";
import { PlanRegistry } from "../../lifecycle/merge-plan";
import {
  behavior,
  component,
  jsTypeWithPropertyAccess,
  jsTypeWithPropertyShape,
  jsTypeWithReadableProperty,
  jsTypeWithWritableProperty,
  mergeHooks,
  partialPlan,
  rootPlan,
} from "../support/plan-lifecycle-fixtures";

describe("browser object contract fragments", () => {
  it("shares an identical type contract across slots until the last owner unloads", () => {
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

  it("removes only the unloaded fragment from a shared type key", () => {
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

  it("lets a later slot reuse a member differently after the previous fragment unloads", () => {
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

  it("merges compatible fragments for a root-owned app-level component reference", () => {
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
    expect(Object.keys(registry.get(planId)?.types[drawerTypeKey].properties ?? {}))
      .toEqual(["classAdd"]);
  });

  it("removes one partial fragment from a root-owned app-level component reference", () => {
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

  it("recomputes merged property access when a partial fragment unloads", () => {
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
});
