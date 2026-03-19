import { describe, it, expect, afterEach } from "vitest";
import { evalRead, resolveRoot } from "../resolution/component";
import { wireTrigger } from "../execution/trigger";
import { mutateElement } from "../execution/element";
import { resolveGather } from "../http/gather";
import { TestWidget } from "../components/lab/test-widget";

describe("when failing fast on missing targets", () => {
  afterEach(() => { document.body.innerHTML = ""; });

  // -- component.ts --

  describe("evalRead", () => {
    it("throws when element not found", () => {
      expect(() => evalRead("missing", "native", "value"))
        .toThrow("element not found: missing");
    });

    it("throws when vendor root missing", () => {
      const el = document.createElement("div");
      el.id = "no-ej2";
      document.body.appendChild(el);
      expect(() => evalRead("no-ej2", "fusion", "value"))
        .toThrow("no vendor root");
    });
  });

  describe("resolveRoot", () => {
    it("throws on unknown vendor", () => {
      const el = document.createElement("div");
      expect(() => resolveRoot(el, "unknown" as any))
        .toThrow("unknown vendor");
    });
  });

  // -- trigger.ts --

  describe("wireTrigger", () => {
    it("throws when component element not found", () => {
      expect(() => wireTrigger(
        { kind: "component-event", componentId: "missing-comp", jsEvent: "change", vendor: "native" },
        { kind: "sequential", commands: [] },
      )).toThrow("element not found: missing-comp");
    });

    it("throws when vendor root missing for component event", () => {
      const el = document.createElement("div");
      el.id = "no-root-comp";
      document.body.appendChild(el);

      expect(() => wireTrigger(
        { kind: "component-event", componentId: "no-root-comp", jsEvent: "change", vendor: "fusion", readExpr: "value" },
        { kind: "sequential", commands: [] },
      )).toThrow("no vendor root");
    });
  });

  // -- element.ts --

  describe("mutateElement", () => {
    it("throws when target not found", () => {
      expect(() => mutateElement({
        kind: "mutate-element",
        target: "missing-el",
        mutation: { kind: "set-prop", prop: "textContent" },
        value: "hello",
      })).toThrow("target not found: missing-el");
    });
  });

  // -- gather.ts --

  describe("resolveGather", () => {
    it("throws when explicit component not found", () => {
      expect(() => resolveGather(
        [{ kind: "component", componentId: "missing-gather", vendor: "native", name: "Field", readExpr: "value" }],
        "POST",
        {},
      )).toThrow("missing-gather");
    });
  });
});
