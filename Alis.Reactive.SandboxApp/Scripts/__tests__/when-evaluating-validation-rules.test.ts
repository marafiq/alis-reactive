import { describe, it, expect, beforeEach } from "vitest";
import { JSDOM } from "jsdom";
import type { ValidationDescriptor, ValidationField } from "../types";

let validate: typeof import("../validation").validate;
let showServerErrors: typeof import("../validation").showServerErrors;
let clearAll: typeof import("../validation").clearAll;

function field(overrides: Partial<ValidationField> & { fieldId: string }): ValidationField {
  return {
    fieldName: overrides.fieldName ?? overrides.fieldId,
    vendor: overrides.vendor ?? "native",
    readExpr: overrides.readExpr ?? "value",
    rules: overrides.rules ?? [],
    ...overrides,
  };
}

function makeDesc(formId: string, fields: ValidationField[]): ValidationDescriptor {
  return { formId, fields };
}

function errSpan(fieldName: string): string {
  return `<span data-valmsg-for="${fieldName}" hidden style="display:none"></span>`;
}

beforeEach(async () => {
  const dom = new JSDOM(`<!DOCTYPE html><html><body>
    <form id="testForm">
      <input id="Name" name="Name" value="" />
      ${errSpan("Name")}

      <input id="Email" name="Email" value="" />
      ${errSpan("Email")}

      <input id="Phone" name="Phone" value="" />
      ${errSpan("Phone")}

      <input id="IsEmployed" name="IsEmployed" type="checkbox" />

      <input id="JobTitle" name="JobTitle" value="" />
      ${errSpan("JobTitle")}

      <input id="CareLevel" name="CareLevel" value="" />
      ${errSpan("CareLevel")}

      <input id="Physician" name="Physician" value="" />
      ${errSpan("Physician")}

      <input id="HasContact" name="HasContact" type="checkbox" />

      <input id="Reason" name="Reason" value="" />
      ${errSpan("Reason")}

      <div id="FusionDrop"></div>
      ${errSpan("FusionDrop")}

      <div id="hiddenSection" hidden>
        <input id="HiddenField" name="HiddenField" value="" />
        ${errSpan("HiddenField")}
      </div>

      <div id="visibleSection">
        <input id="VisibleField" name="VisibleField" value="" />
        ${errSpan("VisibleField")}
      </div>
    </form>
    <div data-alis-validation-summary hidden></div>
  </body></html>`);

  (globalThis as any).document = dom.window.document;
  (globalThis as any).HTMLElement = dom.window.HTMLElement;
  (globalThis as any).Event = dom.window.Event;

  const mod = await import("../validation");
  validate = mod.validate;
  showServerErrors = mod.showServerErrors;
  clearAll = mod.clearAll;
});

function errorSpan(fieldName: string): HTMLSpanElement | null {
  return document.querySelector(`span[data-valmsg-for="${fieldName}"]`);
}

// ── Form container contract ─────────────────────────────

describe("When form container does not exist", () => {
  it("returns true when form missing (fields can't be found in DOM)", () => {
    const desc = makeDesc("nonexistent-form", [
      field({ fieldId: "Name", rules: [{ rule: "required", message: "required" }] }),
    ]);
    expect(validate(desc)).toBe(true);
  });

  it("does not evaluate any field rules", () => {
    const desc = makeDesc("nonexistent-form", [
      field({ fieldId: "Name", rules: [{ rule: "required", message: "required" }] }),
    ]);
    validate(desc);
    expect(errorSpan("Name")!.textContent).toBe("");
  });
});

// ── Unenriched fields ───────────────────────────────────

describe("When a field has no fieldId (unenriched)", () => {
  it("does not try to read value from DOM", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "required", message: "required" }] }),
      { fieldName: "Address.Street", rules: [{ rule: "required", message: "Street required" }] } as ValidationField,
    ]);
    // Address.Street has no fieldId — should be skipped silently
    const result = validate(desc);
    // Name is empty → fails
    expect(result).toBe(false);
    // Address.Street with no fieldId should not show any error
    expect(errorSpan("Address.Street")).toBeNull();
  });

  it("other enriched fields still validate inline normally", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "required", message: "Name required" }] }),
      { fieldName: "Unenriched", rules: [{ rule: "required", message: "Unenriched required" }] } as ValidationField,
    ]);
    validate(desc);
    expect(errorSpan("Name")!.textContent).toBe("Name required");
  });
});

// ── Hidden field validation ─────────────────────────────

describe("When a visible field fails validation", () => {
  it("shows error inline next to the field", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "VisibleField", fieldName: "VisibleField", rules: [{ rule: "required", message: "visible required" }] }),
    ]);
    expect(validate(desc)).toBe(false);
    expect(errorSpan("VisibleField")!.textContent).toBe("visible required");
  });
});

describe("When a hidden field fails unconditional validation", () => {
  it("validates hidden fields and blocks the form (errors to summary)", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "HiddenField", fieldName: "HiddenField", rules: [{ rule: "required", message: "hidden required" }] }),
    ]);
    // Fail-closed: hidden fields still validate, errors route to summary
    expect(validate(desc)).toBe(false);
  });
});

describe("When a hidden field has valid value", () => {
  it("does not produce an error", () => {
    (document.getElementById("HiddenField") as HTMLInputElement).value = "has value";
    const desc = makeDesc("testForm", [
      field({ fieldId: "HiddenField", fieldName: "HiddenField", rules: [{ rule: "required", message: "hidden required" }] }),
    ]);
    expect(validate(desc)).toBe(true);
  });
});

// ── Condition evaluation ────────────────────────────────

describe("When condition source field is enriched and in DOM", () => {
  it("truthy: returns true when field value is truthy", () => {
    (document.getElementById("IsEmployed") as HTMLInputElement).checked = true;
    (document.getElementById("JobTitle") as HTMLInputElement).value = "";
    const desc = makeDesc("testForm", [
      field({ fieldId: "IsEmployed", fieldName: "IsEmployed", readExpr: "checked", rules: [] }),
      field({ fieldId: "JobTitle", fieldName: "JobTitle", rules: [
        { rule: "required", message: "Job required", when: { field: "IsEmployed", op: "truthy" } },
      ] }),
    ]);
    expect(validate(desc)).toBe(false);
  });

  it("falsy: returns true when field value is falsy", () => {
    (document.getElementById("HasContact") as HTMLInputElement).checked = false;
    (document.getElementById("Reason") as HTMLInputElement).value = "";
    const desc = makeDesc("testForm", [
      field({ fieldId: "HasContact", fieldName: "HasContact", readExpr: "checked", rules: [] }),
      field({ fieldId: "Reason", fieldName: "Reason", rules: [
        { rule: "required", message: "Reason required", when: { field: "HasContact", op: "falsy" } },
      ] }),
    ]);
    expect(validate(desc)).toBe(false);
    expect(errorSpan("Reason")!.textContent).toBe("Reason required");
  });

  it("eq: returns true when field value equals condition value", () => {
    (document.getElementById("CareLevel") as HTMLInputElement).value = "Memory Care";
    (document.getElementById("Phone") as HTMLInputElement).value = "";
    const desc = makeDesc("testForm", [
      field({ fieldId: "CareLevel", fieldName: "CareLevel", rules: [] }),
      field({ fieldId: "Phone", fieldName: "Phone", rules: [
        { rule: "required", message: "Phone required", when: { field: "CareLevel", op: "eq", value: "Memory Care" } },
      ] }),
    ]);
    expect(validate(desc)).toBe(false);
  });

  it("neq: returns true when field value does not equal condition value", () => {
    (document.getElementById("CareLevel") as HTMLInputElement).value = "Assisted";
    (document.getElementById("Physician") as HTMLInputElement).value = "";
    const desc = makeDesc("testForm", [
      field({ fieldId: "CareLevel", fieldName: "CareLevel", rules: [] }),
      field({ fieldId: "Physician", fieldName: "Physician", rules: [
        { rule: "required", message: "Physician required", when: { field: "CareLevel", op: "neq", value: "Independent" } },
      ] }),
    ]);
    expect(validate(desc)).toBe(false);
    expect(errorSpan("Physician")!.textContent).toBe("Physician required");
  });
});

describe("When condition source field is not enriched", () => {
  it("defaults to allowing the rule (returns true for condition)", () => {
    (document.getElementById("JobTitle") as HTMLInputElement).value = "";
    const desc = makeDesc("testForm", [
      // IsEmployed not in the field list — unenriched source
      field({ fieldId: "JobTitle", fieldName: "JobTitle", rules: [
        { rule: "required", message: "Job required", when: { field: "NonExistent", op: "truthy" } },
      ] }),
    ]);
    // When source is not found, evalCondition returns true → rule applies → fails
    expect(validate(desc)).toBe(false);
  });
});

// ── equalTo evaluation ──────────────────────────────────

describe("When equalTo peer is enriched and in DOM", () => {
  it("fails when values differ", () => {
    (document.getElementById("Name") as HTMLInputElement).value = "john@test.com";
    (document.getElementById("Email") as HTMLInputElement).value = "different@test.com";
    const desc = makeDesc("testForm", [
      field({ fieldId: "Name", fieldName: "Name", rules: [] }),
      field({ fieldId: "Email", fieldName: "Email", rules: [
        { rule: "equalTo", message: "must match Name", constraint: "Name" },
      ] }),
    ]);
    expect(validate(desc)).toBe(false);
    expect(errorSpan("Email")!.textContent).toBe("must match Name");
  });

  it("passes when values match", () => {
    (document.getElementById("Name") as HTMLInputElement).value = "same@test.com";
    (document.getElementById("Email") as HTMLInputElement).value = "same@test.com";
    const desc = makeDesc("testForm", [
      field({ fieldId: "Name", fieldName: "Name", rules: [] }),
      field({ fieldId: "Email", fieldName: "Email", rules: [
        { rule: "equalTo", message: "must match", constraint: "Name" },
      ] }),
    ]);
    expect(validate(desc)).toBe(true);
  });
});

describe("When equalTo peer is not enriched", () => {
  it("fails closed when peer is unresolvable (blocks the form)", () => {
    (document.getElementById("Email") as HTMLInputElement).value = "test@test.com";
    const desc = makeDesc("testForm", [
      // Name not in field list — peer not enriched
      field({ fieldId: "Email", fieldName: "Email", rules: [
        { rule: "equalTo", message: "must match", constraint: "NonExistent" },
      ] }),
    ]);
    // Fail-closed: peer unresolvable → rule fails → form blocked
    expect(validate(desc)).toBe(false);
  });
});

// ── Server error routing ────────────────────────────────

describe("When server error field has a visible error span", () => {
  it("shows error inline in the span", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Name", fieldName: "Name", rules: [] }),
    ]);
    showServerErrors(desc, { errors: { Name: ["Server says invalid"] } });
    expect(errorSpan("Name")!.textContent).toBe("Server says invalid");
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(true);
  });
});

describe("When server error field has no matching field in descriptor", () => {
  it("still shows in the error span if it exists in DOM", () => {
    const desc = makeDesc("testForm", []);
    showServerErrors(desc, { errors: { Name: ["Orphan error"] } });
    // The span exists in DOM but no matching field — span still gets populated
    expect(errorSpan("Name")!.textContent).toBe("Orphan error");
  });
});

// ── clearAll lifecycle ──────────────────────────────────

describe("When clearAll is called", () => {
  it("clears inline errors", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "required", message: "required" }] }),
    ]);
    validate(desc);
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(true);

    clearAll(desc);
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(false);
    expect(errorSpan("Name")!.textContent).toBe("");
  });
});

describe("When validate is called after clearAll", () => {
  it("shows fresh errors without stale state from previous validation", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "required", message: "Name required" }] }),
      field({ fieldId: "Email", rules: [{ rule: "required", message: "Email required" }] }),
    ]);

    // First validation
    validate(desc);
    expect(errorSpan("Name")!.textContent).toBe("Name required");
    expect(errorSpan("Email")!.textContent).toBe("Email required");

    // Fix Name
    (document.getElementById("Name") as HTMLInputElement).value = "John";

    // clearAll + revalidate
    clearAll(desc);
    validate(desc);

    // Name should pass now, Email still fails
    expect(errorSpan("Name")!.textContent).toBe("");
    expect(errorSpan("Email")!.textContent).toBe("Email required");
  });
});

// ── Fusion vendor condition ─────────────────────────────

describe("When condition reads from fusion vendor component", () => {
  it("evaluates condition from ej2 instance value", () => {
    (document.getElementById("FusionDrop") as any).ej2_instances = [{ value: "Memory Care" }];
    (document.getElementById("Phone") as HTMLInputElement).value = "";

    const desc = makeDesc("testForm", [
      field({ fieldId: "FusionDrop", fieldName: "FusionDrop", vendor: "fusion", readExpr: "value", rules: [] }),
      field({ fieldId: "Phone", fieldName: "Phone", rules: [
        { rule: "required", message: "Phone required for Memory Care",
          when: { field: "FusionDrop", op: "eq", value: "Memory Care" } },
      ] }),
    ]);
    expect(validate(desc)).toBe(false);
    expect(errorSpan("Phone")!.textContent).toBe("Phone required for Memory Care");
  });
});
