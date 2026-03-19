import { describe, it, expect, afterEach } from "vitest";
import { evalRead } from "../resolution/component";
import { TestWidget } from "../components/lab/test-widget";

describe("when reading component value", () => {
  afterEach(() => { document.body.innerHTML = ""; });

  describe("native vendor", () => {
    it("reads el.value via readExpr 'value'", () => {
      const input = document.createElement("input");
      input.id = "native-read"; input.value = "hello";
      document.body.appendChild(input);
      expect(evalRead("native-read", "native", "value")).toBe("hello");
    });

    it("reads el.checked via readExpr 'checked'", () => {
      const cb = document.createElement("input");
      cb.id = "native-cb"; cb.type = "checkbox"; cb.checked = true;
      document.body.appendChild(cb);
      expect(evalRead("native-cb", "native", "checked")).toBe(true);
    });

    it("throws for missing element", () => {
      expect(() => evalRead("missing", "native", "value")).toThrow("element not found: missing");
    });
  });

  describe("fusion vendor", () => {
    it("reads real TestWidget.value via readExpr 'value'", () => {
      const el = document.createElement("div");
      el.id = "fusion-read";
      const widget = new TestWidget(el);
      widget.value = "hello";
      (el as any).ej2_instances = [widget];
      document.body.appendChild(el);
      expect(evalRead("fusion-read", "fusion", "value")).toBe("hello");
    });

    it("reads real TestWidget.items length via readExpr 'items.length'", () => {
      const el = document.createElement("div");
      el.id = "fusion-items";
      const widget = new TestWidget(el);
      widget.setItems(["a", "b", "c"]);
      (el as any).ej2_instances = [widget];
      document.body.appendChild(el);
      expect(evalRead("fusion-items", "fusion", "items.length")).toBe(3);
    });

    it("throws when ej2_instances missing", () => {
      const el = document.createElement("div");
      el.id = "no-ej2";
      document.body.appendChild(el);
      expect(() => evalRead("no-ej2", "fusion", "value")).toThrow("no vendor root");
    });
  });

  it("same readExpr 'value' works for both vendors", () => {
    const native = document.createElement("input");
    native.id = "both-n"; native.value = "same";
    document.body.appendChild(native);

    const fusion = document.createElement("div");
    fusion.id = "both-f";
    const widget = new TestWidget(fusion);
    widget.value = "same";
    (fusion as any).ej2_instances = [widget];
    document.body.appendChild(fusion);

    expect(evalRead("both-n", "native", "value")).toBe("same");
    expect(evalRead("both-f", "fusion", "value")).toBe("same");
  });
});
