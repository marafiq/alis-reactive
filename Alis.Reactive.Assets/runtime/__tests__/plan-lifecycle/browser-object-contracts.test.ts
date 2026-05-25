import { describe, expect, it } from "vitest";
import { AppliedBrowserPlans } from "../../lifecycle/merge-plan";
import {
  behavior,
  component,
  jsTypeWithMethodShape,
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
    const browserPlans = new AppliedBrowserPlans();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const sharedTypeKey = "plugin.address";

    browserPlans.loadPartialSlot("first-slot", [
      partialPlan(planId, "server-first", {
        types: { [sharedTypeKey]: jsTypeWithReadableProperty("token") },
      }),
    ], hooks);
    browserPlans.loadPartialSlot("second-slot", [
      partialPlan(planId, "server-second", {
        types: { [sharedTypeKey]: jsTypeWithReadableProperty("token") },
      }),
    ], hooks);

    browserPlans.unloadPartialSlot("first-slot");

    expect(browserPlans.get(planId)?.types[sharedTypeKey]).toBeDefined();
    expect(browserPlans.get(planId)?.types[sharedTypeKey].properties.token).toBeDefined();

    browserPlans.unloadPartialSlot("second-slot");

    expect(browserPlans.get(planId)).toBeUndefined();
  });

  it("removes only the unloaded fragment from a shared type key", () => {
    const browserPlans = new AppliedBrowserPlans();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const sharedTypeKey = "native.component.shared-drawer";

    browserPlans.loadPartialSlot("first-slot", [
      partialPlan(planId, "server-first", {
        types: { [sharedTypeKey]: jsTypeWithWritableProperty("classRemove") },
      }),
    ], hooks);
    browserPlans.loadPartialSlot("second-slot", [
      partialPlan(planId, "server-second", {
        types: { [sharedTypeKey]: jsTypeWithWritableProperty("classToggle") },
      }),
    ], hooks);

    expect(Object.keys(browserPlans.get(planId)?.types[sharedTypeKey].properties ?? {}))
      .toEqual(["classRemove", "classToggle"]);

    browserPlans.unloadPartialSlot("first-slot");

    expect(Object.keys(browserPlans.get(planId)?.types[sharedTypeKey].properties ?? {}))
      .toEqual(["classToggle"]);

    browserPlans.unloadPartialSlot("second-slot");

    expect(browserPlans.get(planId)).toBeUndefined();
  });

  it("rejects a shared type key when the same member has an incompatible contract", () => {
    const browserPlans = new AppliedBrowserPlans();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const sharedTypeKey = "plugin.address";

    browserPlans.loadPartialSlot("first-slot", [
      partialPlan(planId, "server-first", {
        types: { [sharedTypeKey]: jsTypeWithReadableProperty("token") },
      }),
    ], hooks);

    expect(() => browserPlans.loadPartialSlot("second-slot", [
      partialPlan(planId, "server-second", {
        types: { [sharedTypeKey]: jsTypeWithPropertyShape("token", { kind: "number" }) },
      }),
    ], hooks)).toThrow('partial plan contribution "second-slot" cannot declare type "plugin.address"');
  });

  it("refines compatible property shapes when object contract fragments merge", () => {
    const browserPlans = new AppliedBrowserPlans();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const sharedTypeKey = "plugin.address";

    browserPlans.loadPartialSlot("first-slot", [
      partialPlan(planId, "server-first", {
        types: { [sharedTypeKey]: jsTypeWithPropertyShape("token", { kind: "any" }) },
      }),
    ], hooks);
    browserPlans.loadPartialSlot("second-slot", [
      partialPlan(planId, "server-second", {
        types: { [sharedTypeKey]: jsTypeWithPropertyShape("token", { kind: "string" }) },
      }),
    ], hooks);

    expect(browserPlans.get(planId)?.types[sharedTypeKey].properties.token.shape)
      .toEqual({ kind: "string" });

    browserPlans.unloadPartialSlot("second-slot");

    expect(browserPlans.get(planId)?.types[sharedTypeKey].properties.token.shape)
      .toEqual({ kind: "any" });
  });

  it("merges compatible object property fields from separate fragments", () => {
    const browserPlans = new AppliedBrowserPlans();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const sharedTypeKey = "plugin.resident";

    browserPlans.loadPartialSlot("name-slot", [
      partialPlan(planId, "server-name", {
        types: {
          [sharedTypeKey]: jsTypeWithPropertyShape("profile", {
            kind: "object",
            fields: { name: { kind: "string" } },
            additional: false,
          }),
        },
      }),
    ], hooks);
    browserPlans.loadPartialSlot("age-slot", [
      partialPlan(planId, "server-age", {
        types: {
          [sharedTypeKey]: jsTypeWithPropertyShape("profile", {
            kind: "object",
            fields: { age: { kind: "number" } },
            additional: false,
          }),
        },
      }),
    ], hooks);

    expect(browserPlans.get(planId)?.types[sharedTypeKey].properties.profile.shape)
      .toEqual({
        kind: "object",
        fields: {
          name: { kind: "string" },
          age: { kind: "number" },
        },
        additional: false,
      });
  });

  it("merges compatible method argument and return shapes", () => {
    const browserPlans = new AppliedBrowserPlans();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const sharedTypeKey = "plugin.address";

    browserPlans.loadPartialSlot("first-slot", [
      partialPlan(planId, "server-first", {
        types: { [sharedTypeKey]: jsTypeWithMethodShape("normalize", { kind: "any" }, { kind: "any" }) },
      }),
    ], hooks);
    browserPlans.loadPartialSlot("second-slot", [
      partialPlan(planId, "server-second", {
        types: { [sharedTypeKey]: jsTypeWithMethodShape("normalize", { kind: "string" }, { kind: "string" }) },
      }),
    ], hooks);

    const method = browserPlans.get(planId)?.types[sharedTypeKey].methods.normalize;
    expect(method?.arguments).toEqual({ kind: "exact", shapes: [{ kind: "string" }] });
    expect(method?.returns).toEqual({ kind: "string" });
  });

  it("lets a later slot reuse a member differently after the previous fragment unloads", () => {
    const browserPlans = new AppliedBrowserPlans();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const sharedTypeKey = "native.component.shared-drawer";

    browserPlans.loadPartialSlot("first-slot", [
      partialPlan(planId, "server-first", {
        types: { [sharedTypeKey]: jsTypeWithReadableProperty("token") },
      }),
    ], hooks);

    browserPlans.unloadPartialSlot("first-slot");

    browserPlans.loadPartialSlot("second-slot", [
      partialPlan(planId, "server-second", {
        types: { [sharedTypeKey]: jsTypeWithPropertyShape("token", { kind: "number" }) },
      }),
    ], hooks);

    expect(browserPlans.get(planId)?.types[sharedTypeKey].properties.token.shape)
      .toEqual({ kind: "number" });
  });

  it("merges compatible fragments for a root-owned app-level component reference", () => {
    const browserPlans = new AppliedBrowserPlans();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const drawerTypeKey = "native.component.alis-drawer";

    browserPlans.register({
      ...rootPlan(planId),
      types: {
        [drawerTypeKey]: jsTypeWithWritableProperty("classAdd"),
      },
      components: {
        "alis-drawer": component("alis-drawer", drawerTypeKey),
      },
    });

    browserPlans.loadPartialSlot("alis-drawer-content", [
      partialPlan(planId, "server-form", {
        types: {
          [drawerTypeKey]: jsTypeWithWritableProperty("classRemove"),
        },
        components: {
          "alis-drawer": component("alis-drawer", drawerTypeKey),
        },
        behaviors: [behavior()],
      }),
    ], hooks);

    const merged = browserPlans.get(planId)!;
    expect(merged.components["alis-drawer"]).toBeDefined();
    expect(Object.keys(merged.types[drawerTypeKey].properties)).toEqual(["classAdd", "classRemove"]);

    browserPlans.unloadPartialSlot("alis-drawer-content");

    expect(browserPlans.get(planId)?.components["alis-drawer"]).toBeDefined();
    expect(Object.keys(browserPlans.get(planId)?.types[drawerTypeKey].properties ?? {}))
      .toEqual(["classAdd"]);
  });

  it("removes one partial fragment from a root-owned app-level component reference", () => {
    const browserPlans = new AppliedBrowserPlans();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const drawerTypeKey = "native.component.alis-drawer";

    browserPlans.register({
      ...rootPlan(planId),
      types: {
        [drawerTypeKey]: jsTypeWithWritableProperty("classAdd"),
      },
      components: {
        "alis-drawer": component("alis-drawer", drawerTypeKey),
      },
    });

    browserPlans.loadPartialSlot("first-drawer-content", [
      partialPlan(planId, "server-first", {
        types: {
          [drawerTypeKey]: jsTypeWithWritableProperty("classRemove"),
        },
        components: {
          "alis-drawer": component("alis-drawer", drawerTypeKey),
        },
      }),
    ], hooks);
    browserPlans.loadPartialSlot("second-drawer-content", [
      partialPlan(planId, "server-second", {
        types: {
          [drawerTypeKey]: jsTypeWithWritableProperty("classToggle"),
        },
        components: {
          "alis-drawer": component("alis-drawer", drawerTypeKey),
        },
      }),
    ], hooks);

    expect(Object.keys(browserPlans.get(planId)?.types[drawerTypeKey].properties ?? {}))
      .toEqual(["classAdd", "classRemove", "classToggle"]);

    browserPlans.unloadPartialSlot("first-drawer-content");

    expect(browserPlans.get(planId)?.components["alis-drawer"]).toBeDefined();
    expect(Object.keys(browserPlans.get(planId)?.types[drawerTypeKey].properties ?? {}))
      .toEqual(["classAdd", "classToggle"]);

    browserPlans.unloadPartialSlot("second-drawer-content");

    expect(Object.keys(browserPlans.get(planId)?.types[drawerTypeKey].properties ?? {}))
      .toEqual(["classAdd"]);
  });

  it("recomputes merged property access when a partial fragment unloads", () => {
    const browserPlans = new AppliedBrowserPlans();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const typeKey = "native.component.care-unit";

    browserPlans.register({
      ...rootPlan(planId),
      types: {
        [typeKey]: jsTypeWithPropertyAccess("value", "read"),
      },
      components: {
        "care-unit": component("care-unit", typeKey),
      },
    });

    browserPlans.loadPartialSlot("care-unit-editor", [
      partialPlan(planId, "server-editor", {
        types: {
          [typeKey]: jsTypeWithPropertyAccess("value", "write"),
        },
        components: {
          "care-unit": component("care-unit", typeKey),
        },
      }),
    ], hooks);

    expect(browserPlans.get(planId)?.types[typeKey].properties.value.access).toBe("readwrite");

    browserPlans.unloadPartialSlot("care-unit-editor");

    expect(browserPlans.get(planId)?.types[typeKey].properties.value.access).toBe("read");
  });

  it("allows a partial behavior to reference a root-owned injection host element", () => {
    const browserPlans = new AppliedBrowserPlans();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const hostTypeKey = "native.element.step-container";

    browserPlans.register({
      ...rootPlan(planId),
      types: {
        [hostTypeKey]: jsTypeWithWritableProperty("html"),
      },
      components: {
        "step-container": component("step-container", hostTypeKey),
      },
    });

    browserPlans.loadPartialSlot("step-container", [
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

    expect(Object.keys(browserPlans.get(planId)?.types[hostTypeKey].properties ?? {}))
      .toEqual(["html", "hidden"]);

    browserPlans.unloadPartialSlot("step-container");

    expect(browserPlans.get(planId)?.components["step-container"]).toBeDefined();
    expect(Object.keys(browserPlans.get(planId)?.types[hostTypeKey].properties ?? {}))
      .toEqual(["html"]);
  });

  it("scopes component and type ownership to the runtime plan document", () => {
    const browserPlans = new AppliedBrowserPlans();
    const { hooks } = mergeHooks();
    const rootPlanId = "Drawer.Root";
    const partialPlanId = "DrawerResident.Partial";
    const drawerTypeKey = "native.component.alis-drawer";

    browserPlans.register({
      ...rootPlan(rootPlanId),
      types: {
        [drawerTypeKey]: jsTypeWithWritableProperty("classAdd"),
      },
      components: {
        "alis-drawer": component("alis-drawer", drawerTypeKey),
      },
    });

    browserPlans.loadPartialSlot("alis-drawer-content", [
      partialPlan(partialPlanId, "server-form", {
        types: {
          [drawerTypeKey]: jsTypeWithWritableProperty("classRemove"),
        },
        components: {
          "alis-drawer": component("alis-drawer", drawerTypeKey),
        },
        behaviors: [behavior()],
      }),
    ], hooks);

    const loaded = browserPlans.get(partialPlanId)!;
    expect(loaded.planId).toBe(partialPlanId);
    expect(loaded.components["alis-drawer"]).toBeDefined();
    expect(browserPlans.get(rootPlanId)?.components["alis-drawer"]).toBeDefined();

    browserPlans.unloadPartialSlot("alis-drawer-content");

    expect(browserPlans.get(partialPlanId)).toBeUndefined();
    expect(browserPlans.get(rootPlanId)?.components["alis-drawer"]).toBeDefined();
  });
});
