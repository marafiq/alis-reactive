import { describe, it, expect, afterEach } from "vitest";
import { boot } from "../boot";
import { TestWidget } from "../test-widget";

describe("when wiring component event with payload", () => {
  afterEach(() => { document.body.innerHTML = ""; });

  describe("native vendor", () => {
    it("passes value as evt property on native change", () => {
      const input = document.createElement("input");
      input.id = "native-evt"; input.value = "hello"; input.type = "text";
      document.body.appendChild(input);
      const result = document.createElement("div");
      result.id = "native-evt-result"; document.body.appendChild(result);

      boot({ planId: "Test.Model", components: {}, entries: [{
        trigger: { kind: "component-event", componentId: "native-evt",
          jsEvent: "change", vendor: "native" },
        reaction: { kind: "sequential", commands: [{
          kind: "mutate-element", target: "native-evt-result",
          prop: "textContent", source: { kind: "event", path: "evt.value" },
        }] },
      }] });

      input.dispatchEvent(new Event("change"));
      expect(result.textContent).toBe("hello");
    });
  });

  describe("fusion vendor", () => {
    it("real TestWidget fires change with {newValue} on inner input", () => {
      const el = document.createElement("div");
      el.id = "fusion-evt";
      const widget = new TestWidget(el);
      (el as any).ej2_instances = [widget];
      document.body.appendChild(el);
      const result = document.createElement("div");
      result.id = "fusion-evt-result"; document.body.appendChild(result);

      boot({ planId: "Test.Model", components: {}, entries: [{
        trigger: { kind: "component-event", componentId: "fusion-evt",
          jsEvent: "change", vendor: "fusion" },
        reaction: { kind: "sequential", commands: [{
          kind: "mutate-element", target: "fusion-evt-result",
          prop: "textContent", source: { kind: "event", path: "evt.newValue" },
        }] },
      }] });

      // Simulate user typing in inner input — TestWidget detects input event,
      // fires "change" with {newValue, previousValue} on its event API.
      const inner = el.querySelector("input") as HTMLInputElement;
      inner.value = "fusion-payload";
      inner.dispatchEvent(new Event("input"));
      expect(result.textContent).toBe("fusion-payload");
    });

    it("real TestWidget fires items-changed with {count} on setItems()", () => {
      const el = document.createElement("div");
      el.id = "fusion-items-evt";
      const widget = new TestWidget(el);
      (el as any).ej2_instances = [widget];
      document.body.appendChild(el);
      const result = document.createElement("div");
      result.id = "fusion-items-evt-result"; document.body.appendChild(result);

      boot({ planId: "Test.Model", components: {}, entries: [{
        trigger: { kind: "component-event", componentId: "fusion-items-evt",
          jsEvent: "items-changed", vendor: "fusion" },
        reaction: { kind: "sequential", commands: [{
          kind: "mutate-element", target: "fusion-items-evt-result",
          prop: "textContent", source: { kind: "event", path: "evt.count" },
        }] },
      }] });

      widget.setItems(["x", "y", "z"]);
      expect(result.textContent).toBe("3");
    });
  });
});
