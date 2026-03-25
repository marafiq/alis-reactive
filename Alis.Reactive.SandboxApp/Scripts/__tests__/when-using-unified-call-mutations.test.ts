import { describe, it, expect, afterEach } from "vitest";
import { boot } from "../lifecycle/boot";
import { TestWidget } from "../components/lab/test-widget";

describe("when using unified call mutations", () => {
  afterEach(() => { document.body.innerHTML = ""; });

  function mountWidget(id: string): { el: HTMLElement; widget: TestWidget } {
    const el = document.createElement("div");
    el.id = id;
    const widget = new TestWidget(el);
    (el as any).ej2_instances = [widget];
    document.body.appendChild(el);
    return { el, widget };
  }

  it("calls void method with no args", () => {
    const { widget } = mountWidget("tw");

    boot({ planId: "Test.Model", components: {}, entries: [{
      trigger: { kind: "dom-ready" },
      reaction: { kind: "sequential", commands: [{
        kind: "mutate-element", target: "tw",
        mutation: { kind: "call", method: "focus" }, vendor: "fusion",
      }] },
    }] });

    expect(widget.focused).toBe(true);
  });

  it("calls chained method with single literal arg", () => {
    document.body.innerHTML = '<div id="el" class="base">text</div>';

    boot({ planId: "Test.Model", components: {}, entries: [{
      trigger: { kind: "dom-ready" },
      reaction: { kind: "sequential", commands: [{
        kind: "mutate-element", target: "el",
        mutation: { kind: "call", method: "add", chain: "classList",
          args: [{ kind: "literal", value: "highlight" }] },
      }] },
    }] });

    expect(document.getElementById("el")!.classList.contains("highlight")).toBe(true);
  });

  it("calls method with single source arg from event walk", () => {
    const { widget } = mountWidget("tw-items");

    boot({ planId: "Test.Model", components: {}, entries: [
      {
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [{
          kind: "dispatch", event: "load-data",
          payload: { data: { items: ["a", "b"] } },
        }] },
      },
      {
        trigger: { kind: "custom-event", event: "load-data" },
        reaction: { kind: "sequential", commands: [{
          kind: "mutate-element", target: "tw-items",
          mutation: { kind: "call", method: "setItems",
            args: [{ kind: "source", source: { kind: "event", path: "evt.data.items" } }] },
          vendor: "fusion",
        }] },
      },
    ] });

    expect(widget.items).toEqual(["a", "b"]);
  });

  it("calls method with multiple literal args", () => {
    document.body.innerHTML = '<div id="el">text</div>';

    boot({ planId: "Test.Model", components: {}, entries: [{
      trigger: { kind: "dom-ready" },
      reaction: { kind: "sequential", commands: [{
        kind: "mutate-element", target: "el",
        mutation: { kind: "call", method: "setAttribute",
          args: [{ kind: "literal", value: "data-x" }, { kind: "literal", value: "42" }] },
      }] },
    }] });

    expect(document.getElementById("el")!.getAttribute("data-x")).toBe("42");
  });

  it("calls method with mixed literal and source args", () => {
    const { widget } = mountWidget("tw-mixed");

    boot({ planId: "Test.Model", components: {}, entries: [
      {
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [{
          kind: "dispatch", event: "set-prop",
          payload: { form: { label: "hello" } },
        }] },
      },
      {
        trigger: { kind: "custom-event", event: "set-prop" },
        reaction: { kind: "sequential", commands: [{
          kind: "mutate-element", target: "tw-mixed",
          mutation: { kind: "call", method: "setProperty",
            args: [
              { kind: "literal", value: "label" },
              { kind: "source", source: { kind: "event", path: "evt.form.label" } },
            ] },
          vendor: "fusion",
        }] },
      },
    ] });

    expect(widget.getProperty("label")).toBe("hello");
  });

  it("calls method with multiple source args", () => {
    const { widget } = mountWidget("tw-multi");
    widget.setItems(["existing"]);

    boot({ planId: "Test.Model", components: {}, entries: [
      {
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [{
          kind: "dispatch", event: "add-item",
          payload: { data: { item: "new", position: 1 } },
        }] },
      },
      {
        trigger: { kind: "custom-event", event: "add-item" },
        reaction: { kind: "sequential", commands: [{
          kind: "mutate-element", target: "tw-multi",
          mutation: { kind: "call", method: "addItem",
            args: [
              { kind: "source", source: { kind: "event", path: "evt.data.item" } },
              { kind: "source", source: { kind: "event", path: "evt.data.position" }, coerce: "number" },
            ] },
          vendor: "fusion",
        }] },
      },
    ] });

    expect(widget.items).toEqual(["existing", "new"]);
  });

  it("coerces individual source arg to number", () => {
    const { widget } = mountWidget("tw-coerce");
    widget.setItems(["a", "b", "c"]);

    boot({ planId: "Test.Model", components: {}, entries: [
      {
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [{
          kind: "dispatch", event: "insert",
          payload: { config: { idx: "2" } },
        }] },
      },
      {
        trigger: { kind: "custom-event", event: "insert" },
        reaction: { kind: "sequential", commands: [{
          kind: "mutate-element", target: "tw-coerce",
          mutation: { kind: "call", method: "addItem",
            args: [
              { kind: "literal", value: "inserted" },
              { kind: "source", source: { kind: "event", path: "evt.config.idx" }, coerce: "number" },
            ] },
          vendor: "fusion",
        }] },
      },
    ] });

    expect(widget.items).toEqual(["a", "b", "inserted", "c"]);
  });

  it("reads component value and passes as method arg", () => {
    const { widget: widgetA } = mountWidget("a");
    widgetA.value = "from-a";
    const { widget: widgetB } = mountWidget("b");

    boot({
      planId: "Test.Model",
      components: {
        srcA: { id: "a", vendor: "fusion", readExpr: "value", componentType: "textbox", coerceAs: "string" },
      },
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [{
          kind: "mutate-element", target: "b",
          mutation: { kind: "call", method: "setProperty",
            args: [
              { kind: "literal", value: "label" },
              { kind: "source", source: { kind: "component", componentId: "a", vendor: "fusion", readExpr: "value" } },
            ] },
          vendor: "fusion",
        }] },
      }],
    });

    expect(widgetB.getProperty("label")).toBe("from-a");
  });

  it("set-prop still reads value from command level", () => {
    document.body.innerHTML = '<p id="el">old</p>';

    boot({ planId: "Test.Model", components: {}, entries: [{
      trigger: { kind: "dom-ready" },
      reaction: { kind: "sequential", commands: [{
        kind: "mutate-element", target: "el",
        mutation: { kind: "set-prop", prop: "textContent" }, value: "from-command",
      }] },
    }] });

    expect(document.getElementById("el")!.textContent).toBe("from-command");
  });

  it("set-prop still reads source from command level", () => {
    document.body.innerHTML = '<p id="el">old</p>';

    boot({ planId: "Test.Model", components: {}, entries: [
      {
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [{
          kind: "dispatch", event: "msg",
          payload: { msg: "resolved" },
        }] },
      },
      {
        trigger: { kind: "custom-event", event: "msg" },
        reaction: { kind: "sequential", commands: [{
          kind: "mutate-element", target: "el",
          mutation: { kind: "set-prop", prop: "textContent" },
          source: { kind: "event", path: "evt.msg" },
        }] },
      },
    ] });

    expect(document.getElementById("el")!.textContent).toBe("resolved");
  });
});
