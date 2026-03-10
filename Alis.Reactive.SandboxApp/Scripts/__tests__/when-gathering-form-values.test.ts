import { describe, it, expect, beforeEach } from "vitest";
import { JSDOM } from "jsdom";
import type { GatherItem } from "../types";

let resolveGather: typeof import("../gather").resolveGather;

beforeEach(async () => {
  const dom = new JSDOM(`<!DOCTYPE html><html><body>
    <form id="myForm">
      <input id="FirstName" name="FirstName" value="John" />
      <input id="LastName" name="LastName" value="Doe" />
      <select id="FacilityId" name="FacilityId"><option value="42" selected>Main</option></select>
    </form>
  </body></html>`);

  (globalThis as any).document = dom.window.document;
  (globalThis as any).FormData = dom.window.FormData;

  const mod = await import("../gather");
  resolveGather = mod.resolveGather;
});

describe("gather", () => {
  it("native input value appears in POST body", () => {
    const items: GatherItem[] = [
      { kind: "component", componentId: "FirstName", vendor: "native", name: "FirstName" },
    ];
    const result = resolveGather(items, "POST");
    expect(result.body).toEqual({ FirstName: "John" });
    expect(result.urlParams).toEqual([]);
  });

  it("fusion component value appears in POST body", () => {
    // Mock ej2_instances on the element
    const el = document.getElementById("FacilityId")!;
    (el as any).ej2_instances = [{ value: 42 }];

    const items: GatherItem[] = [
      { kind: "component", componentId: "FacilityId", vendor: "fusion", name: "FacilityId", readExpr: "comp.value" },
    ];
    const result = resolveGather(items, "POST");
    expect(result.body).toEqual({ FacilityId: 42 });
  });

  it("GET request carries values as URL params", () => {
    const items: GatherItem[] = [
      { kind: "component", componentId: "FirstName", vendor: "native", name: "FirstName" },
    ];
    const result = resolveGather(items, "GET");
    expect(result.urlParams).toEqual(["FirstName=John"]);
    expect(result.body).toEqual({});
  });

  it("static values appear in POST body", () => {
    const items: GatherItem[] = [
      { kind: "static", param: "action", value: "save" },
      { kind: "static", param: "version", value: 2 },
    ];
    const result = resolveGather(items, "POST");
    expect(result.body).toEqual({ action: "save", version: 2 });
  });

  it("all form fields flow into POST body", () => {
    const items: GatherItem[] = [
      { kind: "all", formId: "myForm" },
    ];
    const result = resolveGather(items, "POST");
    expect(result.body).toEqual({
      FirstName: "John",
      LastName: "Doe",
      FacilityId: "42",
    });
  });

  it("mixed gather items combine correctly", () => {
    const items: GatherItem[] = [
      { kind: "component", componentId: "FirstName", vendor: "native", name: "FirstName" },
      { kind: "static", param: "csrfToken", value: "abc123" },
    ];
    const result = resolveGather(items, "POST");
    expect(result.body).toEqual({ FirstName: "John", csrfToken: "abc123" });
  });

  it("empty gather returns empty result", () => {
    const result = resolveGather([], "POST");
    expect(result.body).toEqual({});
    expect(result.urlParams).toEqual([]);
  });
});
