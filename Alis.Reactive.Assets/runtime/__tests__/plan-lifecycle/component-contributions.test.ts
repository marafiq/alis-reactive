import { describe, expect, it } from "vitest";
import { AppliedBrowserPlans } from "../../lifecycle/merge-plan";
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
    const browserPlans = new AppliedBrowserPlans();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";

    browserPlans.register(rootPlan(planId));

    const firstComponent = component("shared-field", "native.element.shared-a");
    browserPlans.loadPartialSlot("first-slot", [
      partialPlan(planId, "first-slot", {
        types: { "native.element.shared-a": jsType() },
        components: { "shared-field": firstComponent },
      }),
    ], hooks);

    expect(() => browserPlans.loadPartialSlot("second-slot", [
      partialPlan(planId, "second-slot", {
        types: { "native.element.shared-b": jsType() },
        components: { "shared-field": component("shared-field", "native.element.shared-b") },
      }),
    ], hooks)).toThrow('partial plan contribution "second-slot" cannot declare component "shared-field"');
    expect(browserPlans.get(planId)?.components["shared-field"]).toBe(firstComponent);
  });

  it("keeps type fragments out when component preflight rejects the contribution", () => {
    const browserPlans = new AppliedBrowserPlans();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";

    browserPlans.register(rootPlan(planId));
    browserPlans.loadPartialSlot("first-slot", [
      partialPlan(planId, "first-slot", {
        components: { "shared-field": component("shared-field", "native.element.shared-a") },
      }),
    ], hooks);

    expect(() => browserPlans.loadPartialSlot("second-slot", [
      partialPlan(planId, "second-slot", {
        types: { "native.element.second-only": jsType() },
        components: { "shared-field": component("shared-field", "native.element.shared-b") },
      }),
    ], hooks)).toThrow('partial plan contribution "second-slot" cannot declare component "shared-field"');
    expect(browserPlans.get(planId)?.types["native.element.second-only"]).toBeUndefined();
  });

  it("rejects a partial owned definition for a root-owned component key", () => {
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

    expect(() => browserPlans.loadPartialSlot("alis-drawer-content", [
      partialPlan(planId, "server-form", {
        types: {
          [drawerTypeKey]: jsTypeWithWritableProperty("classRemove"),
        },
        components: {
          "alis-drawer": registeredInputComponent("alis-drawer", drawerTypeKey),
        },
      }),
    ], hooks)).toThrow('partial plan contribution "alis-drawer-content" cannot declare component "alis-drawer"');

    expect(Object.keys(browserPlans.get(planId)?.types[drawerTypeKey].properties ?? {}))
      .toEqual(["classAdd"]);
  });
});

describe("layout object contributions", () => {
  it("lets multiple slots share one layout-owned app component", () => {
    const browserPlans = new AppliedBrowserPlans();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const toastTypeKey = "fusion.component.alisFusionToast";

    browserPlans.register(rootPlan(planId));

    browserPlans.loadPartialSlot("first-toast-slot", [
      partialPlan(planId, "first-toast-plan", {
        types: {
          [toastTypeKey]: jsTypeWithWritableProperty("title"),
        },
        components: {
          alisFusionToast: layoutComponent("alisFusionToast", toastTypeKey),
        },
      }),
    ], hooks);
    browserPlans.loadPartialSlot("second-toast-slot", [
      partialPlan(planId, "second-toast-plan", {
        types: {
          [toastTypeKey]: jsTypeWithWritableProperty("content"),
        },
        components: {
          alisFusionToast: layoutComponent("alisFusionToast", toastTypeKey),
        },
      }),
    ], hooks);

    expect(browserPlans.get(planId)?.components.alisFusionToast).toBeDefined();
    expect(Object.keys(browserPlans.get(planId)?.types[toastTypeKey].properties ?? {}))
      .toEqual(["title", "content"]);

    browserPlans.unloadPartialSlot("first-toast-slot");

    expect(browserPlans.get(planId)?.components.alisFusionToast).toBeDefined();
    expect(Object.keys(browserPlans.get(planId)?.types[toastTypeKey].properties ?? {}))
      .toEqual(["content"]);

    browserPlans.unloadPartialSlot("second-toast-slot");

    expect(browserPlans.get(planId)?.components.alisFusionToast).toBeUndefined();
    expect(browserPlans.get(planId)?.types[toastTypeKey]).toBeUndefined();
  });

  it("rejects owned component state for a layout-owned app component key", () => {
    const browserPlans = new AppliedBrowserPlans();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const loaderTypeKey = "native.component.alis-loader";

    browserPlans.register(rootPlan(planId));
    browserPlans.loadPartialSlot("loader-slot", [
      partialPlan(planId, "loader-plan", {
        types: {
          [loaderTypeKey]: jsTypeWithWritableProperty("classAdd"),
        },
        components: {
          "alis-loader": layoutComponent("alis-loader", loaderTypeKey),
        },
      }),
    ], hooks);

    expect(() => browserPlans.loadPartialSlot("owned-loader-slot", [
      partialPlan(planId, "owned-loader-plan", {
        types: {
          [loaderTypeKey]: jsTypeWithWritableProperty("classRemove"),
        },
        components: {
          "alis-loader": registeredInputComponent("alis-loader", loaderTypeKey),
        },
      }),
    ], hooks)).toThrow('partial plan contribution "owned-loader-slot" cannot declare component "alis-loader"');

    browserPlans.unloadPartialSlot("loader-slot");

    expect(browserPlans.get(planId)?.components["alis-loader"]).toBeUndefined();
    expect(browserPlans.get(planId)?.types[loaderTypeKey]).toBeUndefined();
  });
  it("rejects a layout-object reference with a mismatched runtime identity", () => {
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
        "alis-drawer": layoutComponent("alis-drawer", drawerTypeKey),
      },
    });

    expect(() => browserPlans.loadPartialSlot("drawer-slot", [
      partialPlan(planId, "drawer-plan", {
        types: {
          [drawerTypeKey]: jsTypeWithWritableProperty("classRemove"),
        },
        components: {
          "alis-drawer": layoutComponent("other-drawer", drawerTypeKey),
        },
      }),
    ], hooks)).toThrow('partial plan contribution "drawer-slot" cannot declare component "alis-drawer"');

    expect(Object.keys(browserPlans.get(planId)?.types[drawerTypeKey].properties ?? {}))
      .toEqual(["classAdd"]);
  });

});
