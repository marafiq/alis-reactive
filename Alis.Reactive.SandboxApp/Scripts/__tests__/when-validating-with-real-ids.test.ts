import { describe, it, expect, beforeEach } from "vitest";
import { JSDOM } from "jsdom";
import type { ValidationDescriptor, ValidationField } from "../types";

/**
 * Tests validation with REAL IdGenerator-style IDs.
 * In the actual app:
 *   fieldId  = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ComponentGatherModel__DietaryRestrictions"
 *   fieldName = "DietaryRestrictions" (from FluentValidation, matches data-valmsg-for)
 *   error span id = "{fieldId}_error"
 *
 * The mismatch between fieldId (long namespace) and fieldName (short binding path)
 * is the real-world scenario. Tests must verify error display works with both.
 */

let validate: typeof import("../validation").validate;
let resetLiveClearForTests: typeof import("../validation").resetLiveClearForTests;

// Simulates what IdGenerator produces
const SCOPE = "App_Models_GatherModel";
const FIELD_ID = `${SCOPE}__DietaryRestrictions`;
const FIELD_NAME = "DietaryRestrictions"; // FluentValidation property name = data-valmsg-for value

function errorSpanById(): HTMLElement | null {
  return document.getElementById(FIELD_ID + "_error");
}

function errorSpanByAttr(): HTMLElement | null {
  return document.querySelector(`span[data-valmsg-for="${FIELD_NAME}"]`);
}

describe("when validating with real namespace-qualified IDs", () => {

  describe("FusionMultiSelect with IdGenerator IDs", () => {
    let ej2Instance: any;

    beforeEach(async () => {
      ej2Instance = { value: null, addEventListener: () => {} };

      // This is what Html.InputField + MultiSelect actually renders:
      // - wrapper <span> has id from IdGenerator (FIELD_ID)
      // - error <span> has id="{FIELD_ID}_error" and data-valmsg-for="{FIELD_NAME}"
      const dom = new JSDOM(`<!DOCTYPE html><html><body>
        <form id="gather-form">
          <div>
            <label for="${FIELD_ID}">Dietary Restrictions</label>
            <span class="e-control-wrapper e-multi-select-wrapper" id="${FIELD_ID}">
              <input class="e-multi-select e-input" name="${FIELD_NAME}" />
            </span>
            <span id="${FIELD_ID}_error" data-valmsg-for="${FIELD_NAME}" class="text-danger" hidden></span>
          </div>
        </form>
      </body></html>`);

      (globalThis as any).document = dom.window.document;
      (globalThis as any).HTMLElement = dom.window.HTMLElement;
      (globalThis as any).Event = dom.window.Event;

      (dom.window.document.getElementById(FIELD_ID) as any).ej2_instances = [ej2Instance];

      const mod = await import("../validation");
      validate = mod.validate;
      resetLiveClearForTests = mod.resetLiveClearForTests;
      resetLiveClearForTests();
    });

    it("error span is findable by ID ({fieldId}_error)", () => {
      expect(errorSpanById()).not.toBeNull();
      expect(errorSpanById()!.id).toBe(FIELD_ID + "_error");
    });

    it("error span is also findable by data-valmsg-for attribute", () => {
      expect(errorSpanByAttr()).not.toBeNull();
      expect(errorSpanByAttr()!.getAttribute("data-valmsg-for")).toBe(FIELD_NAME);
    });

    it("shows error message when validation fails", () => {
      ej2Instance.value = [];

      const d: ValidationDescriptor = {
        formId: "gather-form",
        fields: [{
          fieldName: FIELD_NAME,
          fieldId: FIELD_ID,
          vendor: "fusion",
          readExpr: "value",
          rules: [{ rule: "required", message: "Select at least one dietary restriction" }],
        }],
      };

      const result = validate(d);

      expect(result).toBe(false);

      // Error message should appear in the error span
      const span = errorSpanById()!;
      expect(span.textContent).toBe("Select at least one dietary restriction");
      expect(span.hasAttribute("hidden")).toBe(false);
      expect(span.style.display).not.toBe("none");

      // Error class should be on the component element (SF wrapper)
      const wrapper = document.getElementById(FIELD_ID)!;
      expect(wrapper.classList.contains("alis-has-error")).toBe(true);
    });

    it("clears error when validation passes", () => {
      ej2Instance.value = [];
      const d: ValidationDescriptor = {
        formId: "gather-form",
        fields: [{
          fieldName: FIELD_NAME,
          fieldId: FIELD_ID,
          vendor: "fusion",
          readExpr: "value",
          rules: [{ rule: "required", message: "required" }],
        }],
      };

      // First validate fails
      validate(d);
      expect(errorSpanById()!.textContent).toBe("required");

      // Set value and revalidate
      ej2Instance.value = ["gluten-free"];
      const result = validate(d);

      expect(result).toBe(true);
      expect(errorSpanById()!.textContent).toBe("");
      expect(document.getElementById(FIELD_ID)!.classList.contains("alis-has-error")).toBe(false);
    });

    it("fieldName (short) does NOT equal fieldId (long) — this is the real-world scenario", () => {
      expect(FIELD_NAME).not.toBe(FIELD_ID);
      expect(FIELD_ID).toContain("__");
      expect(FIELD_NAME).not.toContain("__");
    });
  });

  describe("native input with IdGenerator IDs", () => {
    const NATIVE_ID = `${SCOPE}__ResidentName`;
    const NATIVE_NAME = "ResidentName";

    beforeEach(async () => {
      const dom = new JSDOM(`<!DOCTYPE html><html><body>
        <form id="gather-form">
          <div>
            <label for="${NATIVE_ID}">Resident Name</label>
            <input id="${NATIVE_ID}" name="${NATIVE_NAME}" type="text" value="" />
            <span id="${NATIVE_ID}_error" data-valmsg-for="${NATIVE_NAME}" class="text-danger" hidden></span>
          </div>
        </form>
      </body></html>`);

      (globalThis as any).document = dom.window.document;
      (globalThis as any).HTMLElement = dom.window.HTMLElement;
      (globalThis as any).Event = dom.window.Event;

      const mod = await import("../validation");
      validate = mod.validate;
      resetLiveClearForTests = mod.resetLiveClearForTests;
      resetLiveClearForTests();
    });

    it("shows error with namespace-qualified fieldId", () => {
      const d: ValidationDescriptor = {
        formId: "gather-form",
        fields: [{
          fieldName: NATIVE_NAME,
          fieldId: NATIVE_ID,
          vendor: "native",
          readExpr: "value",
          rules: [{ rule: "required", message: "Name is required" }],
        }],
      };

      expect(validate(d)).toBe(false);

      const span = document.getElementById(NATIVE_ID + "_error")!;
      expect(span.textContent).toBe("Name is required");
      expect(span.hasAttribute("hidden")).toBe(false);
    });
  });
});
