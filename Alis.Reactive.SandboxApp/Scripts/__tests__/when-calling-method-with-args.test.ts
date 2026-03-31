import { describe, it, expect, afterEach } from "vitest";
import { boot } from "../lifecycle/boot";
import { TestWidget } from "../components/lab/test-widget";

import { dispatchPayload, sourceValue } from "./test-value-helpers";
describe("when calling component method with args (source walk → val)", () => {
  afterEach(() => { document.body.innerHTML = ""; });

  function mountWidget(id: string): { el: HTMLElement; widget: TestWidget } {
    const el = document.createElement("div");
    el.id = id;
    const widget = new TestWidget(el);
    (el as any).ej2_instances = [widget];
    document.body.appendChild(el);
    return { el, widget };
  }

  describe("fusion vendor", () => {
    it("walks source path and passes ARRAY val to real TestWidget.setItems()", () => {
      const { widget } = mountWidget("fusion-args");

      boot({ planId: "Test.Model", components: {}, entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: { kind: "sequential", commands: [{
            kind: "dispatch", event: "load-data",
            payload: dispatchPayload({ data: { items: ["a", "b", "c"] } }),
          }] },
        },
        {
          trigger: { kind: "custom-event", event: "load-data" },
          reaction: { kind: "sequential", commands: [{
            kind: "mutate-element", target: "fusion-args",
            mutation: { kind: "call", method: "setItems", args: [sourceValue({ kind: "event", path: "evt.data.items" })] }, vendor: "fusion",
          }] },
        },
      ] });

      expect(widget.items).toEqual(["a", "b", "c"]);
    });

    it("walks deep nested source and passes SCALAR val to property setter", () => {
      const { widget } = mountWidget("fusion-set");

      boot({ planId: "Test.Model", components: {}, entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: { kind: "sequential", commands: [{
            kind: "dispatch", event: "set-val",
            payload: dispatchPayload({ result: { detail: { newValue: "walked-value" } } }),
          }] },
        },
        {
          trigger: { kind: "custom-event", event: "set-val" },
          reaction: { kind: "sequential", commands: [{
            kind: "mutate-element", target: "fusion-set",
            mutation: { kind: "set-prop", prop: "value", value: sourceValue({ kind: "event", path: "evt.result.detail.newValue" }) }, vendor: "fusion",
          }] },
        },
      ] });

      expect(widget.value).toBe("walked-value");
    });
  });

  describe("native vendor", () => {
    it("walks source path and passes val to native element property", () => {
      const input = document.createElement("input");
      input.id = "native-args";
      document.body.appendChild(input);

      boot({ planId: "Test.Model", components: {}, entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: { kind: "sequential", commands: [{
            kind: "dispatch", event: "set-attr",
            payload: dispatchPayload({ attr: { val: "active" } }),
          }] },
        },
        {
          trigger: { kind: "custom-event", event: "set-attr" },
          reaction: { kind: "sequential", commands: [{
            kind: "mutate-element", target: "native-args",
            mutation: { kind: "set-prop", prop: "value", value: { kind: "source", source: { kind: "event", path: "evt.attr.val" } } },
          }] },
        },
      ] });

      expect(input.value).toBe("active");
    });

    it("walks source path and passes val to native element value setter", () => {
      const input = document.createElement("input");
      input.id = "native-set";
      document.body.appendChild(input);

      boot({ planId: "Test.Model", components: {}, entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: { kind: "sequential", commands: [{
            kind: "dispatch", event: "fill",
            payload: dispatchPayload({ form: { username: "walked-user" } }),
          }] },
        },
        {
          trigger: { kind: "custom-event", event: "fill" },
          reaction: { kind: "sequential", commands: [{
            kind: "mutate-element", target: "native-set",
            mutation: { kind: "set-prop", prop: "value", value: { kind: "source", source: { kind: "event", path: "evt.form.username" } } },
          }] },
        },
      ] });

      expect(input.value).toBe("walked-user");
    });
  });
});
