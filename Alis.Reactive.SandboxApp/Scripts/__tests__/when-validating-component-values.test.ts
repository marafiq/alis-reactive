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

  function errorSpan(_formId: string, fieldName: string): HTMLElement | null {
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
          readExpr: "value", rules: [{ rule: "max", constraint: 100, message: "Max 100", coerceAs: "number" }] }],
      })).toBe(false);
      expect(errorSpan("f6", "score")?.textContent).toBe("Max 100");
    });
  });

  describe("fusion numeric (real SF behavior: value is number|null, not string)", () => {
    // Syncfusion NumericTextBox .value returns null when cleared, 0 when model default.
    // TestWidget stores string — these tests bypass it to test real numeric behavior.

    function addFusionNumeric(form: HTMLElement, id: string, value: number | null): void {
      const el = document.createElement("div");
      el.id = id;
      // Mimic real SF: ej2_instances[0] is a plain object with numeric .value
      (el as any).ej2_instances = [{ value }];
      form.appendChild(el);
      addErrorSpan(form, id, id);
    }

    it("required rule: fails when value is null (empty numeric field)", () => {
      const form = setupForm("fn1");
      addFusionNumeric(form, "amount", null);

      expect(validate({
        formId: "fn1",
        fields: [{ fieldId: "amount", fieldName: "amount", vendor: "fusion",
          readExpr: "value", rules: [{ rule: "required", message: "Amount is required" }] }],
      })).toBe(false);
      expect(errorSpan("fn1", "amount")?.textContent).toBe("Amount is required");
    });

    it("required rule: passes when value is 0 (user explicitly typed zero)", () => {
      const form = setupForm("fn2");
      addFusionNumeric(form, "amount", 0);

      expect(validate({
        formId: "fn2",
        fields: [{ fieldId: "amount", fieldName: "amount", vendor: "fusion",
          readExpr: "value", rules: [{ rule: "required", message: "Amount is required" }] }],
      })).toBe(true);
    });

    it("required rule: passes when value is 42 (normal value)", () => {
      const form = setupForm("fn3");
      addFusionNumeric(form, "amount", 42);

      expect(validate({
        formId: "fn3",
        fields: [{ fieldId: "amount", fieldName: "amount", vendor: "fusion",
          readExpr: "value", rules: [{ rule: "required", message: "Amount is required" }] }],
      })).toBe(true);
    });

    it("min rule: fails when value is below minimum", () => {
      const form = setupForm("fn4");
      addFusionNumeric(form, "amount", 5);

      expect(validate({
        formId: "fn4",
        fields: [{ fieldId: "amount", fieldName: "amount", vendor: "fusion",
          readExpr: "value", rules: [{ rule: "min", constraint: 10, message: "Min 10", coerceAs: "number" }] }],
      })).toBe(false);
      expect(errorSpan("fn4", "amount")?.textContent).toBe("Min 10");
    });

    it("min rule: skips when value is null (not required, min only)", () => {
      const form = setupForm("fn5");
      addFusionNumeric(form, "amount", null);

      expect(validate({
        formId: "fn5",
        fields: [{ fieldId: "amount", fieldName: "amount", vendor: "fusion",
          readExpr: "value", rules: [{ rule: "min", constraint: 10, message: "Min 10", coerceAs: "number" }] }],
      })).toBe(true); // min skips empty values — only required catches empty
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
            readExpr: "value", rules: [{ rule: "equalTo", field: "password", message: "Must match" }] },
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
            readExpr: "value", rules: [{ rule: "equalTo", field: "password", message: "Must match" }] },
        ],
      })).toBe(false);
      expect(errorSpan("f8", "confirm")?.textContent).toBe("Must match");
    });
  });

  // -- Condition reader with uncoercible value (toString Err) -------------------
  // When a component's readExpr walks to a plain object (plan misconfiguration),
  // toString returns Err. The condition reader returns undefined → condition
  // evaluates to null (unresolvable) → rule blocks via fail-closed behavior.
  // See: https://github.com/marafiq/alis-reactive/issues/49
  describe("condition reader with uncoercible component value", () => {
    it("conditional rule blocks when condition source value is a plain object", () => {
      const form = setupForm("f-obj");

      // Condition source: Fusion widget whose value is a plain object
      // (simulates wrong readExpr walking to an object instead of a scalar)
      const el = document.createElement("div");
      el.id = "ObjField";
      const widget = new TestWidget(el);
      widget.value = { nested: "data" } as any; // plain object — toString returns Err
      (el as any).ej2_instances = [widget];
      form.appendChild(el);
      addErrorSpan(form, "ObjField", "ObjField");

      // Target field: native input with conditional required
      addNativeInput(form, "TargetField", "some value");

      const result = validate({
        formId: "f-obj",
        fields: [
          { fieldId: "ObjField", fieldName: "ObjField",
            vendor: "fusion", readExpr: "value", rules: [] },
          { fieldId: "TargetField", fieldName: "TargetField",
            vendor: "native", readExpr: "value",
            rules: [{
              rule: "required", message: "Required when object field set",
              when: { field: "ObjField", op: "truthy" }
            }]
          },
        ],
      });

      // toString(plainObject) → Err → condition reader returns undefined
      // → evalCondition returns null (unresolvable) → checkRuleCondition returns "block"
      // → rule blocks → validation FAILS (fail-closed)
      expect(result).toBe(false);
    });
  });

  // -- Condition reader with Date component and coerceAs "date" -------------------
  // When coerceAs === "date", condition reader converts Date via toDate() → Unix ms string.
  // C# WhenField<DateTime> serializes condition value as Unix ms (long).
  // Both sides produce the same numeric string → eq/neq comparison matches.
  describe("condition reader with date component (Unix ms comparison)", () => {
    it("condition eq matches when component Date Unix ms equals plan Unix ms", () => {
      const form = setupForm("f-date-cond");

      // Fusion DatePicker with Date value
      const el = document.createElement("div");
      el.id = "AdmDate";
      const widget = new TestWidget(el);
      // Simulate DatePicker value: 2026-07-01T00:00:00Z
      const dateValue = new Date("2026-07-01T00:00:00Z");
      widget.value = dateValue as any;
      (el as any).ej2_instances = [widget];
      form.appendChild(el);
      addErrorSpan(form, "AdmDate", "AdmDate");

      // Target field
      addNativeInput(form, "PatientName", "");

      // Unix ms for 2026-07-01T00:00:00Z (same as C# DateTimeOffset.ToUnixTimeMilliseconds)
      const unixMs = dateValue.getTime();

      const result = validate({
        formId: "f-date-cond",
        fields: [
          { fieldId: "AdmDate", fieldName: "AdmDate",
            vendor: "fusion", readExpr: "value", coerceAs: "date", rules: [] },
          { fieldId: "PatientName", fieldName: "PatientName",
            vendor: "native", readExpr: "value",
            rules: [{
              rule: "required", message: "Name required for this date",
              when: { field: "AdmDate", op: "eq", value: unixMs }
            }]
          },
        ],
      });

      // Component Date → toDate() → getTime() → Unix ms string
      // Plan value: unixMs (number) → toString() → same Unix ms string
      // eq matches → required fires → PatientName empty → FAILS
      expect(result).toBe(false);
    });

    it("condition truthy works for Date without Unix ms comparison", () => {
      const form = setupForm("f-date-truthy");

      const el = document.createElement("div");
      el.id = "AdmDate2";
      const widget = new TestWidget(el);
      widget.value = new Date("2026-07-01T00:00:00Z") as any;
      (el as any).ej2_instances = [widget];
      form.appendChild(el);
      addErrorSpan(form, "AdmDate2", "AdmDate2");

      addNativeInput(form, "PatientName2", "");

      const result = validate({
        formId: "f-date-truthy",
        fields: [
          { fieldId: "AdmDate2", fieldName: "AdmDate2",
            vendor: "fusion", readExpr: "value", coerceAs: "date", rules: [] },
          { fieldId: "PatientName2", fieldName: "PatientName2",
            vendor: "native", readExpr: "value",
            rules: [{
              rule: "required", message: "Name required when date set",
              when: { field: "AdmDate2", op: "truthy" }
            }]
          },
        ],
      });

      // Date → toDate() → Unix ms → String(ms) → non-empty → truthy = true
      // → required fires → PatientName2 empty → FAILS
      expect(result).toBe(false);
    });

    it("condition reader returns empty for null Date with coerceAs date", () => {
      const form = setupForm("f-date-null");

      const el = document.createElement("div");
      el.id = "AdmDate3";
      const widget = new TestWidget(el);
      widget.value = null as any; // no date selected
      (el as any).ej2_instances = [widget];
      form.appendChild(el);
      addErrorSpan(form, "AdmDate3", "AdmDate3");

      addNativeInput(form, "PatientName3", "");

      const result = validate({
        formId: "f-date-null",
        fields: [
          { fieldId: "AdmDate3", fieldName: "AdmDate3",
            vendor: "fusion", readExpr: "value", coerceAs: "date", rules: [] },
          { fieldId: "PatientName3", fieldName: "PatientName3",
            vendor: "native", readExpr: "value",
            rules: [{
              rule: "required", message: "Name required when date set",
              when: { field: "AdmDate3", op: "truthy" }
            }]
          },
        ],
      });

      // null Date → normalized to "" at line 251 → truthy = false
      // → condition NOT met → required skipped → PASSES
      expect(result).toBe(true);
    });
  });
});
