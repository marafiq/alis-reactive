import { describe, it, expect, beforeEach, afterEach } from "vitest";
import { JSDOM } from "jsdom";
import type { ValidationDescriptor, ValidationField } from "../types";

let validate: typeof import("../validation").validate;
let wireLiveValidation: typeof import("../validation").wireLiveValidation;
let resetLiveClearForTests: typeof import("../validation").resetLiveClearForTests;

// ── Helpers ──────────────────────────────────────────────

function nativeField(id: string, name: string, rules: ValidationField["rules"]): ValidationField {
  return { fieldName: name, fieldId: id, vendor: "native", readExpr: "value", rules };
}

function fusionField(id: string, name: string, rules: ValidationField["rules"]): ValidationField {
  return { fieldName: name, fieldId: id, vendor: "fusion", readExpr: "value", rules };
}

function desc(formId: string, fields: ValidationField[], planId?: string): ValidationDescriptor {
  return { formId, fields, planId };
}

function errSpan(fieldName: string, fieldId?: string): string {
  const idAttr = fieldId ? ` id="${fieldId}_error"` : "";
  return `<span${idAttr} data-valmsg-for="${fieldName}" hidden></span>`;
}

function errorText(fieldId: string): string {
  // Try ID-based lookup first (matches InputFieldBuilder {fieldId}_error convention)
  const byId = document.getElementById(fieldId + "_error");
  if (byId) return byId.textContent ?? "";
  // Fallback for tests without ID on span
  return document.querySelector(`span[data-valmsg-for="${fieldId}"]`)?.textContent ?? "";
}

function hasError(id: string): boolean {
  return document.getElementById(id)?.classList.contains("alis-has-error") ?? false;
}

function typeInto(id: string, value: string): void {
  const el = document.getElementById(id) as HTMLInputElement;
  el.value = value;
  el.dispatchEvent(new Event("input", { bubbles: true }));
}

function changeNative(id: string, value: string): void {
  const el = document.getElementById(id) as HTMLInputElement;
  el.value = value;
  el.dispatchEvent(new Event("change", { bubbles: true }));
}

// ── Setup ────────────────────────────────────────────────

describe("when live-clearing validation errors", () => {

  describe("native text input", () => {
    beforeEach(async () => {
      const dom = new JSDOM(`<!DOCTYPE html><html><body>
        <form id="myForm">
          <div>
            <label for="Name">Name</label>
            <input id="Name" name="Name" type="text" value="" />
            ${errSpan("Name", "Name")}
          </div>
          <div>
            <label for="Email">Email</label>
            <input id="Email" name="Email" type="text" value="" />
            ${errSpan("Email", "Email")}
          </div>
        </form>
      </body></html>`);

      (globalThis as any).document = dom.window.document;
      (globalThis as any).HTMLElement = dom.window.HTMLElement;
      (globalThis as any).Event = dom.window.Event;

      const mod = await import("../validation");
      validate = mod.validate;
      wireLiveValidation = mod.wireLiveValidation;
      resetLiveClearForTests = mod.resetLiveClearForTests;
      resetLiveClearForTests();
    });

    it("shows error on validate, clears when user types", () => {
      const d = desc("myForm", [
        nativeField("Name", "Name", [{ rule: "required", message: "Name is required" }]),
      ]);

      // Wire live clearing BEFORE validate
      wireLiveValidation(d);

      // Validate with empty field → error shown
      expect(validate(d)).toBe(false);
      expect(errorText("Name")).toBe("Name is required");
      expect(hasError("Name")).toBe(true);

      // User types → error auto-clears
      typeInto("Name", "John");
      expect(errorText("Name")).toBe("");
      expect(hasError("Name")).toBe(false);
    });

    it("clears only the field that was typed into, not other fields", () => {
      const d = desc("myForm", [
        nativeField("Name", "Name", [{ rule: "required", message: "Name required" }]),
        nativeField("Email", "Email", [{ rule: "required", message: "Email required" }]),
      ]);

      wireLiveValidation(d);
      validate(d);

      // Both fields have errors
      expect(errorText("Name")).toBe("Name required");
      expect(errorText("Email")).toBe("Email required");

      // Type into Name only
      typeInto("Name", "John");

      // Name cleared, Email still has error
      expect(errorText("Name")).toBe("");
      expect(errorText("Email")).toBe("Email required");
    });

    it("clears on change event (select/checkbox)", () => {
      const d = desc("myForm", [
        nativeField("Name", "Name", [{ rule: "required", message: "required" }]),
      ]);

      wireLiveValidation(d);
      validate(d);
      expect(hasError("Name")).toBe(true);

      changeNative("Name", "selected");
      expect(hasError("Name")).toBe(false);
    });

    it("does not double-wire on repeated calls", () => {
      const d = desc("myForm", [
        nativeField("Name", "Name", [{ rule: "required", message: "required" }]),
      ]);

      wireLiveValidation(d);
      wireLiveValidation(d); // second call should be no-op

      validate(d);
      typeInto("Name", "John");
      expect(hasError("Name")).toBe(false); // should clear once, not twice
    });
  });

  // ── Native compound component (radio group / checklist) ─
  //
  // Radio groups use a hidden input as the canonical element.
  // Auto-sync sets hiddenInput.value via set-prop mutation when
  // the user clicks a radio button. set-prop uses bracket notation
  // (root[prop] = val) which does NOT fire DOM events.
  // Live-clear listens for "input"/"change" on the hidden input.
  // BUG: live-clear never fires because set-prop doesn't dispatch events.

  describe("native compound component (radio group backing field)", () => {
    beforeEach(async () => {
      const dom = new JSDOM(`<!DOCTYPE html><html><body>
        <form id="myForm">
          <div>
            <input type="hidden" id="CareLevel" value="" />
            <div>
              <label><input type="radio" id="CareLevel_r0" name="CareLevel" value="independent" /></label>
              <label><input type="radio" id="CareLevel_r1" name="CareLevel" value="assisted" /></label>
            </div>
            ${errSpan("CareLevel", "CareLevel")}
          </div>
        </form>
      </body></html>`);

      (globalThis as any).document = dom.window.document;
      (globalThis as any).HTMLElement = dom.window.HTMLElement;
      (globalThis as any).Event = dom.window.Event;

      const mod = await import("../validation");
      validate = mod.validate;
      wireLiveValidation = mod.wireLiveValidation;
      resetLiveClearForTests = mod.resetLiveClearForTests;
      resetLiveClearForTests();
    });

    it("clears error when hidden input value is set via JS (simulating set-prop auto-sync)", () => {
      const d = desc("myForm", [
        nativeField("CareLevel", "CareLevel", [{ rule: "required", message: "Care Level is required" }]),
      ]);

      wireLiveValidation(d);

      // Validate: hidden input is empty → required fails
      expect(validate(d)).toBe(false);
      expect(errorText("CareLevel")).toBe("Care Level is required");
      expect(hasError("CareLevel")).toBe(true);

      // Simulate what auto-sync does: set-prop on hidden input
      // This is root[prop] = val — no DOM event fires
      const hidden = document.getElementById("CareLevel") as HTMLInputElement;
      hidden.value = "independent";

      // Dispatch change event (what the runtime SHOULD do after set-prop)
      hidden.dispatchEvent(new Event("change", { bubbles: true }));

      // Error should clear
      expect(errorText("CareLevel")).toBe("");
      expect(hasError("CareLevel")).toBe(false);
    });
  });

  // ── Syncfusion component ────────────────────────────────

  describe("fusion component", () => {
    let ej2Instance: any;
    let ej2Listeners: Record<string, ((...args: any[]) => void)[]>;

    beforeEach(async () => {
      ej2Listeners = {};
      ej2Instance = {
        value: "",
        addEventListener: (event: string, fn: (...args: any[]) => void) => {
          if (!ej2Listeners[event]) ej2Listeners[event] = [];
          ej2Listeners[event].push(fn);
        },
      };

      const dom = new JSDOM(`<!DOCTYPE html><html><body>
        <form id="myForm">
          <div>
            <label for="Amount">Amount</label>
            <span class="e-control-wrapper" id="Amount">
              <input class="e-numerictextbox e-input" name="Amount" />
            </span>
            ${errSpan("Amount", "Amount")}
          </div>
        </form>
      </body></html>`);

      (globalThis as any).document = dom.window.document;
      (globalThis as any).HTMLElement = dom.window.HTMLElement;
      (globalThis as any).Event = dom.window.Event;

      // Mount ej2 instance on the wrapper element (SF pattern)
      const wrapper = dom.window.document.getElementById("Amount")!;
      (wrapper as any).ej2_instances = [ej2Instance];

      const mod = await import("../validation");
      validate = mod.validate;
      wireLiveValidation = mod.wireLiveValidation;
      resetLiveClearForTests = mod.resetLiveClearForTests;
      resetLiveClearForTests();
    });

    it("validates fusion component value via resolveRoot", () => {
      const d = desc("myForm", [
        fusionField("Amount", "Amount", [{ rule: "required", message: "Amount is required" }]),
      ]);

      // ej2 value is empty
      ej2Instance.value = "";
      expect(validate(d)).toBe(false);
      expect(errorText("Amount")).toBe("Amount is required");

      // Set ej2 value and revalidate
      ej2Instance.value = "42";
      expect(validate(d)).toBe(true);
    });

    it("SHOULD clear error when SF component fires change callback", () => {
      const d = desc("myForm", [
        fusionField("Amount", "Amount", [{ rule: "required", message: "Amount is required" }]),
      ]);

      wireLiveValidation(d);
      ej2Instance.value = "";
      validate(d);
      expect(hasError("Amount")).toBe(true);

      // Simulate SF component change: user enters a value
      ej2Instance.value = "100";

      // SF fires change callback on ej2 instance, NOT a DOM event
      // This is how SF components communicate changes
      for (const fn of ej2Listeners["change"] ?? []) {
        fn({ value: 100 });
      }

      // EXPECTATION: error should be cleared
      // CURRENT REALITY: live-clear.ts listens for DOM events, not ej2 callbacks
      // This test documents the bug — it will FAIL until live-clear is fixed
      expect(hasError("Amount")).toBe(false);
    });

    // NOTE: We intentionally do NOT test typing into SF's inner <input>.
    // That would depend on SF's internal DOM structure. Our approach is
    // vendor-agnostic: resolveRoot → ej2 instance → addEventListener("change").
    // SF fires the change callback when value changes — that's our contract.
  });

  // ── Partial scenario — lazy component availability ──────

  describe("partial scenario — field becomes enriched lazily", () => {
    beforeEach(async () => {
      const dom = new JSDOM(`<!DOCTYPE html><html><body>
        <form id="myForm">
          <div>
            <label for="Name">Name</label>
            <input id="Name" name="Name" type="text" value="" />
            ${errSpan("Name", "Name")}
          </div>
        </form>
        <div id="TestPlan_validation_summary" data-reactive-validation-summary="TestPlan" hidden></div>
      </body></html>`);

      (globalThis as any).document = dom.window.document;
      (globalThis as any).HTMLElement = dom.window.HTMLElement;
      (globalThis as any).Event = dom.window.Event;

      const mod = await import("../validation");
      validate = mod.validate;
      wireLiveValidation = mod.wireLiveValidation;
      resetLiveClearForTests = mod.resetLiveClearForTests;
      resetLiveClearForTests();
    });

    it("unenriched field with unconditional rule blocks form", () => {
      // Field exists in validator but component not registered yet (partial not loaded)
      const d = desc("myForm", [
        { fieldName: "Address.Street", rules: [{ rule: "required", message: "Street required" }] },
      ], "TestPlan");

      // No fieldId, vendor, readExpr — unenriched
      expect(validate(d)).toBe(false);
    });

    it("unenriched field with conditional rule that evaluates false is skipped", () => {
      // Address.Street is required only when HasAddress is truthy
      // HasAddress is enriched and has value "false" (empty) — condition is false → skip
      const d = desc("myForm", [
        nativeField("Name", "Name", []),
        {
          fieldName: "Address.Street",
          rules: [{
            rule: "required",
            message: "Street required",
            when: { field: "Name", op: "truthy" }
          }]
        },
      ], "TestPlan");

      // Name is empty → condition "Name truthy" is false → Street rule skipped
      (document.getElementById("Name") as HTMLInputElement).value = "";
      expect(validate(d)).toBe(true);
    });

    it("unenriched field with conditional rule that evaluates true blocks", () => {
      const d = desc("myForm", [
        nativeField("Name", "Name", []),
        {
          fieldName: "Address.Street",
          rules: [{
            rule: "required",
            message: "Street required",
            when: { field: "Name", op: "truthy" }
          }]
        },
      ], "TestPlan");

      // Name has value → condition true → Street rule should apply → but unenriched → block
      (document.getElementById("Name") as HTMLInputElement).value = "John";
      expect(validate(d)).toBe(false);
    });

    it("enriched field outside form container is skipped (partial in different form)", () => {
      // Field is enriched but the element is NOT inside the form
      const outsideEl = document.createElement("input");
      outsideEl.id = "Outside";
      document.body.appendChild(outsideEl);

      const d = desc("myForm", [
        nativeField("Outside", "Outside", [{ rule: "required", message: "required" }]),
      ]);

      // Element exists but is outside the form → skip, not block
      expect(validate(d)).toBe(true);
    });
  });

  // ── Hidden field routing ────────────────────────────────

  describe("hidden field error routing", () => {
    beforeEach(async () => {
      const dom = new JSDOM(`<!DOCTYPE html><html><body>
        <form id="myForm">
          <div>
            <input id="VisibleField" name="Visible" value="" />
            ${errSpan("Visible", "VisibleField")}
          </div>
          <div hidden>
            <input id="HiddenField" name="Hidden" value="" />
            ${errSpan("Hidden", "HiddenField")}
          </div>
        </form>
        <div id="TestPlan_validation_summary" data-reactive-validation-summary="TestPlan" hidden></div>
      </body></html>`);

      (globalThis as any).document = dom.window.document;
      (globalThis as any).HTMLElement = dom.window.HTMLElement;
      (globalThis as any).Event = dom.window.Event;

      const mod = await import("../validation");
      validate = mod.validate;
    });

    it("visible field error goes inline", () => {
      const d = desc("myForm", [
        nativeField("VisibleField", "Visible", [{ rule: "required", message: "visible required" }]),
      ], "TestPlan");

      validate(d);
      expect(errorText("VisibleField")).toBe("visible required");
      expect(hasError("VisibleField")).toBe(true);
    });

    it("hidden field error goes to summary", () => {
      const d = desc("myForm", [
        nativeField("HiddenField", "Hidden", [{ rule: "required", message: "hidden required" }]),
      ], "TestPlan");

      validate(d);

      // Inline error should NOT be shown for hidden fields
      expect(errorText("HiddenField")).toBe("");

      // Error should be in summary instead
      const summary = document.getElementById("TestPlan_validation_summary")!;
      expect(summary.hasAttribute("hidden")).toBe(false); // summary shown
      expect(summary.querySelector("[data-valmsg-summary-for='Hidden']")?.textContent).toBe("hidden required");
    });
  });

  // ── Conditional rules ───────────────────────────────────

  describe("conditional validation rules", () => {
    beforeEach(async () => {
      const dom = new JSDOM(`<!DOCTYPE html><html><body>
        <form id="myForm">
          <div>
            <input id="IsEmployed" name="IsEmployed" type="checkbox" />
          </div>
          <div>
            <input id="Company" name="Company" value="" />
            ${errSpan("Company", "Company")}
          </div>
          <div>
            <select id="Country" name="Country"><option value="">--</option><option value="US">US</option></select>
            ${errSpan("Country", "Country")}
          </div>
          <div>
            <input id="State" name="State" value="" />
            ${errSpan("State", "State")}
          </div>
        </form>
      </body></html>`);

      (globalThis as any).document = dom.window.document;
      (globalThis as any).HTMLElement = dom.window.HTMLElement;
      (globalThis as any).Event = dom.window.Event;

      const mod = await import("../validation");
      validate = mod.validate;
    });

    it("skips rule when condition is falsy (checkbox unchecked)", () => {
      const d = desc("myForm", [
        { fieldName: "IsEmployed", fieldId: "IsEmployed", vendor: "native", readExpr: "checked", rules: [] },
        nativeField("Company", "Company", [{
          rule: "required",
          message: "Company required when employed",
          when: { field: "IsEmployed", op: "truthy" }
        }]),
      ]);

      // Checkbox unchecked → checked is false → condition "truthy" is false → skip
      expect(validate(d)).toBe(true);
    });

    it("applies rule when condition is truthy (checkbox checked)", () => {
      const d = desc("myForm", [
        { fieldName: "IsEmployed", fieldId: "IsEmployed", vendor: "native", readExpr: "checked", rules: [] },
        nativeField("Company", "Company", [{
          rule: "required",
          message: "Company required when employed",
          when: { field: "IsEmployed", op: "truthy" }
        }]),
      ]);

      (document.getElementById("IsEmployed") as HTMLInputElement).checked = true;
      expect(validate(d)).toBe(false);
      expect(errorText("Company")).toBe("Company required when employed");
    });

    it("applies rule when eq condition matches", () => {
      const d = desc("myForm", [
        nativeField("Country", "Country", []),
        nativeField("State", "State", [{
          rule: "required",
          message: "State required for US",
          when: { field: "Country", op: "eq", value: "US" }
        }]),
      ]);

      (document.getElementById("Country") as HTMLSelectElement).value = "US";
      expect(validate(d)).toBe(false);
      expect(errorText("State")).toBe("State required for US");
    });

    it("skips rule when eq condition does not match", () => {
      const d = desc("myForm", [
        nativeField("Country", "Country", []),
        nativeField("State", "State", [{
          rule: "required",
          message: "State required for US",
          when: { field: "Country", op: "eq", value: "US" }
        }]),
      ]);

      (document.getElementById("Country") as HTMLSelectElement).value = "";
      expect(validate(d)).toBe(true);
    });
  });
});
