import { describe, it, expect, afterEach } from "vitest";
import { validate } from "../validation";
import { TestWidget } from "../components/lab/test-widget";

describe("when validating component values", () => {
  afterEach(() => { document.body.innerHTML = ""; });

  function setupForm(id: string): HTMLElement {
    const form = document.createElement("form");
    form.id = id;
    document.body.appendChild(form);
    return form;
  }

  function addErrorSpan(form: HTMLElement, fieldName: string, fieldId?: string): void {
    const span = document.createElement("span");
    if (fieldId) span.id = fieldId + "_error";
    span.setAttribute("data-valmsg-for", fieldName);
    span.setAttribute("hidden", "");
    span.style.display = "none";
    form.appendChild(span);
  }

  function addNativeInput(form: HTMLElement, id: string, value = ""): HTMLInputElement {
    const input = document.createElement("input");
    input.id = id; input.value = value;
    form.appendChild(input);
    addErrorSpan(form, id, id);
    return input;
  }

  function addFusionWidget(form: HTMLElement, id: string, value = ""): TestWidget {
    const el = document.createElement("div");
    el.id = id;
    const widget = new TestWidget(el);
    widget.value = value;
    (el as any).ej2_instances = [widget];
    form.appendChild(el);
    addErrorSpan(form, id, id);
    return widget;
  }

  function errorSpan(formId: string, fieldName: string): HTMLElement | null {
    return document.getElementById(fieldName + "_error");
  }

  describe("native vendor", () => {
    it("required rule: fails for empty input", () => {
      const form = setupForm("f1");
      addNativeInput(form, "name", "");

      expect(validate({
        formId: "f1",
        fields: [{ fieldId: "name", fieldName: "name", vendor: "native",
          readExpr: "value", rules: [{ rule: "required", message: "Required" }] }],
      })).toBe(false);
      expect(errorSpan("f1", "name")?.textContent).toBe("Required");
    });

    it("required rule: passes for filled input", () => {
      const form = setupForm("f1b");
      addNativeInput(form, "name-ok", "hello");

      expect(validate({
        formId: "f1b",
        fields: [{ fieldId: "name-ok", fieldName: "name-ok", vendor: "native",
          readExpr: "value", rules: [{ rule: "required", message: "Required" }] }],
      })).toBe(true);
    });
  });

  describe("fusion vendor", () => {
    it("required rule: fails for empty widget value", () => {
      const form = setupForm("f4");
      addFusionWidget(form, "amount", "");

      expect(validate({
        formId: "f4",
        fields: [{ fieldId: "amount", fieldName: "amount", vendor: "fusion",
          readExpr: "value", rules: [{ rule: "required", message: "Required" }] }],
      })).toBe(false);
      expect(errorSpan("f4", "amount")?.textContent).toBe("Required");
    });

    it("required rule: passes for filled widget value", () => {
      const form = setupForm("f5");
      addFusionWidget(form, "amount", "42");

      expect(validate({
        formId: "f5",
        fields: [{ fieldId: "amount", fieldName: "amount", vendor: "fusion",
          readExpr: "value", rules: [{ rule: "required", message: "Required" }] }],
      })).toBe(true);
    });

    it("max rule: reads numeric value from real TestWidget", () => {
      const form = setupForm("f6");
      addFusionWidget(form, "score", "150");

      expect(validate({
        formId: "f6",
        fields: [{ fieldId: "score", fieldName: "score", vendor: "fusion",
          readExpr: "value", rules: [{ rule: "max", constraint: 100, message: "Max 100" }] }],
      })).toBe(false);
      expect(errorSpan("f6", "score")?.textContent).toBe("Max 100");
    });
  });

  describe("cross-vendor", () => {
    it("equalTo rule: passes when native and fusion values match", () => {
      const form = setupForm("f7");
      addNativeInput(form, "password", "secret");
      addFusionWidget(form, "confirm", "secret");

      expect(validate({
        formId: "f7",
        fields: [
          { fieldId: "password", fieldName: "password", vendor: "native",
            readExpr: "value", rules: [{ rule: "required", message: "Required" }] },
          { fieldId: "confirm", fieldName: "confirm", vendor: "fusion",
            readExpr: "value", rules: [{ rule: "equalTo", constraint: "password", message: "Must match" }] },
        ],
      })).toBe(true);
    });

    it("equalTo rule: fails when native and fusion values differ", () => {
      const form = setupForm("f8");
      addNativeInput(form, "password", "secret");
      addFusionWidget(form, "confirm", "wrong");

      expect(validate({
        formId: "f8",
        fields: [
          { fieldId: "password", fieldName: "password", vendor: "native",
            readExpr: "value", rules: [{ rule: "required", message: "Required" }] },
          { fieldId: "confirm", fieldName: "confirm", vendor: "fusion",
            readExpr: "value", rules: [{ rule: "equalTo", constraint: "password", message: "Must match" }] },
        ],
      })).toBe(false);
      expect(errorSpan("f8", "confirm")?.textContent).toBe("Must match");
    });
  });
});
