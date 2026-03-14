import { describe, it, expect, afterEach } from "vitest";
import { boot } from "../boot";
import { TestWidget } from "../test-widget";

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
            payload: { data: { items: ["a", "b", "c"] } },
          }] },
        },
        {
          trigger: { kind: "custom-event", event: "load-data" },
          reaction: { kind: "sequential", commands: [{
            kind: "mutate-element", target: "fusion-args",
            method: "setItems", vendor: "fusion",
            source: { kind: "event", path: "evt.data.items" },
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
            payload: { result: { detail: { newValue: "walked-value" } } },
          }] },
        },
        {
          trigger: { kind: "custom-event", event: "set-val" },
          reaction: { kind: "sequential", commands: [{
            kind: "mutate-element", target: "fusion-set",
            prop: "value", vendor: "fusion",
            source: { kind: "event", path: "evt.result.detail.newValue" },
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
            payload: { attr: { val: "active" } },
          }] },
        },
        {
          trigger: { kind: "custom-event", event: "set-attr" },
          reaction: { kind: "sequential", commands: [{
            kind: "mutate-element", target: "native-args",
            prop: "value",
            source: { kind: "event", path: "evt.attr.val" },
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
            payload: { form: { username: "walked-user" } },
          }] },
        },
        {
          trigger: { kind: "custom-event", event: "fill" },
          reaction: { kind: "sequential", commands: [{
            kind: "mutate-element", target: "native-set",
            prop: "value",
            source: { kind: "event", path: "evt.form.username" },
          }] },
        },
      ] });

      expect(input.value).toBe("walked-user");
    });
  });
});
