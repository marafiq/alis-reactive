import { describe, it, expect, afterEach } from "vitest";
import { boot } from "../lifecycle/boot";
import { TestWidget } from "../components/lab/test-widget";

/**
 * Proves that source walk (dot-path resolution from event payload)
 * flows correctly into EVERY mutation kind: set-prop, set-prop+coerce,
 * call (with SourceArg), call+chain.
 *
 * Each test dispatches an event with nested payload, then a custom-event
 * handler walks the path and applies the resolved value via a specific
 * mutation kind. The test asserts the DOM/component state changed.
 */
describe("when walking source into each mutation kind", () => {
  afterEach(() => { document.body.innerHTML = ""; });

  function mountWidget(id: string): { el: HTMLElement; widget: TestWidget } {
    const el = document.createElement("div");
    el.id = id;
    const widget = new TestWidget(el);
    (el as any).ej2_instances = [widget];
    document.body.appendChild(el);
    return { el, widget };
  }

  // ── set-prop: walk source → writable prop ──

  describe("set-prop", () => {
    it("walks evt.form.username into input.value", () => {
      const input = document.createElement("input");
      input.id = "target";
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
            kind: "mutate-element", target: "target",
            mutation: { kind: "set-prop", prop: "value" },
            source: { kind: "event", path: "evt.form.username" },
          }] },
        },
      ] });

      expect(input.value).toBe("walked-user");
    });

    it("walks 3-level deep path into innerHTML", () => {
      document.body.innerHTML = '<div id="deep">—</div>';

      boot({ planId: "Test.Model", components: {}, entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: { kind: "sequential", commands: [{
            kind: "dispatch", event: "deep",
            payload: { a: { b: { c: "<em>deep</em>" } } },
          }] },
        },
        {
          trigger: { kind: "custom-event", event: "deep" },
          reaction: { kind: "sequential", commands: [{
            kind: "mutate-element", target: "deep",
            mutation: { kind: "set-prop", prop: "innerHTML" },
            source: { kind: "event", path: "evt.a.b.c" },
          }] },
        },
      ] });

      expect(document.getElementById("deep")!.innerHTML).toBe("<em>deep</em>");
    });

    it("walks source into fusion widget.value via vendor root", () => {
      const { widget } = mountWidget("fw");

      boot({ planId: "Test.Model", components: {}, entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: { kind: "sequential", commands: [{
            kind: "dispatch", event: "set-fw",
            payload: { result: { newValue: "from-source" } },
          }] },
        },
        {
          trigger: { kind: "custom-event", event: "set-fw" },
          reaction: { kind: "sequential", commands: [{
            kind: "mutate-element", target: "fw",
            mutation: { kind: "set-prop", prop: "value" }, vendor: "fusion",
            source: { kind: "event", path: "evt.result.newValue" },
          }] },
        },
      ] });

      expect(widget.value).toBe("from-source");
    });
  });

  // ── set-prop + coerce: walk source → coerced writable prop ──

  describe("set-prop with coerce", () => {
    it("walks string '42' and coerces to number for input.valueAsNumber pattern", () => {
      const { widget } = mountWidget("num");

      boot({ planId: "Test.Model", components: {}, entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: { kind: "sequential", commands: [{
            kind: "dispatch", event: "set-num",
            payload: { data: { amount: "99" } },
          }] },
        },
        {
          trigger: { kind: "custom-event", event: "set-num" },
          reaction: { kind: "sequential", commands: [{
            kind: "mutate-element", target: "num",
            mutation: { kind: "set-prop", prop: "value", coerce: "number" }, vendor: "fusion",
            source: { kind: "event", path: "evt.data.amount" },
          }] },
        },
      ] });

      // TestWidget.value is a string setter, but coerce converts "99" → 99 (number)
      // The widget stores it — we verify coerce ran by checking the type
      expect(typeof widget.value).toBe("number");
      expect(widget.value).toBe(99);
    });

    it("walks boolean string and coerces to boolean for checked prop", () => {
      const checkbox = document.createElement("input");
      checkbox.type = "checkbox";
      checkbox.id = "cb";
      checkbox.checked = true;
      document.body.appendChild(checkbox);

      boot({ planId: "Test.Model", components: {}, entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: { kind: "sequential", commands: [{
            kind: "dispatch", event: "toggle",
            payload: { state: { checked: "false" } },
          }] },
        },
        {
          trigger: { kind: "custom-event", event: "toggle" },
          reaction: { kind: "sequential", commands: [{
            kind: "mutate-element", target: "cb",
            mutation: { kind: "set-prop", prop: "checked", coerce: "boolean" },
            source: { kind: "event", path: "evt.state.checked" },
          }] },
        },
      ] });

      // "false" coerced to boolean false → checkbox unchecked
      expect(checkbox.checked).toBe(false);
    });
  });

  // ── call: walk source → method arg (SourceArg) ──

  describe("call (with SourceArg)", () => {
    it("walks source into classList.add via chain", () => {
      document.body.innerHTML = '<div id="styled" class="base">text</div>';

      boot({ planId: "Test.Model", components: {}, entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: { kind: "sequential", commands: [{
            kind: "dispatch", event: "style",
            payload: { css: { className: "highlight" } },
          }] },
        },
        {
          trigger: { kind: "custom-event", event: "style" },
          reaction: { kind: "sequential", commands: [{
            kind: "mutate-element", target: "styled",
            mutation: { kind: "call", method: "add", chain: "classList", args: [{ kind: "source", source: { kind: "event", path: "evt.css.className" } }] },
          }] },
        },
      ] });

      expect(document.getElementById("styled")!.classList.contains("highlight")).toBe(true);
      expect(document.getElementById("styled")!.classList.contains("base")).toBe(true);
    });

    it("walks array source into fusion widget.setItems", () => {
      const { widget } = mountWidget("items-target");

      boot({ planId: "Test.Model", components: {}, entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: { kind: "sequential", commands: [{
            kind: "dispatch", event: "load-items",
            payload: { response: { data: { list: ["x", "y", "z"] } } },
          }] },
        },
        {
          trigger: { kind: "custom-event", event: "load-items" },
          reaction: { kind: "sequential", commands: [{
            kind: "mutate-element", target: "items-target",
            mutation: { kind: "call", method: "setItems", args: [{ kind: "source", source: { kind: "event", path: "evt.response.data.list" } }] }, vendor: "fusion",
          }] },
        },
      ] });

      expect(widget.items).toEqual(["x", "y", "z"]);
    });
  });

  // ── call (no args) + call (literal args): no source walk (static) — verify no val leaks ──

  describe("call (no args, no source walk)", () => {
    it("calls focus() without any val argument", () => {
      const { widget } = mountWidget("focus-target");

      boot({ planId: "Test.Model", components: {}, entries: [{
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [{
          kind: "mutate-element", target: "focus-target",
          mutation: { kind: "call", method: "focus" }, vendor: "fusion",
        }] },
      }] });

      expect(widget.focused).toBe(true);
    });
  });

  describe("call (literal args, no source walk)", () => {
    it("calls removeAttribute with single literal arg", () => {
      document.body.innerHTML = '<div id="show-me" hidden>content</div>';

      boot({ planId: "Test.Model", components: {}, entries: [{
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [{
          kind: "mutate-element", target: "show-me",
          mutation: { kind: "call", method: "removeAttribute", args: [{ kind: "literal", value: "hidden" }] },
        }] },
      }] });

      expect(document.getElementById("show-me")!.hasAttribute("hidden")).toBe(false);
    });

    it("calls setAttribute with TWO literal args", () => {
      document.body.innerHTML = '<div id="tag-me">content</div>';

      boot({ planId: "Test.Model", components: {}, entries: [{
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [{
          kind: "mutate-element", target: "tag-me",
          mutation: { kind: "call", method: "setAttribute", args: [{ kind: "literal", value: "data-status" }, { kind: "literal", value: "active" }] },
        }] },
      }] });

      expect(document.getElementById("tag-me")!.getAttribute("data-status")).toBe("active");
    });
  });

  // ── ComponentSource: read one component → write to another ──

  describe("component source read → prop write", () => {
    it("reads native input value and writes to another element textContent", () => {
      const input = document.createElement("input");
      input.id = "src-input";
      input.value = "from-native";
      document.body.appendChild(input);

      document.body.insertAdjacentHTML("beforeend", '<span id="echo">—</span>');

      boot({
        planId: "Test.Model",
        components: {
          srcField: { id: "src-input", vendor: "native", readExpr: "value", componentType: "textbox" },
        },
        entries: [{
          trigger: { kind: "dom-ready" },
          reaction: { kind: "sequential", commands: [{
            kind: "mutate-element", target: "echo",
            mutation: { kind: "set-prop", prop: "textContent" },
            source: { kind: "component", componentId: "src-input", vendor: "native", readExpr: "value" },
          }] },
        }],
      });

      expect(document.getElementById("echo")!.textContent).toBe("from-native");
    });

    it("reads fusion widget value and writes to native input", () => {
      const { widget } = mountWidget("src-widget");
      widget.value = "from-fusion";

      const target = document.createElement("input");
      target.id = "dest-input";
      document.body.appendChild(target);

      boot({
        planId: "Test.Model",
        components: {
          srcWidget: { id: "src-widget", vendor: "fusion", readExpr: "value", componentType: "textbox" },
        },
        entries: [{
          trigger: { kind: "dom-ready" },
          reaction: { kind: "sequential", commands: [{
            kind: "mutate-element", target: "dest-input",
            mutation: { kind: "set-prop", prop: "value" },
            source: { kind: "component", componentId: "src-widget", vendor: "fusion", readExpr: "value" },
          }] },
        }],
      });

      expect(target.value).toBe("from-fusion");
    });

    it("reads fusion widget value and writes to ANOTHER fusion widget", () => {
      const { widget: srcWidget } = mountWidget("cross-src");
      srcWidget.value = "cross-vendor-val";

      const { widget: destWidget } = mountWidget("cross-dest");

      boot({
        planId: "Test.Model",
        components: {
          src: { id: "cross-src", vendor: "fusion", readExpr: "value", componentType: "textbox" },
        },
        entries: [{
          trigger: { kind: "dom-ready" },
          reaction: { kind: "sequential", commands: [{
            kind: "mutate-element", target: "cross-dest",
            mutation: { kind: "set-prop", prop: "value" }, vendor: "fusion",
            source: { kind: "component", componentId: "cross-src", vendor: "fusion", readExpr: "value" },
          }] },
        }],
      });

      expect(destWidget.value).toBe("cross-vendor-val");
    });
  });
});
