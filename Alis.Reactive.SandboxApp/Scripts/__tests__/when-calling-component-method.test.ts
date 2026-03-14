import { describe, it, expect, afterEach } from "vitest";
import { boot } from "../boot";
import { TestWidget } from "../test-widget";

describe("when calling component method (void)", () => {
  afterEach(() => { document.body.innerHTML = ""; });

  describe("native vendor", () => {
    it("calls el.focus() via structured method", () => {
      const input = document.createElement("input");
      input.id = "native-method";
      let focused = false;
      input.focus = () => { focused = true; };
      document.body.appendChild(input);

      boot({ planId: "Test.Model", components: {}, entries: [{
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [{
          kind: "mutate-element", target: "native-method",
          mutation: { kind: "call-void", method: "focus" },
        }] },
      }] });

      expect(focused).toBe(true);
    });
  });

  describe("fusion vendor", () => {
    it("calls real TestWidget.focus() via structured method", () => {
      const el = document.createElement("div");
      el.id = "fusion-method";
      const widget = new TestWidget(el);
      (el as any).ej2_instances = [widget];
      document.body.appendChild(el);

      boot({ planId: "Test.Model", components: {}, entries: [{
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [{
          kind: "mutate-element", target: "fusion-method",
          mutation: { kind: "call-void", method: "focus" }, vendor: "fusion",
        }] },
      }] });

      expect(widget.focused).toBe(true);
    });
  });
});
