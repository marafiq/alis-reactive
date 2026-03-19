import { describe, it, expect, beforeEach, afterEach } from "vitest";
import { JSDOM } from "jsdom";
import type { ValidationDescriptor, ValidationField } from "../types";

let validate: typeof import("../validation").validate;
let wireLiveClearing: typeof import("../validation").wireLiveClearing;

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

function errSpan(fieldName: string): string {
  return `<span data-valmsg-for="${fieldName}" hidden></span>`;
}

function errorText(fieldName: string): string {
  return document.querySelector(`span[data-valmsg-for="${fieldName}"]`)?.textContent ?? "";
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
            ${errSpan("Name")}
          </div>
          <div>
            <label for="Email">Email</label>
            <input id="Email" name="Email" type="text" value="" />
            ${errSpan("Email")}
          </div>
        </form>
      </body></html>`);

      (globalThis as any).document = dom.window.document;
      (globalThis as any).HTMLElement = dom.window.HTMLElement;
      (globalThis as any).Event = dom.window.Event;

      const mod = await import("../validation");
      validate = mod.validate;
      wireLiveClearing = mod.wireLiveClearing;
    });

    it("shows error on validate, clears when user types", () => {
      const d = desc("myForm", [
        nativeField("Name", "Name", [{ rule: "required", message: "Name is required" }]),
      ]);

      // Wire live clearing BEFORE validate
      wireLiveClearing(d);

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

      wireLiveClearing(d);
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

      wireLiveClearing(d);
      validate(d);
      expect(hasError("Name")).toBe(true);

      changeNative("Name", "selected");
      expect(hasError("Name")).toBe(false);
    });

    it("does not double-wire on repeated calls", () => {
      const d = desc("myForm", [
        nativeField("Name", "Name", [{ rule: "required", message: "required" }]),
      ]);

      wireLiveClearing(d);
      wireLiveClearing(d); // second call should be no-op

      validate(d);
      typeInto("Name", "John");
      expect(hasError("Name")).toBe(false); // should clear once, not twice
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
            ${errSpan("Amount")}
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
      wireLiveClearing = mod.wireLiveClearing;
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

      wireLiveClearing(d);
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

    it("SHOULD clear error when user types in SF inner input", () => {
      const d = desc("myForm", [
        fusionField("Amount", "Amount", [{ rule: "required", message: "Amount is required" }]),
      ]);

      wireLiveClearing(d);
      ej2Instance.value = "";
      validate(d);
      expect(hasError("Amount")).toBe(true);

      // Simulate typing in the inner <input> that SF creates
      // The inner input fires a native "input" event — does it bubble?
      const innerInput = document.querySelector("input.e-numerictextbox") as HTMLInputElement;
      innerInput.value = "50";
      innerInput.dispatchEvent(new Event("input", { bubbles: true }));

      // Even if the DOM event bubbles, live-clear walks up looking for
      // data-valmsg-for — it goes: innerInput → e-control-wrapper → div → form
      // The span with data-valmsg-for is inside the div, so querySelector
      // on the div SHOULD find it.
      expect(hasError("Amount")).toBe(false);
    });
  });

  // ── Partial scenario — lazy component availability ──────

  describe("partial scenario — field becomes enriched lazily", () => {
    beforeEach(async () => {
      const dom = new JSDOM(`<!DOCTYPE html><html><body>
        <form id="myForm">
          <div>
            <label for="Name">Name</label>
            <input id="Name" name="Name" type="text" value="" />
            ${errSpan("Name")}
          </div>
        </form>
        <div data-alis-validation-summary="TestPlan" hidden></div>
      </body></html>`);

      (globalThis as any).document = dom.window.document;
      (globalThis as any).HTMLElement = dom.window.HTMLElement;
      (globalThis as any).Event = dom.window.Event;

      const mod = await import("../validation");
      validate = mod.validate;
      wireLiveClearing = mod.wireLiveClearing;
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
            ${errSpan("Visible")}
          </div>
          <div hidden>
            <input id="HiddenField" name="Hidden" value="" />
            ${errSpan("Hidden")}
          </div>
        </form>
        <div data-alis-validation-summary="TestPlan" hidden></div>
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
      expect(errorText("Visible")).toBe("visible required");
      expect(hasError("VisibleField")).toBe(true);
    });

    it("hidden field error goes to summary", () => {
      const d = desc("myForm", [
        nativeField("HiddenField", "Hidden", [{ rule: "required", message: "hidden required" }]),
      ], "TestPlan");

      validate(d);

      // Inline error should NOT be shown for hidden fields
      expect(errorText("Hidden")).toBe("");

      // Error should be in summary instead
      const summary = document.querySelector("[data-alis-validation-summary]")!;
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
            ${errSpan("Company")}
          </div>
          <div>
            <select id="Country" name="Country"><option value="">--</option><option value="US">US</option></select>
            ${errSpan("Country")}
          </div>
          <div>
            <input id="State" name="State" value="" />
            ${errSpan("State")}
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
