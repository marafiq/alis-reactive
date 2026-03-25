import { beforeEach, describe, expect, it, vi } from "vitest";

describe("when auto booting plans", () => {
  beforeEach(() => {
    document.body.innerHTML = "";
    vi.resetModules();
  });

  it("merges plans with the same planId before booting", async () => {
    document.body.innerHTML = `
      <div id="status"></div>
      <script type="application/json" data-reactive-plan>
        ${JSON.stringify({
          planId: "Test.Model",
          components: {
            "Root.Name": { id: "root-name", vendor: "native", readExpr: "value", componentType: "textbox", coerceAs: "string" },
          },
          entries: [{
            trigger: { kind: "dom-ready" },
            reaction: {
              kind: "sequential",
              commands: [{
                kind: "mutate-element",
                target: "status",
                mutation: { kind: "set-prop", prop: "textContent" },
                value: "root",
              }],
            },
          }],
        })}
      </script>
      <script type="application/json" data-reactive-plan>
        ${JSON.stringify({
          planId: "Test.Model",
          components: {
            "Nested.City": { id: "nested-city", vendor: "native", readExpr: "value", componentType: "textbox", coerceAs: "string" },
          },
          entries: [{
            trigger: { kind: "custom-event", event: "kick" },
            reaction: {
              kind: "sequential",
              commands: [{
                kind: "mutate-element",
                target: "status",
                mutation: { kind: "set-prop", prop: "textContent" },
                value: "partial",
              }],
            },
          }],
        })}
      </script>
    `;

    const bootModule = await import("../lifecycle/boot");
    await import("../root");

    const plan = bootModule.getBootedPlan("Test.Model");
    expect(plan).toBeDefined();
    expect(Object.keys(plan!.components)).toEqual(expect.arrayContaining(["Root.Name", "Nested.City"]));
    expect(plan!.entries).toHaveLength(2);

    document.dispatchEvent(new CustomEvent("kick"));
    expect(document.getElementById("status")?.textContent).toBe("partial");
  });
});
