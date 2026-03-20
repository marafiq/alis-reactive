import { describe, it, expect, beforeEach } from "vitest";
import { JSDOM } from "jsdom";
import type { ValidationDescriptor, ValidationField } from "../types";

let validate: typeof import("../validation").validate;
let showServerErrors: typeof import("../validation").showServerErrors;
let clearAll: typeof import("../validation").clearAll;
let wireLiveValidation: typeof import("../validation").wireLiveValidation;

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

function errSpan(fieldName: string, fieldId?: string): string {
  const idAttr = fieldId ? ` id="${fieldId}_error"` : "";
  return `<span${idAttr} data-valmsg-for="${fieldName}" hidden></span>`;
}

beforeEach(async () => {
  const dom = new JSDOM(`<!DOCTYPE html><html><body>
    <form id="testForm">
      <div><input id="Name" name="Name" value="" />${errSpan("Name", "Name")}</div>
      <div><input id="Email" name="Email" value="" />${errSpan("Email", "Email")}</div>
      <div><input id="Phone" name="Phone" value="" />${errSpan("Phone", "Phone")}</div>
      <div><input id="Age" name="Age" type="number" value="" />${errSpan("Age", "Age")}</div>
      <div><input id="Website" name="Website" value="" />${errSpan("Website", "Website")}</div>
      <div><input id="Salary" name="Salary" type="number" value="" />${errSpan("Salary", "Salary")}</div>

      <input id="Password" name="Password" type="password" value="" />
      ${errSpan("Password", "Password")}

      <input id="ConfirmPassword" name="ConfirmPassword" type="password" value="" />
      ${errSpan("ConfirmPassword", "ConfirmPassword")}

      <input id="Tags" name="Tags" value="" />
      ${errSpan("Tags", "Tags")}

      <div><input id="IsEmployed" name="IsEmployed" type="checkbox" /></div>
      <div><input id="JobTitle" name="JobTitle" value="" />${errSpan("JobTitle", "JobTitle")}</div>
      <div><input id="Address_Street" name="Address.Street" value="" />${errSpan("Address.Street", "Address_Street")}</div>
      <div><input id="Address_City" name="Address.City" value="" />${errSpan("Address.City", "Address_City")}</div>
      <div><div id="FusionDrop"></div>${errSpan("FusionDrop", "FusionDrop")}</div>
      <div id="hiddenField" style="display:none">
        <div><input id="HiddenInput" name="HiddenInput" value="" />${errSpan("HiddenInput", "HiddenInput")}</div>
      </div>
    </form>
    <div data-reactive-validation-summary hidden></div>
  </body></html>`);

  (globalThis as any).document = dom.window.document;
  (globalThis as any).HTMLElement = dom.window.HTMLElement;
  (globalThis as any).Event = dom.window.Event;

  const mod = await import("../validation");
  validate = mod.validate;
  showServerErrors = mod.showServerErrors;
  clearAll = mod.clearAll;
  wireLiveValidation = mod.wireLiveValidation;
  mod.resetLiveClearForTests();
});

function errorSpan(fieldName: string): HTMLSpanElement | null {
  // For nested fields (Address.Street), the fieldId uses underscores (Address_Street)
  const fieldId = fieldName.replace(/\./g, "_");
  return document.getElementById(fieldId + "_error") as HTMLSpanElement | null;
}

// ── validate — required ───────────────────────────────────

describe("validate — required", () => {
  it("fails when field is empty", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "required", message: "Name is required" }] }),
    ]);
    expect(validate(desc)).toBe(false);
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(true);
    expect(errorSpan("Name")!.textContent).toBe("Name is required");
  });

  it("passes when field has value", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "required", message: "required" }] }),
    ]);
    (document.getElementById("Name")! as HTMLInputElement).value = "John";
    expect(validate(desc)).toBe(true);
  });

  it("fails for null-ish value", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "required", message: "required" }] }),
    ]);
    (document.getElementById("Name")! as HTMLInputElement).value = "";
    expect(validate(desc)).toBe(false);
  });
});

// ── validate — minLength ──────────────────────────────────

describe("validate — minLength", () => {
  it("fails when too short", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "minLength", message: "too short", constraint: 3 }] }),
    ]);
    (document.getElementById("Name")! as HTMLInputElement).value = "ab";
    expect(validate(desc)).toBe(false);
  });

  it("passes at exact length", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "minLength", message: "too short", constraint: 3 }] }),
    ]);
    (document.getElementById("Name")! as HTMLInputElement).value = "abc";
    expect(validate(desc)).toBe(true);
  });
});

// ── validate — maxLength ──────────────────────────────────

describe("validate — maxLength", () => {
  it("fails when too long", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "maxLength", message: "too long", constraint: 5 }] }),
    ]);
    (document.getElementById("Name")! as HTMLInputElement).value = "abcdef";
    expect(validate(desc)).toBe(false);
  });

  it("passes at exact length", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "maxLength", message: "too long", constraint: 5 }] }),
    ]);
    (document.getElementById("Name")! as HTMLInputElement).value = "abcde";
    expect(validate(desc)).toBe(true);
  });
});

// ── validate — email ──────────────────────────────────────

describe("validate — email", () => {
  it("fails for invalid email", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Email", rules: [{ rule: "email", message: "bad email" }] }),
    ]);
    (document.getElementById("Email")! as HTMLInputElement).value = "notanemail";
    expect(validate(desc)).toBe(false);
  });

  it("passes for valid email", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Email", rules: [{ rule: "email", message: "bad email" }] }),
    ]);
    (document.getElementById("Email")! as HTMLInputElement).value = "user@example.com";
    expect(validate(desc)).toBe(true);
  });

  it("passes when empty (not required)", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Email", rules: [{ rule: "email", message: "bad email" }] }),
    ]);
    (document.getElementById("Email")! as HTMLInputElement).value = "";
    expect(validate(desc)).toBe(true);
  });
});

// ── validate — regex ──────────────────────────────────────

describe("validate — regex", () => {
  it("fails when pattern does not match", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Phone", rules: [{ rule: "regex", message: "bad format", constraint: "^\\d{3}-\\d{3}-\\d{4}$" }] }),
    ]);
    (document.getElementById("Phone")! as HTMLInputElement).value = "123";
    expect(validate(desc)).toBe(false);
  });

  it("passes when pattern matches", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Phone", rules: [{ rule: "regex", message: "bad format", constraint: "^\\d{3}-\\d{3}-\\d{4}$" }] }),
    ]);
    (document.getElementById("Phone")! as HTMLInputElement).value = "123-456-7890";
    expect(validate(desc)).toBe(true);
  });

  it("fails closed on invalid regex (blocks the form)", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Phone", rules: [{ rule: "regex", message: "bad", constraint: "[invalid" }] }),
    ]);
    (document.getElementById("Phone")! as HTMLInputElement).value = "test";
    expect(validate(desc)).toBe(false);
  });
});

// ── validate — url ────────────────────────────────────────

describe("validate — url", () => {
  it("fails for non-URL", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Website", rules: [{ rule: "url", message: "bad url" }] }),
    ]);
    (document.getElementById("Website")! as HTMLInputElement).value = "not-a-url";
    expect(validate(desc)).toBe(false);
  });

  it("passes for http/https URL", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Website", rules: [{ rule: "url", message: "bad url" }] }),
    ]);
    (document.getElementById("Website")! as HTMLInputElement).value = "https://example.com";
    expect(validate(desc)).toBe(true);
  });
});

// ── validate — min / max / range ──────────────────────────

describe("validate — min", () => {
  it("fails below minimum", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Salary", rules: [{ rule: "min", message: "too low", constraint: 100 }] }),
    ]);
    (document.getElementById("Salary")! as HTMLInputElement).value = "50";
    expect(validate(desc)).toBe(false);
  });

  it("passes at minimum", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Salary", rules: [{ rule: "min", message: "too low", constraint: 100 }] }),
    ]);
    (document.getElementById("Salary")! as HTMLInputElement).value = "100";
    expect(validate(desc)).toBe(true);
  });
});

describe("validate — max", () => {
  it("fails above maximum", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Salary", rules: [{ rule: "max", message: "too high", constraint: 500 }] }),
    ]);
    (document.getElementById("Salary")! as HTMLInputElement).value = "600";
    expect(validate(desc)).toBe(false);
  });

  it("passes at maximum", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Salary", rules: [{ rule: "max", message: "too high", constraint: 500 }] }),
    ]);
    (document.getElementById("Salary")! as HTMLInputElement).value = "500";
    expect(validate(desc)).toBe(true);
  });
});

describe("validate — range", () => {
  it("fails outside range", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Age", rules: [{ rule: "range", message: "out of range", constraint: [0, 120] }] }),
    ]);
    (document.getElementById("Age")! as HTMLInputElement).value = "-1";
    expect(validate(desc)).toBe(false);
  });

  it("passes within range", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Age", rules: [{ rule: "range", message: "out of range", constraint: [0, 120] }] }),
    ]);
    (document.getElementById("Age")! as HTMLInputElement).value = "25";
    expect(validate(desc)).toBe(true);
  });
});

// ── validate — equalTo ────────────────────────────────────

describe("validate — equalTo", () => {
  it("fails when values differ", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Password", fieldName: "Password", rules: [] }),
      field({ fieldId: "ConfirmPassword", fieldName: "ConfirmPassword", rules: [
        { rule: "equalTo", message: "must match", constraint: "Password" },
      ] }),
    ]);
    (document.getElementById("Password")! as HTMLInputElement).value = "secret";
    (document.getElementById("ConfirmPassword")! as HTMLInputElement).value = "different";
    expect(validate(desc)).toBe(false);
  });

  it("passes when values match", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Password", fieldName: "Password", rules: [] }),
      field({ fieldId: "ConfirmPassword", fieldName: "ConfirmPassword", rules: [
        { rule: "equalTo", message: "must match", constraint: "Password" },
      ] }),
    ]);
    (document.getElementById("Password")! as HTMLInputElement).value = "secret";
    (document.getElementById("ConfirmPassword")! as HTMLInputElement).value = "secret";
    expect(validate(desc)).toBe(true);
  });
});

// ── validate — atLeastOne ─────────────────────────────────

describe("validate — atLeastOne", () => {
  it("fails when empty", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Tags", rules: [{ rule: "atLeastOne", message: "need one" }] }),
    ]);
    (document.getElementById("Tags")! as HTMLInputElement).value = "";
    expect(validate(desc)).toBe(false);
  });

  it("passes when non-empty", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Tags", rules: [{ rule: "atLeastOne", message: "need one" }] }),
    ]);
    (document.getElementById("Tags")! as HTMLInputElement).value = "tag1";
    expect(validate(desc)).toBe(true);
  });
});

// ── validate — conditional rules ──────────────────────────

describe("validate — conditional rules", () => {
  it("applies when truthy condition is met", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "IsEmployed", fieldName: "IsEmployed", readExpr: "checked", rules: [] }),
      field({ fieldId: "JobTitle", fieldName: "JobTitle", rules: [
        { rule: "required", message: "Job title required", when: { field: "IsEmployed", op: "truthy" } },
      ] }),
    ]);
    (document.getElementById("IsEmployed")! as HTMLInputElement).checked = true;
    (document.getElementById("JobTitle")! as HTMLInputElement).value = "";
    expect(validate(desc)).toBe(false);
  });

  it("skips when truthy condition is not met", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "IsEmployed", fieldName: "IsEmployed", readExpr: "checked", rules: [] }),
      field({ fieldId: "JobTitle", fieldName: "JobTitle", rules: [
        { rule: "required", message: "Job title required", when: { field: "IsEmployed", op: "truthy" } },
      ] }),
    ]);
    (document.getElementById("IsEmployed")! as HTMLInputElement).checked = false;
    expect(validate(desc)).toBe(true);
  });

  it("evaluates eq condition", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Name", fieldName: "Name", rules: [] }),
      field({ fieldId: "Email", fieldName: "Email", rules: [
        { rule: "required", message: "required for VIP", when: { field: "Name", op: "eq", value: "VIP" } },
      ] }),
    ]);
    (document.getElementById("Name")! as HTMLInputElement).value = "VIP";
    (document.getElementById("Email")! as HTMLInputElement).value = "";
    expect(validate(desc)).toBe(false);
  });

  it("evaluates neq condition", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Name", fieldName: "Name", rules: [] }),
      field({ fieldId: "Email", fieldName: "Email", rules: [
        { rule: "required", message: "required", when: { field: "Name", op: "neq", value: "SKIP" } },
      ] }),
    ]);
    (document.getElementById("Name")! as HTMLInputElement).value = "SKIP";
    (document.getElementById("Email")! as HTMLInputElement).value = "";
    expect(validate(desc)).toBe(true);
  });

  it("evaluates falsy condition", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "IsEmployed", fieldName: "IsEmployed", readExpr: "checked", rules: [] }),
      field({ fieldId: "JobTitle", fieldName: "JobTitle", rules: [
        { rule: "required", message: "explain", when: { field: "IsEmployed", op: "falsy" } },
      ] }),
    ]);
    (document.getElementById("IsEmployed")! as HTMLInputElement).checked = false;
    (document.getElementById("JobTitle")! as HTMLInputElement).value = "";
    expect(validate(desc)).toBe(false);
  });
});

// ── validate — first-fail-wins ────────────────────────────

describe("validate — first-fail-wins", () => {
  it("only shows first error per field", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Name", rules: [
        { rule: "required", message: "required" },
        { rule: "minLength", message: "too short", constraint: 3 },
      ] }),
    ]);
    (document.getElementById("Name")! as HTMLInputElement).value = "";
    validate(desc);
    expect(errorSpan("Name")!.textContent).toBe("required");
  });
});

// ── validate — fusion vendor ──────────────────────────────

describe("validate — fusion vendor", () => {
  it("reads value via readExpr from vendor root", () => {
    (document.getElementById("FusionDrop")! as any).ej2_instances = [{ value: "" }];
    const desc = makeDesc("testForm", [
      field({
        fieldId: "FusionDrop",
        fieldName: "FusionDrop",
        vendor: "fusion",
        readExpr: "value",
        rules: [{ rule: "required", message: "required" }],
      }),
    ]);
    expect(validate(desc)).toBe(false);
  });
});

// ── validate — hidden fields ──────────────────────────────

describe("validate — hidden fields (fail-closed)", () => {
  it("validates hidden fields and blocks the form", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "HiddenInput", rules: [{ rule: "required", message: "required" }] }),
    ]);
    // Fail-closed: hidden fields validate, errors route to summary
    expect(validate(desc)).toBe(false);
  });
});

// ── validate — nested properties ──────────────────────────

describe("validate — nested properties", () => {
  it("validates nested field with underscore ID and dotted name", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Address_Street", fieldName: "Address.Street", rules: [
        { rule: "required", message: "Street required" },
      ] }),
    ]);
    (document.getElementById("Address_Street")! as HTMLInputElement).value = "";
    expect(validate(desc)).toBe(false);
    expect(errorSpan("Address.Street")!.textContent).toBe("Street required");
  });
});

// ── showServerErrors ──────────────────────────────────────

describe("showServerErrors", () => {
  it("shows errors from ProblemDetails format", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Name", fieldName: "Name", rules: [] }),
    ]);
    showServerErrors(desc, { errors: { Name: ["Name is required"] } });
    expect(errorSpan("Name")!.textContent).toBe("Name is required");
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(true);
  });

  it("rejects non-ProblemDetails flat format (fail-closed)", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Email", fieldName: "Email", rules: [] }),
    ]);
    // Flat format { Email: [...] } is NOT ProblemDetails — must be rejected
    showServerErrors(desc, { Email: ["Invalid email"] });
    expect(errorSpan("Email")!.textContent).toBe("");
  });

  it("handles dotted nested property names", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Address_Street", fieldName: "Address.Street", rules: [] }),
    ]);
    showServerErrors(desc, { errors: { "Address.Street": ["Street is required"] } });
    expect(errorSpan("Address.Street")!.textContent).toBe("Street is required");
  });
});

// ── clearAll ──────────────────────────────────────────────

describe("clearAll", () => {
  it("removes error class and hides spans", () => {
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

// ── live clearing ─────────────────────────────────────────

describe("live clearing", () => {
  it("input event clears field error", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "required", message: "required" }] }),
    ]);
    wireLiveValidation(desc);
    validate(desc);
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(true);

    document.getElementById("Name")!.dispatchEvent(new Event("input", { bubbles: true }));
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(false);
  });

  it("change event re-validates — error stays if field still invalid", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "required", message: "required" }] }),
    ]);
    wireLiveValidation(desc);
    validate(desc);
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(true);

    // Change on empty field → re-validates → still invalid → error stays
    document.getElementById("Name")!.dispatchEvent(new Event("change", { bubbles: true }));
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(true);
  });

  it("change event re-validates — error clears if field becomes valid", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "required", message: "required" }] }),
    ]);
    wireLiveValidation(desc);
    validate(desc);
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(true);

    // Set value, then change → re-validates → valid → error clears
    (document.getElementById("Name") as HTMLInputElement).value = "John";
    document.getElementById("Name")!.dispatchEvent(new Event("change", { bubbles: true }));
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(false);
  });

  it("does not wire duplicate listeners", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "required", message: "required" }] }),
    ]);
    wireLiveValidation(desc);
    wireLiveValidation(desc);
    validate(desc);

    document.getElementById("Name")!.dispatchEvent(new Event("input", { bubbles: true }));
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(false);
  });
});
