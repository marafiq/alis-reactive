import { describe, it, expect, beforeEach } from "vitest";
import { JSDOM } from "jsdom";
import type { ValidationDescriptor, ValidationField } from "../types";

let register: typeof import("../forms").register;
let validate: typeof import("../forms").validate;
let showFieldErrors: typeof import("../forms").showFieldErrors;
let clearErrors: typeof import("../forms").clearErrors;
let _reset: typeof import("../forms")._reset;

function field(overrides: Partial<ValidationField> & { fieldId: string }): ValidationField {
  return {
    fieldName: overrides.fieldName ?? overrides.fieldId,
    errorId: overrides.errorId ?? `err_${overrides.fieldId}`,
    vendor: overrides.vendor ?? "native",
    rules: overrides.rules ?? [],
    ...overrides,
  };
}

function makeDesc(formId: string, fields: ValidationField[]): ValidationDescriptor {
  return { formId, fields };
}

beforeEach(async () => {
  // Reset modules so each test gets fresh state
  const dom = new JSDOM(`<!DOCTYPE html><html><body>
    <form id="testForm">
      <input id="Name" name="Name" value="" />
      <span id="err_Name" style="display:none"></span>

      <input id="Email" name="Email" value="" />
      <span id="err_Email" style="display:none"></span>

      <input id="Phone" name="Phone" value="" />
      <span id="err_Phone" style="display:none"></span>

      <input id="Age" name="Age" type="number" value="" />
      <span id="err_Age" style="display:none"></span>

      <input id="Website" name="Website" value="" />
      <span id="err_Website" style="display:none"></span>

      <input id="Salary" name="Salary" type="number" value="" />
      <span id="err_Salary" style="display:none"></span>

      <input id="Password" name="Password" type="password" value="" />
      <span id="err_Password" style="display:none"></span>

      <input id="ConfirmPassword" name="ConfirmPassword" type="password" value="" />
      <span id="err_ConfirmPassword" style="display:none"></span>

      <input id="Tags" name="Tags" value="" />
      <span id="err_Tags" style="display:none"></span>

      <input id="IsEmployed" name="IsEmployed" type="checkbox" />
      <span id="err_IsEmployed" style="display:none"></span>

      <input id="JobTitle" name="JobTitle" value="" />
      <span id="err_JobTitle" style="display:none"></span>

      <input id="Address_Street" name="Address.Street" value="" />
      <span id="err_Address_Street" style="display:none"></span>

      <input id="Address_City" name="Address.City" value="" />
      <span id="err_Address_City" style="display:none"></span>

      <div id="FusionDrop"></div>
      <span id="err_FusionDrop" style="display:none"></span>

      <div id="hiddenField" style="display:none">
        <input id="HiddenInput" name="HiddenInput" value="" />
        <span id="err_HiddenInput" style="display:none"></span>
      </div>
    </form>
  </body></html>`);

  (globalThis as any).document = dom.window.document;
  (globalThis as any).HTMLElement = dom.window.HTMLElement;

  // Dynamic import — module is cached but we reset internal state
  const mod = await import("../forms");
  register = mod.register;
  validate = mod.validate;
  showFieldErrors = mod.showFieldErrors;
  clearErrors = mod.clearErrors;
  _reset = mod._reset;

  // Reset forms module state between tests
  _reset();
});

// ── register ──────────────────────────────────────────────

describe("register", () => {
  it("stores descriptor and builds byName map", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Name", fieldName: "Name", rules: [{ rule: "required", message: "required" }] }),
    ]);
    register(desc);
    // Verify it was stored by running validate (returns true for empty form with valid data)
    document.getElementById("Name")!.setAttribute("value", "John");
    (document.getElementById("Name")! as HTMLInputElement).value = "John";
    const result = validate("testForm");
    expect(result).toBe(true);
  });
});

// ── validate — required ───────────────────────────────────

describe("validate — required", () => {
  it("fails when field is empty", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "required", message: "Name is required" }] }),
    ]));
    const result = validate("testForm");
    expect(result).toBe(false);
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(true);
    expect(document.getElementById("err_Name")!.textContent).toBe("Name is required");
  });

  it("fails when value is null-ish", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "required", message: "required" }] }),
    ]));
    (document.getElementById("Name")! as HTMLInputElement).value = "";
    expect(validate("testForm")).toBe(false);
  });

  it("passes when field has value", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "required", message: "required" }] }),
    ]));
    (document.getElementById("Name")! as HTMLInputElement).value = "John";
    expect(validate("testForm")).toBe(true);
  });
});

// ── validate — minLength ──────────────────────────────────

describe("validate — minLength", () => {
  it("fails when too short", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "minLength", message: "too short", constraint: 3 }] }),
    ]));
    (document.getElementById("Name")! as HTMLInputElement).value = "ab";
    expect(validate("testForm")).toBe(false);
  });

  it("passes at exact length", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "minLength", message: "too short", constraint: 3 }] }),
    ]));
    (document.getElementById("Name")! as HTMLInputElement).value = "abc";
    expect(validate("testForm")).toBe(true);
  });
});

// ── validate — maxLength ──────────────────────────────────

describe("validate — maxLength", () => {
  it("fails when too long", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "maxLength", message: "too long", constraint: 5 }] }),
    ]));
    (document.getElementById("Name")! as HTMLInputElement).value = "abcdef";
    expect(validate("testForm")).toBe(false);
  });

  it("passes at exact length", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "maxLength", message: "too long", constraint: 5 }] }),
    ]));
    (document.getElementById("Name")! as HTMLInputElement).value = "abcde";
    expect(validate("testForm")).toBe(true);
  });
});

// ── validate — email ──────────────────────────────────────

describe("validate — email", () => {
  it("fails for invalid email", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Email", rules: [{ rule: "email", message: "bad email" }] }),
    ]));
    (document.getElementById("Email")! as HTMLInputElement).value = "notanemail";
    expect(validate("testForm")).toBe(false);
  });

  it("passes for valid email", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Email", rules: [{ rule: "email", message: "bad email" }] }),
    ]));
    (document.getElementById("Email")! as HTMLInputElement).value = "user@example.com";
    expect(validate("testForm")).toBe(true);
  });

  it("passes when empty (not required)", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Email", rules: [{ rule: "email", message: "bad email" }] }),
    ]));
    (document.getElementById("Email")! as HTMLInputElement).value = "";
    expect(validate("testForm")).toBe(true);
  });
});

// ── validate — regex ──────────────────────────────────────

describe("validate — regex", () => {
  it("fails when pattern does not match", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Phone", rules: [{ rule: "regex", message: "bad format", constraint: "^\\d{3}-\\d{3}-\\d{4}$" }] }),
    ]));
    (document.getElementById("Phone")! as HTMLInputElement).value = "123";
    expect(validate("testForm")).toBe(false);
  });

  it("passes when pattern matches", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Phone", rules: [{ rule: "regex", message: "bad format", constraint: "^\\d{3}-\\d{3}-\\d{4}$" }] }),
    ]));
    (document.getElementById("Phone")! as HTMLInputElement).value = "123-456-7890";
    expect(validate("testForm")).toBe(true);
  });

  it("handles invalid regex gracefully", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Phone", rules: [{ rule: "regex", message: "bad", constraint: "[invalid" }] }),
    ]));
    (document.getElementById("Phone")! as HTMLInputElement).value = "test";
    // Should not throw, returns valid (false = don't block on bad regex)
    expect(validate("testForm")).toBe(true);
  });
});

// ── validate — url ────────────────────────────────────────

describe("validate — url", () => {
  it("fails for non-URL", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Website", rules: [{ rule: "url", message: "bad url" }] }),
    ]));
    (document.getElementById("Website")! as HTMLInputElement).value = "not-a-url";
    expect(validate("testForm")).toBe(false);
  });

  it("passes for http URL", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Website", rules: [{ rule: "url", message: "bad url" }] }),
    ]));
    (document.getElementById("Website")! as HTMLInputElement).value = "http://example.com";
    expect(validate("testForm")).toBe(true);
  });

  it("passes for https URL", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Website", rules: [{ rule: "url", message: "bad url" }] }),
    ]));
    (document.getElementById("Website")! as HTMLInputElement).value = "https://example.com/path";
    expect(validate("testForm")).toBe(true);
  });
});

// ── validate — min ────────────────────────────────────────

describe("validate — min", () => {
  it("fails below minimum", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Salary", rules: [{ rule: "min", message: "too low", constraint: 100 }] }),
    ]));
    (document.getElementById("Salary")! as HTMLInputElement).value = "50";
    expect(validate("testForm")).toBe(false);
  });

  it("passes at minimum", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Salary", rules: [{ rule: "min", message: "too low", constraint: 100 }] }),
    ]));
    (document.getElementById("Salary")! as HTMLInputElement).value = "100";
    expect(validate("testForm")).toBe(true);
  });
});

// ── validate — max ────────────────────────────────────────

describe("validate — max", () => {
  it("fails above maximum", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Salary", rules: [{ rule: "max", message: "too high", constraint: 500 }] }),
    ]));
    (document.getElementById("Salary")! as HTMLInputElement).value = "600";
    expect(validate("testForm")).toBe(false);
  });

  it("passes at maximum", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Salary", rules: [{ rule: "max", message: "too high", constraint: 500 }] }),
    ]));
    (document.getElementById("Salary")! as HTMLInputElement).value = "500";
    expect(validate("testForm")).toBe(true);
  });
});

// ── validate — range ──────────────────────────────────────

describe("validate — range", () => {
  it("fails below range", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Age", rules: [{ rule: "range", message: "out of range", constraint: [0, 120] }] }),
    ]));
    (document.getElementById("Age")! as HTMLInputElement).value = "-1";
    expect(validate("testForm")).toBe(false);
  });

  it("fails above range", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Age", rules: [{ rule: "range", message: "out of range", constraint: [0, 120] }] }),
    ]));
    (document.getElementById("Age")! as HTMLInputElement).value = "121";
    expect(validate("testForm")).toBe(false);
  });

  it("passes within range", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Age", rules: [{ rule: "range", message: "out of range", constraint: [0, 120] }] }),
    ]));
    (document.getElementById("Age")! as HTMLInputElement).value = "25";
    expect(validate("testForm")).toBe(true);
  });
});

// ── validate — equalTo ────────────────────────────────────

describe("validate — equalTo", () => {
  it("fails when values differ", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Password", fieldName: "Password", rules: [] }),
      field({ fieldId: "ConfirmPassword", fieldName: "ConfirmPassword", rules: [
        { rule: "equalTo", message: "must match password", constraint: "Password" },
      ] }),
    ]));
    (document.getElementById("Password")! as HTMLInputElement).value = "secret";
    (document.getElementById("ConfirmPassword")! as HTMLInputElement).value = "different";
    expect(validate("testForm")).toBe(false);
  });

  it("passes when values match", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Password", fieldName: "Password", rules: [] }),
      field({ fieldId: "ConfirmPassword", fieldName: "ConfirmPassword", rules: [
        { rule: "equalTo", message: "must match password", constraint: "Password" },
      ] }),
    ]));
    (document.getElementById("Password")! as HTMLInputElement).value = "secret";
    (document.getElementById("ConfirmPassword")! as HTMLInputElement).value = "secret";
    expect(validate("testForm")).toBe(true);
  });
});

// ── validate — atLeastOne ─────────────────────────────────

describe("validate — atLeastOne", () => {
  it("fails when empty", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Tags", rules: [{ rule: "atLeastOne", message: "need at least one" }] }),
    ]));
    (document.getElementById("Tags")! as HTMLInputElement).value = "";
    expect(validate("testForm")).toBe(false);
  });

  it("passes when non-empty", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Tags", rules: [{ rule: "atLeastOne", message: "need at least one" }] }),
    ]));
    (document.getElementById("Tags")! as HTMLInputElement).value = "tag1";
    expect(validate("testForm")).toBe(true);
  });
});

// ── validate — conditional rules ──────────────────────────

describe("validate — conditional rules", () => {
  it("applies when truthy condition is met", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "IsEmployed", fieldName: "IsEmployed", rules: [] }),
      field({ fieldId: "JobTitle", fieldName: "JobTitle", rules: [
        { rule: "required", message: "Job title required", when: { field: "IsEmployed", op: "truthy" } },
      ] }),
    ]));
    (document.getElementById("IsEmployed")! as HTMLInputElement).checked = true;
    (document.getElementById("JobTitle")! as HTMLInputElement).value = "";
    expect(validate("testForm")).toBe(false);
  });

  it("skips when truthy condition is not met", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "IsEmployed", fieldName: "IsEmployed", rules: [] }),
      field({ fieldId: "JobTitle", fieldName: "JobTitle", rules: [
        { rule: "required", message: "Job title required", when: { field: "IsEmployed", op: "truthy" } },
      ] }),
    ]));
    (document.getElementById("IsEmployed")! as HTMLInputElement).checked = false;
    (document.getElementById("JobTitle")! as HTMLInputElement).value = "";
    expect(validate("testForm")).toBe(true);
  });

  it("evaluates eq condition", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Name", fieldName: "Name", rules: [] }),
      field({ fieldId: "Email", fieldName: "Email", rules: [
        { rule: "required", message: "Email required for VIP", when: { field: "Name", op: "eq", value: "VIP" } },
      ] }),
    ]));
    (document.getElementById("Name")! as HTMLInputElement).value = "VIP";
    (document.getElementById("Email")! as HTMLInputElement).value = "";
    expect(validate("testForm")).toBe(false);
  });

  it("evaluates neq condition", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Name", fieldName: "Name", rules: [] }),
      field({ fieldId: "Email", fieldName: "Email", rules: [
        { rule: "required", message: "Email required", when: { field: "Name", op: "neq", value: "SKIP" } },
      ] }),
    ]));
    (document.getElementById("Name")! as HTMLInputElement).value = "SKIP";
    (document.getElementById("Email")! as HTMLInputElement).value = "";
    // When Name === "SKIP", neq("SKIP") is false → skip rule → valid
    expect(validate("testForm")).toBe(true);
  });

  it("evaluates falsy condition", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "IsEmployed", fieldName: "IsEmployed", rules: [] }),
      field({ fieldId: "JobTitle", fieldName: "JobTitle", rules: [
        { rule: "required", message: "Explain unemployment", when: { field: "IsEmployed", op: "falsy" } },
      ] }),
    ]));
    (document.getElementById("IsEmployed")! as HTMLInputElement).checked = false;
    (document.getElementById("JobTitle")! as HTMLInputElement).value = "";
    expect(validate("testForm")).toBe(false);
  });
});

// ── validate — first-fail-wins ────────────────────────────

describe("validate — first-fail-wins", () => {
  it("only shows first error per field", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Name", rules: [
        { rule: "required", message: "required" },
        { rule: "minLength", message: "too short", constraint: 3 },
      ] }),
    ]));
    (document.getElementById("Name")! as HTMLInputElement).value = "";
    validate("testForm");
    // Should show "required", not "too short"
    expect(document.getElementById("err_Name")!.textContent).toBe("required");
  });

  it("returns boolean", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "required", message: "r" }] }),
    ]));
    const result = validate("testForm");
    expect(typeof result).toBe("boolean");
    expect(result).toBe(false);
  });
});

// ── validate — fusion vendor ──────────────────────────────

describe("validate — fusion vendor", () => {
  it("reads value via readExpr", () => {
    const el = document.getElementById("FusionDrop")!;
    (el as any).ej2_instances = [{ value: "" }];

    register(makeDesc("testForm", [
      field({
        fieldId: "FusionDrop",
        vendor: "fusion",
        readExpr: "el.ej2_instances[0].value",
        rules: [{ rule: "required", message: "required" }],
      }),
    ]));
    expect(validate("testForm")).toBe(false);
  });
});

// ── validate — hidden fields ──────────────────────────────

describe("validate — hidden fields", () => {
  it("skips hidden fields", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "HiddenInput", rules: [{ rule: "required", message: "required" }] }),
    ]));
    // HiddenInput is inside display:none div, so offsetParent is null
    expect(validate("testForm")).toBe(true);
  });
});

// ── showFieldErrors ───────────────────────────────────────

describe("showFieldErrors", () => {
  it("shows errors from MVC format", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Name", fieldName: "Name", rules: [] }),
    ]));
    showFieldErrors("testForm", { errors: { Name: ["Name is required"] } });
    expect(document.getElementById("err_Name")!.textContent).toBe("Name is required");
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(true);
  });

  it("shows errors from flat format", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Email", fieldName: "Email", rules: [] }),
    ]));
    showFieldErrors("testForm", { Email: ["Invalid email"] });
    expect(document.getElementById("err_Email")!.textContent).toBe("Invalid email");
  });

  it("handles dotted nested property names", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Address_Street", fieldName: "Address.Street", rules: [] }),
    ]));
    showFieldErrors("testForm", { errors: { "Address.Street": ["Street is required"] } });
    expect(document.getElementById("err_Address_Street")!.textContent).toBe("Street is required");
  });
});

// ── clearErrors ───────────────────────────────────────────

describe("clearErrors", () => {
  it("removes error class and hides spans", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "required", message: "required" }] }),
    ]));
    validate("testForm"); // triggers error
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(true);

    clearErrors("testForm");
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(false);
    expect(document.getElementById("err_Name")!.style.display).toBe("none");
  });
});

// ── live clearing ─────────────────────────────────────────

describe("live clearing", () => {
  it("input event clears field error", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "required", message: "required" }] }),
    ]));
    validate("testForm");
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(true);

    // Simulate typing
    document.getElementById("Name")!.dispatchEvent(new Event("input"));
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(false);
  });

  it("change event clears field error", () => {
    register(makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "required", message: "required" }] }),
    ]));
    validate("testForm");
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(true);

    document.getElementById("Name")!.dispatchEvent(new Event("change"));
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(false);
  });
});
