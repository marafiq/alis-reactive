import { describe, it, expect, afterEach } from "vitest";
import { boot } from "../boot";
import { TestWidget } from "../test-widget";

describe("when writing component value", () => {
  afterEach(() => { document.body.innerHTML = ""; });

  describe("native vendor", () => {
    it("sets el.value via structured prop", () => {
      const input = document.createElement("input");
      input.id = "native-write";
      document.body.appendChild(input);

      boot({ planId: "Test.Model", components: {}, entries: [{
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [{
          kind: "mutate-element", target: "native-write",
          prop: "value", value: "written",
        }] },
      }] });

      expect((document.getElementById("native-write") as HTMLInputElement).value).toBe("written");
    });
  });

  describe("fusion vendor", () => {
    it("sets real TestWidget.value via structured prop", () => {
      const el = document.createElement("div");
      el.id = "fusion-write";
      const widget = new TestWidget(el);
      (el as any).ej2_instances = [widget];
      document.body.appendChild(el);

      boot({ planId: "Test.Model", components: {}, entries: [{
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [{
          kind: "mutate-element", target: "fusion-write",
          prop: "value", vendor: "fusion", value: "written",
        }] },
      }] });

      expect(widget.value).toBe("written");
    });
  });
});
