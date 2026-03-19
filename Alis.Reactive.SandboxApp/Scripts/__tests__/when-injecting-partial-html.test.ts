import { describe, it, expect, beforeEach } from "vitest";
import { boot, getBootedPlan, resetBootStateForTests } from "../lifecycle/boot";
import { injectHtml } from "../lifecycle/inject";
import type { Plan } from "../types";

beforeEach(() => {
  document.body.innerHTML = "";
  resetBootStateForTests();
});

describe("When injectHtml receives HTML with a plan script", () => {
  it("extracts plan JSON and merges it into the booted plan", () => {
    boot({
      planId: "Test.Model",
      components: { "Name": { id: "name", vendor: "native", readExpr: "value" } },
      entries: [],
    });

    const container = document.createElement("div");
    container.id = "address-container";
    document.body.appendChild(container);

    const partialHtml = `
      <input id="street" name="Address.Street" value="" />
      <script type="application/json" data-alis-plan>${JSON.stringify({
        planId: "Test.Model",
        components: { "Address.Street": { id: "street", vendor: "native", readExpr: "value" } },
        entries: [],
      })}</script>
    `;

    injectHtml(container, partialHtml);

    const plan = getBootedPlan("Test.Model")!;
    expect(plan.components["Name"]).toBeDefined(); // Root preserved
    expect(plan.components["Address.Street"]).toBeDefined(); // Partial merged
    expect(plan.components["Address.Street"].id).toBe("street");
  });

  it("sets sourceId from container ID for dedup on reload", () => {
    boot({ planId: "Test.Model", components: {}, entries: [] });

    const container = document.createElement("div");
    container.id = "my-container";
    document.body.appendChild(container);

    const html = `<script type="application/json" data-alis-plan>${JSON.stringify({
      planId: "Test.Model",
      components: { "Field": { id: "f", vendor: "native", readExpr: "value" } },
      entries: [],
    })}</script>`;

    injectHtml(container, html);

    const plan = getBootedPlan("Test.Model")!;
    expect(plan.components["Field"]).toBeDefined();

    // Reload with different component — old should be removed
    const html2 = `<script type="application/json" data-alis-plan>${JSON.stringify({
      planId: "Test.Model",
      components: { "Field2": { id: "f2", vendor: "native", readExpr: "value" } },
      entries: [],
    })}</script>`;

    injectHtml(container, html2);

    const plan2 = getBootedPlan("Test.Model")!;
    expect(plan2.components["Field"]).toBeUndefined(); // Old removed
    expect(plan2.components["Field2"]).toBeDefined(); // New merged
  });

  it("inserts HTML content into container (plan script removed)", () => {
    const container = document.createElement("div");
    container.id = "slot";
    document.body.appendChild(container);

    const html = `
      <div id="injected-content">Hello</div>
      <script type="application/json" data-alis-plan>${JSON.stringify({
        planId: "X",
        components: {},
        entries: [],
      })}</script>
    `;

    injectHtml(container, html);

    expect(document.getElementById("injected-content")).toBeTruthy();
    expect(document.getElementById("injected-content")!.textContent).toBe("Hello");
    // Plan script should NOT be in the DOM (extracted before insertion)
    expect(container.querySelector("[data-alis-plan]")).toBeNull();
  });

  it("replaces previous container content on reload", () => {
    const container = document.createElement("div");
    container.id = "slot";
    container.innerHTML = "<p>old content</p>";
    document.body.appendChild(container);

    injectHtml(container, `<p id="new-p">new</p>`);

    expect(container.querySelector("#new-p")).toBeTruthy();
    expect(container.textContent).toBe("new");
  });
});

describe("When injectHtml receives HTML without a plan script", () => {
  it("inserts content without merge errors", () => {
    const container = document.createElement("div");
    container.id = "slot";
    document.body.appendChild(container);

    injectHtml(container, `<p id="plain">plain content</p>`);

    expect(document.getElementById("plain")!.textContent).toBe("plain content");
  });
});

describe("When injectHtml receives multiple plan scripts with same planId", () => {
  it("merges all into the same plan", () => {
    boot({ planId: "Test.Model", components: {}, entries: [] });

    const container = document.createElement("div");
    container.id = "multi";
    document.body.appendChild(container);

    const html = `
      <script type="application/json" data-alis-plan>${JSON.stringify({
        planId: "Test.Model",
        components: { "FA": { id: "fa", vendor: "native", readExpr: "value" } },
        entries: [],
      })}</script>
      <script type="application/json" data-alis-plan>${JSON.stringify({
        planId: "Test.Model",
        components: { "FB": { id: "fb", vendor: "native", readExpr: "value" } },
        entries: [],
      })}</script>
    `;

    injectHtml(container, html);

    const plan = getBootedPlan("Test.Model")!;
    // Last merge wins for sourceId-tracked components (same container = same sourceId)
    // Only the second plan's components survive since sourceId dedup removes first
    expect(plan.components["FB"]).toBeDefined();
  });
});
