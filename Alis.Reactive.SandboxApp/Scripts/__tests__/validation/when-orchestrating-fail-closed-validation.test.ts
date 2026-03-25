import { describe, it, expect, beforeEach } from "vitest";
import { JSDOM } from "jsdom";
import type { ValidationDescriptor, ValidationField } from "../../types";

let validate: typeof import("../../validation/orchestrator").validate;
let showServerErrors: typeof import("../../validation/orchestrator").showServerErrors;
let clearAll: typeof import("../../validation/orchestrator").clearAll;

function field(overrides: Partial<ValidationField> & { fieldName: string }): ValidationField {
  return {
    fieldName: overrides.fieldName,
    vendor: overrides.vendor,
    readExpr: overrides.readExpr,
    fieldId: overrides.fieldId,
    rules: overrides.rules ?? [],
    ...overrides,
  };
}

function enrichedField(id: string, rules: ValidationField["rules"] = []): ValidationField {
  return field({ fieldName: id, fieldId: id, vendor: "native", readExpr: "value", rules });
}

function unenrichedField(name: string, rules: ValidationField["rules"] = []): ValidationField {
  return field({ fieldName: name, rules });
}

function desc(formId: string, fields: ValidationField[]): ValidationDescriptor {
  return { formId, planId: "Test.Plan", fields };
}

function errSpan(fieldName: string, fieldId?: string): string {
  const idAttr = fieldId ? ` id="${fieldId}_error"` : "";
  return `<span${idAttr} data-valmsg-for="${fieldName}" hidden style="display:none"></span>`;
}

beforeEach(async () => {
  const dom = new JSDOM(`<!DOCTYPE html><html><body>
    <form id="form">
      <input id="Name" name="Name" value="" />
      ${errSpan("Name", "Name")}

      <input id="Email" name="Email" value="" />
      ${errSpan("Email", "Email")}

      <div id="hiddenWrapper" hidden>
        <input id="HiddenField" name="HiddenField" value="" />
        ${errSpan("HiddenField", "HiddenField")}
      </div>

      <input id="VisibleField" name="VisibleField" value="" />
      ${errSpan("VisibleField", "VisibleField")}

      <input id="IsVet" name="IsVet" type="checkbox" />
      <input id="VetId" name="VetId" value="" />
      ${errSpan("VetId", "VetId")}
    </form>
    <div id="Test_Plan_validation_summary" data-reactive-validation-summary="Test.Plan" hidden></div>
  </body></html>`);

  (globalThis as any).document = dom.window.document;
  (globalThis as any).HTMLElement = dom.window.HTMLElement;
  (globalThis as any).Event = dom.window.Event;

  const mod = await import("../../validation/orchestrator");
  validate = mod.validate;
  showServerErrors = mod.showServerErrors;
  clearAll = mod.clearAll;
});

function summaryDiv(): HTMLElement {
  return document.querySelector("[data-reactive-validation-summary]")!;
}

function summaryText(): string {
  return summaryDiv().textContent ?? "";
}

function errorSpan(name: string): HTMLSpanElement | null {
  return document.querySelector(`span[data-valmsg-for="${name}"]`);
}

// ══════════════════════════════════════════════════════════
// FORM CONTAINER CONTRACT
// ══════════════════════════════════════════════════════════

describe("When form container does not exist", () => {
  it("blocks when fields are declared (fail-closed)", () => {
    expect(validate(desc("nonexistent", [
      enrichedField("X", [{ rule: "required", message: "X required" }]),
    ]))).toBe(false);
  });

  it("passes when no fields declared", () => {
    expect(validate(desc("nonexistent", []))).toBe(true);
  });
});

// ══════════════════════════════════════════════════════════
// UNENRICHED FIELDS → SKIP (awaiting partial merge)
// ══════════════════════════════════════════════════════════

describe("When a field is unenriched (no fieldId/vendor/readExpr)", () => {
  it("blocks the request", () => {
    const result = validate(desc("form", [
      unenrichedField("Address.Street", [{ rule: "required", message: "Street required" }]),
    ]));
    expect(result).toBe(false);
  });

  it("adds first rule message to summary", () => {
    validate(desc("form", [
      unenrichedField("Address.Street", [
        { rule: "required", message: "Street required" },
        { rule: "minLength", message: "Too short", constraint: 5 },
      ]),
    ]));
    expect(summaryText()).toContain("Street required");
    expect(summaryText()).not.toContain("Too short"); // first-rule-wins
  });

  it("shows the summary div", () => {
    validate(desc("form", [
      unenrichedField("X", [{ rule: "required", message: "X" }]),
    ]));
    expect(summaryDiv().hasAttribute("hidden")).toBe(false);
  });

  it("does not prevent enriched fields from validating inline", () => {
    validate(desc("form", [
      unenrichedField("Unenriched", [{ rule: "required", message: "summary msg" }]),
      enrichedField("Name", [{ rule: "required", message: "Name required" }]),
    ]));
    expect(summaryText()).toContain("summary msg");
    expect(errorSpan("Name")!.textContent).toBe("Name required");
  });
});

// ══════════════════════════════════════════════════════════
// HIDDEN FIELDS → EVALUATE + SUMMARY
// ══════════════════════════════════════════════════════════

describe("When a hidden field fails validation", () => {
  it("blocks the request", () => {
    expect(validate(desc("form", [
      enrichedField("HiddenField", [{ rule: "required", message: "Hidden required" }]),
    ]))).toBe(false);
  });

  it("routes error to summary (not inline)", () => {
    validate(desc("form", [
      enrichedField("HiddenField", [{ rule: "required", message: "Hidden required" }]),
    ]));
    expect(summaryText()).toContain("Hidden required");
    expect(errorSpan("HiddenField")!.textContent).toBe("");
  });
});

describe("When a hidden field has valid value", () => {
  it("does not produce an error", () => {
    (document.getElementById("HiddenField") as HTMLInputElement).value = "has value";
    expect(validate(desc("form", [
      enrichedField("HiddenField", [{ rule: "required", message: "Hidden required" }]),
    ]))).toBe(true);
  });
});

describe("When a hidden field rule has a condition that evaluates false", () => {
  it("skips the rule (condition controls, not visibility)", () => {
    (document.getElementById("IsVet") as HTMLInputElement).checked = false;
    expect(validate(desc("form", [
      field({ fieldName: "IsVet", fieldId: "IsVet", vendor: "native", readExpr: "checked", rules: [] }),
      enrichedField("HiddenField", [
        { rule: "required", message: "Hidden req", when: { field: "IsVet", op: "truthy" } },
      ]),
    ]))).toBe(true);
  });
});

// ══════════════════════════════════════════════════════════
// MISSING DOM ELEMENT + WHEN CONDITION → SKIP (wizard partial)
// ══════════════════════════════════════════════════════════

describe("When an enriched field is missing from DOM but all rules have when conditions that evaluate false", () => {
  it("skips the field (condition false → no need to find the element)", () => {
    // Scenario: Step 2 wizard partial loaded for Alzheimer's diagnosis.
    // SystolicBP field does NOT exist in DOM (cardiac section not rendered).
    // Its rule has when: PrimaryDiagnosis == "Heart Disease" → false.
    // PrimaryDiagnosis IS in the DOM as a hidden field with value "Alzheimer's".
    const diagField = field({
      fieldName: "PrimaryDiagnosis", fieldId: "Name", // reuse existing DOM element as stand-in
      vendor: "native", readExpr: "value", rules: [],
    });
    // Set the stand-in element value to "Alzheimer's"
    (document.getElementById("Name") as HTMLInputElement).value = "Alzheimer's";

    const missingField = enrichedField("SystolicBP_MISSING", [
      { rule: "gt", message: "BP required", constraint: 0, coerceAs: "number",
        when: { field: "PrimaryDiagnosis", op: "eq", value: "Heart Disease" } },
    ]);
    // SystolicBP_MISSING does NOT exist in the DOM

    expect(validate(desc("form", [diagField, missingField]))).toBe(true);
  });

  it("blocks when the when condition evaluates TRUE but element is missing", () => {
    (document.getElementById("Name") as HTMLInputElement).value = "Heart Disease";

    const diagField = field({
      fieldName: "PrimaryDiagnosis", fieldId: "Name",
      vendor: "native", readExpr: "value", rules: [],
    });
    const missingField = enrichedField("SystolicBP_MISSING", [
      { rule: "gt", message: "BP required", constraint: 0, coerceAs: "number",
        when: { field: "PrimaryDiagnosis", op: "eq", value: "Heart Disease" } },
    ]);

    expect(validate(desc("form", [diagField, missingField]))).toBe(false);
  });

  it("skips when BOTH the field AND its condition field are missing (section not rendered)", () => {
    // Wizard scenario: cardiac section not rendered for Alzheimer's.
    // PacemakerModel missing from DOM. Its when condition references HasPacemaker.
    // HasPacemaker ALSO missing from DOM (not in validation fields either).
    // Both field and condition field absent = section not rendered = skip.
    const missingField = enrichedField("PacemakerModel_MISSING", [
      { rule: "required", message: "Model required",
        when: { field: "HasPacemaker", op: "truthy" } },
    ]);
    // HasPacemaker is NOT in the DOM and NOT in the fields list
    // → condReader returns null → unresolvable → should SKIP (not block)

    expect(validate(desc("form", [missingField]))).toBe(true);
  });

  it("blocks when condition field IS present and true but validated field is missing", () => {
    // HasPacemaker IS in the DOM (checkbox, checked = true).
    // PacemakerModel is NOT in the DOM.
    // Condition evaluates to true → field SHOULD exist but doesn't → block.
    (document.getElementById("IsVet") as HTMLInputElement).checked = true;

    const condField = field({
      fieldName: "HasPacemaker", fieldId: "IsVet", // reuse IsVet checkbox as stand-in
      vendor: "native", readExpr: "checked", rules: [],
    });
    const missingField = enrichedField("PacemakerModel_MISSING", [
      { rule: "required", message: "Model required",
        when: { field: "HasPacemaker", op: "truthy" } },
    ]);

    // Condition is TRUE (checked), field missing → MUST block
    expect(validate(desc("form", [condField, missingField]))).toBe(false);
  });

  it("blocks when field is missing with an unconditional rule", () => {
    // Field missing + no when condition = must block (fail-closed)
    const missingField = enrichedField("RequiredField_MISSING", [
      { rule: "required", message: "Field required" },
    ]);

    expect(validate(desc("form", [missingField]))).toBe(false);
  });
});

// ══════════════════════════════════════════════════════════
// VISIBLE ENRICHED FIELDS → INLINE
// ══════════════════════════════════════════════════════════

describe("When a visible enriched field fails validation", () => {
  it("shows error inline", () => {
    validate(desc("form", [
      enrichedField("Name", [{ rule: "required", message: "Name required" }]),
    ]));
    expect(errorSpan("Name")!.textContent).toBe("Name required");
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(true);
  });

  it("does NOT add to summary", () => {
    validate(desc("form", [
      enrichedField("Name", [{ rule: "required", message: "Name required" }]),
    ]));
    expect(summaryText()).toBe("");
    expect(summaryDiv().hasAttribute("hidden")).toBe(true);
  });
});

// ══════════════════════════════════════════════════════════
// SUMMARY LIFECYCLE
// ══════════════════════════════════════════════════════════

describe("When clearAll is called", () => {
  it("clears inline errors AND summary", () => {
    validate(desc("form", [
      enrichedField("Name", [{ rule: "required", message: "Name" }]),
      enrichedField("HiddenField", [{ rule: "required", message: "Hidden" }]),
    ]));
    expect(errorSpan("Name")!.textContent).toBe("Name");
    expect(summaryText()).toContain("Hidden");

    clearAll(desc("form", [
      enrichedField("Name", [{ rule: "required", message: "Name" }]),
      enrichedField("HiddenField", [{ rule: "required", message: "Hidden" }]),
    ]));
    expect(errorSpan("Name")!.textContent).toBe("");
    expect(summaryText()).toBe("");
    expect(summaryDiv().hasAttribute("hidden")).toBe(true);
  });
});

// ══════════════════════════════════════════════════════════
// SERVER ERRORS
// ══════════════════════════════════════════════════════════

describe("When server error has a visible error span", () => {
  it("shows error inline", () => {
    showServerErrors(desc("form", [enrichedField("Name")]),
      { errors: { Name: ["Server says bad"] } });
    expect(errorSpan("Name")!.textContent).toBe("Server says bad");
  });
});

describe("When server error for field with no span in DOM", () => {
  it("routes to summary", () => {
    showServerErrors(desc("form", [enrichedField("NonExistentField")]),
      { errors: { NonExistentField: ["Server says bad"] } });
    expect(summaryText()).toContain("Server says bad");
  });
});

describe("When server error for field in hidden section", () => {
  it("shows in span (span exists in DOM)", () => {
    showServerErrors(desc("form", [enrichedField("HiddenField")]),
      { errors: { HiddenField: ["Server says hidden"] } });
    expect(errorSpan("HiddenField")!.textContent).toBe("Server says hidden");
  });
});
