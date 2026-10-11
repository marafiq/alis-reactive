import { describe, expect, it } from "vitest";
import { AppliedPlans } from "../../lifecycle/applied-plans";
import {
  behavior,
  component,
  objectContractWithMethodShape,
  objectContractWithPropertyAccess,
  objectContractWithPropertyShape,
  objectContractWithReadableProperty,
  objectContractWithWritableProperty,
  testPlanWiring,
  partialPlan,
  rootPlan,
} from "../support/plan-lifecycle-fixtures";

describe("browser object contract merging", () => {
  it("shares an identical type contract across slots until the last owner unloads", () => {
    const appliedPlans = new AppliedPlans();
    const { wiring } = testPlanWiring();
    const planId = "Resident.Root";
    const sharedTypeKey = "plugin.address";

    appliedPlans.loadPartialSlot("first-slot", [
      partialPlan(planId, {
        types: { [sharedTypeKey]: objectContractWithReadableProperty("token") },
      }),
    ], wiring);
    appliedPlans.loadPartialSlot("second-slot", [
      partialPlan(planId, {
        types: { [sharedTypeKey]: objectContractWithReadableProperty("token") },
      }),
    ], wiring);

    appliedPlans.unloadPartialSlot("first-slot");

    expect(appliedPlans.get(planId)?.types[sharedTypeKey]).toBeDefined();
    expect(appliedPlans.get(planId)?.types[sharedTypeKey]?.properties.token).toBeDefined();

    appliedPlans.unloadPartialSlot("second-slot");

    expect(appliedPlans.get(planId)).toBeUndefined();
  });

  it("removes only the unloaded slot contract from a shared type key", () => {
    const appliedPlans = new AppliedPlans();
    const { wiring } = testPlanWiring();
    const planId = "Resident.Root";
    const sharedTypeKey = "native.component.shared-drawer";

    appliedPlans.loadPartialSlot("first-slot", [
      partialPlan(planId, {
        types: { [sharedTypeKey]: objectContractWithWritableProperty("classRemove") },
      }),
    ], wiring);
    appliedPlans.loadPartialSlot("second-slot", [
      partialPlan(planId, {
        types: { [sharedTypeKey]: objectContractWithWritableProperty("classToggle") },
      }),
    ], wiring);

    expect(Object.keys(appliedPlans.get(planId)?.types[sharedTypeKey]?.properties ?? {}))
      .toEqual(["classRemove", "classToggle"]);

    appliedPlans.unloadPartialSlot("first-slot");

    expect(Object.keys(appliedPlans.get(planId)?.types[sharedTypeKey]?.properties ?? {}))
      .toEqual(["classToggle"]);

    appliedPlans.unloadPartialSlot("second-slot");

    expect(appliedPlans.get(planId)).toBeUndefined();
  });

  it("refines compatible property shapes when object contracts merge", () => {
    const appliedPlans = new AppliedPlans();
    const { wiring } = testPlanWiring();
    const planId = "Resident.Root";
    const sharedTypeKey = "plugin.address";

    appliedPlans.loadPartialSlot("first-slot", [
      partialPlan(planId, {
        types: { [sharedTypeKey]: objectContractWithPropertyShape("token", { kind: "any" }) },
      }),
    ], wiring);
    appliedPlans.loadPartialSlot("second-slot", [
      partialPlan(planId, {
        types: { [sharedTypeKey]: objectContractWithPropertyShape("token", { kind: "string" }) },
      }),
    ], wiring);

    expect(appliedPlans.get(planId)?.types[sharedTypeKey]?.properties.token?.shape)
      .toEqual({ kind: "string" });

    appliedPlans.unloadPartialSlot("second-slot");

    expect(appliedPlans.get(planId)?.types[sharedTypeKey]?.properties.token?.shape)
      .toEqual({ kind: "any" });
  });

  it("merges compatible object property fields from separate slot contracts", () => {
    const appliedPlans = new AppliedPlans();
    const { wiring } = testPlanWiring();
    const planId = "Resident.Root";
    const sharedTypeKey = "plugin.resident";

    appliedPlans.loadPartialSlot("name-slot", [
      partialPlan(planId, {
        types: {
          [sharedTypeKey]: objectContractWithPropertyShape("profile", {
            kind: "object",
            fields: { name: { kind: "string" } },
            additional: false,
          }),
        },
      }),
    ], wiring);
    appliedPlans.loadPartialSlot("age-slot", [
      partialPlan(planId, {
        types: {
          [sharedTypeKey]: objectContractWithPropertyShape("profile", {
            kind: "object",
            fields: { age: { kind: "number" } },
            additional: false,
          }),
        },
      }),
    ], wiring);

    expect(appliedPlans.get(planId)?.types[sharedTypeKey]?.properties.profile?.shape)
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
    const appliedPlans = new AppliedPlans();
    const { wiring } = testPlanWiring();
    const planId = "Resident.Root";
    const sharedTypeKey = "plugin.address";

    appliedPlans.loadPartialSlot("first-slot", [
      partialPlan(planId, {
        types: { [sharedTypeKey]: objectContractWithMethodShape("normalize", { kind: "any" }, { kind: "any" }) },
      }),
    ], wiring);
    appliedPlans.loadPartialSlot("second-slot", [
      partialPlan(planId, {
        types: { [sharedTypeKey]: objectContractWithMethodShape("normalize", { kind: "string" }, { kind: "string" }) },
      }),
    ], wiring);

    const method = appliedPlans.get(planId)?.types[sharedTypeKey]?.methods.normalize;
    expect(method?.arguments).toEqual({ kind: "exact", shapes: [{ kind: "string" }] });
    expect(method?.returns).toEqual({ kind: "string" });
  });

  it("lets a later slot reuse a member differently after the previous contract unloads", () => {
    const appliedPlans = new AppliedPlans();
    const { wiring } = testPlanWiring();
    const planId = "Resident.Root";
    const sharedTypeKey = "native.component.shared-drawer";

    appliedPlans.loadPartialSlot("first-slot", [
      partialPlan(planId, {
        types: { [sharedTypeKey]: objectContractWithReadableProperty("token") },
      }),
    ], wiring);

    appliedPlans.unloadPartialSlot("first-slot");

    appliedPlans.loadPartialSlot("second-slot", [
      partialPlan(planId, {
        types: { [sharedTypeKey]: objectContractWithPropertyShape("token", { kind: "number" }) },
      }),
    ], wiring);

    expect(appliedPlans.get(planId)?.types[sharedTypeKey]?.properties.token?.shape)
      .toEqual({ kind: "number" });
  });

  it("merges compatible contracts for a root-owned app-level component reference", () => {
    const appliedPlans = new AppliedPlans();
    const { wiring } = testPlanWiring();
    const planId = "Resident.Root";
    const drawerTypeKey = "native.component.alis-drawer";

    appliedPlans.register({
      ...rootPlan(planId),
      types: {
        [drawerTypeKey]: objectContractWithWritableProperty("classAdd"),
      },
      components: {
        "alis-drawer": component("alis-drawer", drawerTypeKey),
      },
    });

    appliedPlans.loadPartialSlot("alis-drawer-content", [
      partialPlan(planId, {
        types: {
          [drawerTypeKey]: objectContractWithWritableProperty("classRemove"),
        },
        components: {
          "alis-drawer": component("alis-drawer", drawerTypeKey),
        },
        behaviors: [behavior()],
      }),
    ], wiring);

    const merged = appliedPlans.get(planId)!;
    expect(merged.components["alis-drawer"]).toBeDefined();
    expect(Object.keys(merged.types[drawerTypeKey]!.properties)).toEqual(["classAdd", "classRemove"]);

    appliedPlans.unloadPartialSlot("alis-drawer-content");

    expect(appliedPlans.get(planId)?.components["alis-drawer"]).toBeDefined();
    expect(Object.keys(appliedPlans.get(planId)?.types[drawerTypeKey]?.properties ?? {}))
      .toEqual(["classAdd"]);
  });

  it("removes one partial contract from a root-owned app-level component reference", () => {
    const appliedPlans = new AppliedPlans();
    const { wiring } = testPlanWiring();
    const planId = "Resident.Root";
    const drawerTypeKey = "native.component.alis-drawer";

    appliedPlans.register({
      ...rootPlan(planId),
      types: {
        [drawerTypeKey]: objectContractWithWritableProperty("classAdd"),
      },
      components: {
        "alis-drawer": component("alis-drawer", drawerTypeKey),
      },
    });

    appliedPlans.loadPartialSlot("first-drawer-content", [
      partialPlan(planId, {
        types: {
          [drawerTypeKey]: objectContractWithWritableProperty("classRemove"),
        },
        components: {
          "alis-drawer": component("alis-drawer", drawerTypeKey),
        },
      }),
    ], wiring);
    appliedPlans.loadPartialSlot("second-drawer-content", [
      partialPlan(planId, {
        types: {
          [drawerTypeKey]: objectContractWithWritableProperty("classToggle"),
        },
        components: {
          "alis-drawer": component("alis-drawer", drawerTypeKey),
        },
      }),
    ], wiring);

    expect(Object.keys(appliedPlans.get(planId)?.types[drawerTypeKey]?.properties ?? {}))
      .toEqual(["classAdd", "classRemove", "classToggle"]);

    appliedPlans.unloadPartialSlot("first-drawer-content");

    expect(appliedPlans.get(planId)?.components["alis-drawer"]).toBeDefined();
    expect(Object.keys(appliedPlans.get(planId)?.types[drawerTypeKey]?.properties ?? {}))
      .toEqual(["classAdd", "classToggle"]);

    appliedPlans.unloadPartialSlot("second-drawer-content");

    expect(Object.keys(appliedPlans.get(planId)?.types[drawerTypeKey]?.properties ?? {}))
      .toEqual(["classAdd"]);
  });

  it("recomputes merged property access when a partial contract unloads", () => {
    const appliedPlans = new AppliedPlans();
    const { wiring } = testPlanWiring();
    const planId = "Resident.Root";
    const typeKey = "native.component.care-unit";

    appliedPlans.register({
      ...rootPlan(planId),
      types: {
        [typeKey]: objectContractWithPropertyAccess("value", "read"),
      },
      components: {
        "care-unit": component("care-unit", typeKey),
      },
    });

    appliedPlans.loadPartialSlot("care-unit-editor", [
      partialPlan(planId, {
        types: {
          [typeKey]: objectContractWithPropertyAccess("value", "write"),
        },
        components: {
          "care-unit": component("care-unit", typeKey),
        },
      }),
    ], wiring);

    expect(appliedPlans.get(planId)?.types[typeKey]?.properties.value?.access).toBe("readwrite");

    appliedPlans.unloadPartialSlot("care-unit-editor");

    expect(appliedPlans.get(planId)?.types[typeKey]?.properties.value?.access).toBe("read");
  });

  it("allows a partial behavior to reference a root-owned injection host element", () => {
    const appliedPlans = new AppliedPlans();
    const { wiring } = testPlanWiring();
    const planId = "Resident.Root";
    const hostTypeKey = "native.element.step-container";

    appliedPlans.register({
      ...rootPlan(planId),
      types: {
        [hostTypeKey]: objectContractWithWritableProperty("html"),
      },
      components: {
        "step-container": component("step-container", hostTypeKey),
      },
    });

    appliedPlans.loadPartialSlot("step-container", [
      partialPlan(planId, {
        types: {
          [hostTypeKey]: objectContractWithWritableProperty("hidden"),
        },
        components: {
          "step-container": component("step-container", hostTypeKey),
        },
        behaviors: [behavior()],
      }),
    ], wiring);

    expect(Object.keys(appliedPlans.get(planId)?.types[hostTypeKey]?.properties ?? {}))
      .toEqual(["html", "hidden"]);

    appliedPlans.unloadPartialSlot("step-container");

    expect(appliedPlans.get(planId)?.components["step-container"]).toBeDefined();
    expect(Object.keys(appliedPlans.get(planId)?.types[hostTypeKey]?.properties ?? {}))
      .toEqual(["html"]);
  });

  it("scopes component and type loads to their Active Plan document", () => {
    const appliedPlans = new AppliedPlans();
    const { wiring } = testPlanWiring();
    const rootPlanId = "Drawer.Root";
    const partialPlanId = "DrawerResident.Partial";
    const drawerTypeKey = "native.component.alis-drawer";

    appliedPlans.register({
      ...rootPlan(rootPlanId),
      types: {
        [drawerTypeKey]: objectContractWithWritableProperty("classAdd"),
      },
      components: {
        "alis-drawer": component("alis-drawer", drawerTypeKey),
      },
    });

    appliedPlans.loadPartialSlot("alis-drawer-content", [
      partialPlan(partialPlanId, {
        types: {
          [drawerTypeKey]: objectContractWithWritableProperty("classRemove"),
        },
        components: {
          "alis-drawer": component("alis-drawer", drawerTypeKey),
        },
        behaviors: [behavior()],
      }),
    ], wiring);

    const loaded = appliedPlans.get(partialPlanId)!;
    expect(loaded.planId).toBe(partialPlanId);
    expect(loaded.components["alis-drawer"]).toBeDefined();
    expect(appliedPlans.get(rootPlanId)?.components["alis-drawer"]).toBeDefined();

    appliedPlans.unloadPartialSlot("alis-drawer-content");

    expect(appliedPlans.get(partialPlanId)).toBeUndefined();
    expect(appliedPlans.get(rootPlanId)?.components["alis-drawer"]).toBeDefined();
  });
});
