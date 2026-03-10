# Validation & Gather Cleanup — Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Replace the badly-designed `forms.ts` module with a stateless `validation.ts` module that uses `data-valmsg-for` attribute selectors (rendered by `Html.Field()`) instead of `errorId`, removes all fallback patterns, and completes the missing JSON schema definitions for validation.

**Architecture:** The validation module becomes stateless — no `register()` step, no global `Map`. `validate()` receives the `ValidationDescriptor` directly from the HTTP pipeline. Error spans are found by `querySelector('span[data-valmsg-for="${fieldName}"]')` scoped to the form container. The `ValidationDescriptor` flows through `ExecContext` so downstream `validation-errors` commands can call `showServerErrors()` without global state.

**Tech Stack:** C# (.NET 9), TypeScript (ESM + esbuild), Vitest + jsdom, NUnit + Verify, Playwright, JSON Schema 2020-12

---

## Summary of Changes

| Layer | What changes | Why |
|-------|-------------|-----|
| C# `ValidationField` | Remove `ErrorId` property | Error spans found by `data-valmsg-for`, not by element ID |
| C# `ValidationDescriptor.WithPrefix()` | Remove `newErrorId` line | No errorId to remap |
| C# `FluentValidationAdapter` | Remove `errorId` from `FindOrCreateField()` | Adapter no longer generates errorId |
| JSON Schema | Add `ValidationDescriptor`, `ValidationField`, `ValidationRule`, `ValidationCondition`, `ValidationErrorsCommand`, `IntoCommand`; add `validation` to `RequestDescriptor` | Schema was incomplete — validation was absent |
| TS `types.ts` | Remove `errorId` from `ValidationField`; add `validationDesc?` to `ExecContext` | Match C# changes; flow descriptor through context |
| TS runtime | **DELETE** `forms.ts`, **CREATE** `validation.ts` | Stateless module, `data-valmsg-for`, no register/fallbacks |
| TS `http.ts` | Import from `validation.ts`; pass descriptor via context | Wire new module |
| TS `commands.ts` | Import from `validation.ts`; read descriptor from context | Wire new module |
| Sandbox view | Replace `<span id="err_xxx">` with `<span data-valmsg-for="fieldName">` | Match new lookup strategy |
| TS tests | Rewrite `when-validating-form-fields.test.ts` | Test new stateless API with `data-valmsg-for` |
| Playwright tests | Update assertions if error span selectors changed | Verify browser behavior still works |

## Anti-Patterns Being Eliminated

1. **`register()` + global `forms` Map** → Stateless `validate(desc)` that takes descriptor directly
2. **`errorId` field** → `data-valmsg-for` attribute selector scoped to form container
3. **Fallback in `evalCondition()`** (line 107-114 of forms.ts) → If field not in descriptor, condition passes (no DOM fallback)
4. **Fallback in `showServerFieldError()`** (line 202-226 of forms.ts) → Find error span by `data-valmsg-for` only
5. **Mixed "forms" naming** → Module is "validation", not "forms"

---

## Task 1: C# — Remove `errorId` from `ValidationField`

**Files:**
- Modify: `Alis.Reactive/Validation/ValidationField.cs`
- Modify: `Alis.Reactive/Validation/ValidationDescriptor.cs:33-40`
- Modify: `Alis.Reactive.FluentValidator/FluentValidationAdapter.cs`

**Step 1: Remove ErrorId from ValidationField**

Open `Alis.Reactive/Validation/ValidationField.cs`. Remove the `ErrorId` property and its constructor parameter:

```csharp
public sealed class ValidationField
{
    public string FieldId { get; }
    public string FieldName { get; }
    public string Vendor { get; }
    public string? ReadExpr { get; }
    public List<ValidationRule> Rules { get; }

    public ValidationField(
        string fieldId,
        string fieldName,
        string vendor,
        string? readExpr,
        List<ValidationRule> rules)
    {
        FieldId = fieldId;
        FieldName = fieldName;
        Vendor = vendor;
        ReadExpr = readExpr;
        Rules = rules;
    }
}
```

**Step 2: Update WithPrefix in ValidationDescriptor**

Open `Alis.Reactive/Validation/ValidationDescriptor.cs`. Remove the `newErrorId` line in `WithPrefix()`:

```csharp
public ValidationDescriptor WithPrefix(string formId, string prefix, params string[] fieldNames)
{
    var filtered = fieldNames.Length > 0
        ? Fields.Where(f => Array.IndexOf(fieldNames, f.FieldName) >= 0)
        : Fields;

    var remapped = new List<ValidationField>();
    foreach (var f in filtered)
    {
        var newFieldId = prefix + f.FieldId;
        remapped.Add(new ValidationField(
            newFieldId, f.FieldName,
            f.Vendor, f.ReadExpr, f.Rules));
    }

    return new ValidationDescriptor(formId, remapped);
}
```

**Step 3: Update FluentValidationAdapter**

Open `Alis.Reactive.FluentValidator/FluentValidationAdapter.cs`. Find `FindOrCreateField()` — remove the `errorId` parameter from the `ValidationField` constructor call. The adapter creates fields like:

```csharp
// Before:
new ValidationField(fieldId, fieldName, "err_" + fieldId, vendor, readExpr, new List<ValidationRule>())

// After:
new ValidationField(fieldId, fieldName, vendor, readExpr, new List<ValidationRule>())
```

Search for ALL places that construct `ValidationField` and remove the errorId argument.

**Step 4: Fix all compilation errors**

Run: `dotnet build`

Expected: All projects compile. Fix any remaining references to `ErrorId`.

**Step 5: Commit**

```bash
git add Alis.Reactive/Validation/ValidationField.cs Alis.Reactive/Validation/ValidationDescriptor.cs Alis.Reactive.FluentValidator/FluentValidationAdapter.cs
git commit -m "refactor: remove errorId from ValidationField — use data-valmsg-for instead"
```

---

## Task 2: JSON Schema — Add Validation Definitions

**Files:**
- Modify: `Alis.Reactive/Schemas/reactive-plan.schema.json`

**Step 1: Add validation types to `$defs`**

Add these definitions to the `$defs` section:

```json
"ValidationCondition": {
  "type": "object",
  "required": ["field", "op"],
  "additionalProperties": false,
  "properties": {
    "field": { "type": "string", "minLength": 1, "description": "Model property name to evaluate" },
    "op": { "type": "string", "enum": ["truthy", "falsy", "eq", "neq"] },
    "value": { "description": "Comparison value for eq/neq operators" }
  }
},
"ValidationRuleType": {
  "type": "string",
  "enum": ["required", "minLength", "maxLength", "email", "regex", "url", "range", "min", "max", "equalTo", "atLeastOne"]
},
"ValidationRule": {
  "type": "object",
  "required": ["rule", "message"],
  "additionalProperties": false,
  "properties": {
    "rule": { "$ref": "#/$defs/ValidationRuleType" },
    "message": { "type": "string", "minLength": 1 },
    "constraint": { "description": "Rule-specific constraint value" },
    "when": { "$ref": "#/$defs/ValidationCondition" }
  }
},
"ValidationField": {
  "type": "object",
  "required": ["fieldId", "fieldName", "vendor", "rules"],
  "additionalProperties": false,
  "properties": {
    "fieldId": { "type": "string", "minLength": 1, "description": "DOM element ID" },
    "fieldName": { "type": "string", "minLength": 1, "description": "Model property name (dot notation)" },
    "vendor": { "$ref": "#/$defs/Vendor" },
    "readExpr": { "type": "string", "minLength": 1, "description": "JS expression to read value. Variable: el" },
    "rules": {
      "type": "array",
      "items": { "$ref": "#/$defs/ValidationRule" }
    }
  }
},
"ValidationDescriptor": {
  "type": "object",
  "required": ["formId", "fields"],
  "additionalProperties": false,
  "properties": {
    "formId": { "type": "string", "minLength": 1, "description": "Form container element ID" },
    "fields": {
      "type": "array",
      "items": { "$ref": "#/$defs/ValidationField" }
    }
  }
}
```

**Step 2: Add `ValidationErrorsCommand` and `IntoCommand` to Command union**

Update the `Command` definition (currently only has Dispatch and MutateElement):

```json
"Command": {
  "oneOf": [
    { "$ref": "#/$defs/DispatchCommand" },
    { "$ref": "#/$defs/MutateElementCommand" },
    { "$ref": "#/$defs/ValidationErrorsCommand" },
    { "$ref": "#/$defs/IntoCommand" }
  ]
}
```

Add the command definitions:

```json
"ValidationErrorsCommand": {
  "type": "object",
  "required": ["kind", "formId"],
  "additionalProperties": false,
  "properties": {
    "kind": { "const": "validation-errors" },
    "formId": { "type": "string", "minLength": 1 },
    "when": { "$ref": "#/$defs/Guard" }
  }
},
"IntoCommand": {
  "type": "object",
  "required": ["kind", "target"],
  "additionalProperties": false,
  "properties": {
    "kind": { "const": "into" },
    "target": { "type": "string", "minLength": 1 },
    "when": { "$ref": "#/$defs/Guard" }
  }
}
```

**Step 3: Add `validation` to `RequestDescriptor`**

Add the `validation` property to the `RequestDescriptor` definition:

```json
"RequestDescriptor": {
  "type": "object",
  "required": ["verb", "url"],
  "additionalProperties": false,
  "properties": {
    "verb": { "type": "string", "enum": ["GET", "POST", "PUT", "DELETE"] },
    "url": { "type": "string", "minLength": 1 },
    "gather": { "type": "array", "items": { "$ref": "#/$defs/GatherItem" } },
    "whileLoading": { "type": "array", "items": { "$ref": "#/$defs/Command" } },
    "onSuccess": { "type": "array", "items": { "$ref": "#/$defs/StatusHandler" } },
    "onError": { "type": "array", "items": { "$ref": "#/$defs/StatusHandler" } },
    "chained": { "$ref": "#/$defs/RequestDescriptor" },
    "validation": { "$ref": "#/$defs/ValidationDescriptor" }
  }
}
```

**Step 4: Run schema tests**

Run: `dotnet test tests/Alis.Reactive.UnitTests`

Expected: Schema tests pass. If existing snapshot tests break due to errorId removal, update the `.verified.txt` files.

**Step 5: Commit**

```bash
git add Alis.Reactive/Schemas/reactive-plan.schema.json
git commit -m "schema: add ValidationDescriptor, ValidationField, ValidationRule, ValidationCondition, ValidationErrorsCommand, IntoCommand"
```

---

## Task 3: TS Types — Remove `errorId`, Add `validationDesc` to Context

**Files:**
- Modify: `Alis.Reactive.SandboxApp/Scripts/types.ts`

**Step 1: Remove `errorId` from `ValidationField` interface**

```typescript
export interface ValidationField {
  fieldId: string;
  fieldName: string;
  vendor: Vendor;
  readExpr?: string;
  rules: ValidationRule[];
}
```

**Step 2: Add `validationDesc` to `ExecContext`**

```typescript
export interface ExecContext {
  evt?: Record<string, unknown>;
  responseBody?: unknown;
  validationDesc?: ValidationDescriptor;
}
```

**Step 3: Run typecheck**

Run: `npx tsc --noEmit`

Expected: Type errors in `forms.ts` (references to `field.errorId`). These will be resolved when we delete `forms.ts` and create `validation.ts`.

**Step 4: Commit**

```bash
git add Alis.Reactive.SandboxApp/Scripts/types.ts
git commit -m "types: remove errorId from ValidationField, add validationDesc to ExecContext"
```

---

## Task 4: Sandbox View — Replace Error Span IDs with `data-valmsg-for`

**Files:**
- Modify: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Validation/Index.cshtml`
- Modify: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Validation/_AddressPartial.cshtml` (if exists)

**Step 1: Replace all `<span id="err_xxx">` with `<span data-valmsg-for="fieldName">`**

Every error span in the view uses `id="err_{fieldId}"`. Replace with `data-valmsg-for="{fieldName}"` where `fieldName` is the model property name (the `name` attribute value on the corresponding input).

**Section 1 (all-rules-form) — fields have no prefix, fieldName = fieldId:**
```html
<!-- Before -->
<span id="err_Name" class="text-xs text-red-600" hidden></span>
<!-- After -->
<span data-valmsg-for="Name" class="text-xs text-red-600" hidden></span>
```

Apply to ALL error spans in section 1: Name, Email, Age, Phone, Salary, Password.

**Section 2 (server-form) — fields have prefix `srv_`, fieldName = name attribute:**
```html
<!-- Input has name="Name" and id="srv_Name" -->
<!-- Before -->
<span id="err_srv_Name" class="text-xs text-red-600" hidden></span>
<!-- After -->
<span data-valmsg-for="Name" class="text-xs text-red-600" hidden></span>
```

Apply to: srv_Name → Name, srv_Email → Email.

**Section 3 (nested-form) — nested property names:**
```html
<!-- Input has name="Address.Street" and id="Address_Street" -->
<!-- Before -->
<span id="err_Address_Street" class="text-xs text-red-600" hidden></span>
<!-- After -->
<span data-valmsg-for="Address.Street" class="text-xs text-red-600" hidden></span>
```

Apply to: Address.Street, Address.City, Address.ZipCode.

**Section 4 (conditional-form) — no prefix:**
```html
<span data-valmsg-for="JobTitle" class="text-xs text-red-600" hidden></span>
```

**Section 5 (live-form) — prefix `live_`:**
```html
<!-- Input has name="Name" and id="live_Name" -->
<span data-valmsg-for="Name" class="text-xs text-red-600" hidden></span>
```

Apply to: live_Name → Name, live_Email → Email.

**Section 6 (combined-form) — prefix `cmb_`:**
Apply to: cmb_Name → Name, cmb_Email → Email, cmb_Age → Age, cmb_Phone → Phone.

**Section 7 (hidden-fields-form) — prefix `hf_`:**
Apply to: hf_Name → Name, hf_Phone → Phone, hf_Salary → Salary.

**Section 8 (db-form) — prefix `db_`:**
Apply to: db_Name → Name, db_Email → Email.

**Section 9 (partial-form):** Update the partial view file similarly.

**Key rule:** `data-valmsg-for` value = the model property name (the `name=""` attribute on the input, NOT the `id=""` attribute). This is what `FieldBuilder` generates and what the server returns in Problem Details error keys.

**Step 2: Verify no error span has an `id` attribute**

Search the file for `id="err_` — should find zero matches.

**Step 3: Commit**

```bash
git add Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Validation/
git commit -m "view: replace error span IDs with data-valmsg-for attributes"
```

---

## Task 5: Create `validation.ts` — Stateless Validator (TDD)

**Files:**
- Create: `Alis.Reactive.SandboxApp/Scripts/validation.ts`
- Rewrite: `Alis.Reactive.SandboxApp/Scripts/__tests__/when-validating-form-fields.test.ts`

### Step 1: Write the failing test file

Create the test file that imports from `validation.ts` (which doesn't exist yet). Use `data-valmsg-for` spans instead of `id="err_xxx"`:

```typescript
import { describe, it, expect, beforeEach } from "vitest";
import { JSDOM } from "jsdom";
import type { ValidationDescriptor, ValidationField } from "../types";

let validate: typeof import("../validation").validate;
let showServerErrors: typeof import("../validation").showServerErrors;
let clearAll: typeof import("../validation").clearAll;
let wireLiveClearing: typeof import("../validation").wireLiveClearing;

function field(overrides: Partial<ValidationField> & { fieldId: string }): ValidationField {
  return {
    fieldName: overrides.fieldName ?? overrides.fieldId,
    vendor: overrides.vendor ?? "native",
    rules: overrides.rules ?? [],
    ...overrides,
  };
}

function makeDesc(formId: string, fields: ValidationField[]): ValidationDescriptor {
  return { formId, fields };
}

// Helper: build error span with data-valmsg-for (not id)
function errSpan(fieldName: string): string {
  return `<span data-valmsg-for="${fieldName}" style="display:none" hidden></span>`;
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

      <input id="Age" name="Age" type="number" value="" />
      ${errSpan("Age")}

      <input id="Website" name="Website" value="" />
      ${errSpan("Website")}

      <input id="Salary" name="Salary" type="number" value="" />
      ${errSpan("Salary")}

      <input id="Password" name="Password" type="password" value="" />
      ${errSpan("Password")}

      <input id="ConfirmPassword" name="ConfirmPassword" type="password" value="" />
      ${errSpan("ConfirmPassword")}

      <input id="Tags" name="Tags" value="" />
      ${errSpan("Tags")}

      <input id="IsEmployed" name="IsEmployed" type="checkbox" />

      <input id="JobTitle" name="JobTitle" value="" />
      ${errSpan("JobTitle")}

      <input id="Address_Street" name="Address.Street" value="" />
      ${errSpan("Address.Street")}

      <input id="Address_City" name="Address.City" value="" />
      ${errSpan("Address.City")}

      <div id="FusionDrop"></div>
      ${errSpan("FusionDrop")}

      <div id="hiddenField" style="display:none">
        <input id="HiddenInput" name="HiddenInput" value="" />
        ${errSpan("HiddenInput")}
      </div>
    </form>
  </body></html>`);

  (globalThis as any).document = dom.window.document;
  (globalThis as any).HTMLElement = dom.window.HTMLElement;
  (globalThis as any).Event = dom.window.Event;

  const mod = await import("../validation");
  validate = mod.validate;
  showServerErrors = mod.showServerErrors;
  clearAll = mod.clearAll;
  wireLiveClearing = mod.wireLiveClearing;
});

// Helper to find error span by data-valmsg-for
function errorSpan(fieldName: string): HTMLSpanElement | null {
  return document.querySelector(`span[data-valmsg-for="${fieldName}"]`);
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

  it("handles invalid regex gracefully", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Phone", rules: [{ rule: "regex", message: "bad", constraint: "[invalid" }] }),
    ]);
    (document.getElementById("Phone")! as HTMLInputElement).value = "test";
    expect(validate(desc)).toBe(true);
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
      field({ fieldId: "IsEmployed", fieldName: "IsEmployed", rules: [] }),
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
      field({ fieldId: "IsEmployed", fieldName: "IsEmployed", rules: [] }),
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
    expect(validate(desc)).toBe(true); // neq("SKIP") false → skip rule
  });

  it("evaluates falsy condition", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "IsEmployed", fieldName: "IsEmployed", rules: [] }),
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
  it("reads value via readExpr", () => {
    (document.getElementById("FusionDrop")! as any).ej2_instances = [{ value: "" }];
    const desc = makeDesc("testForm", [
      field({
        fieldId: "FusionDrop",
        fieldName: "FusionDrop",
        vendor: "fusion",
        readExpr: "el.ej2_instances[0].value",
        rules: [{ rule: "required", message: "required" }],
      }),
    ]);
    expect(validate(desc)).toBe(false);
  });
});

// ── validate — hidden fields ──────────────────────────────

describe("validate — hidden fields", () => {
  it("skips hidden fields", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "HiddenInput", rules: [{ rule: "required", message: "required" }] }),
    ]);
    expect(validate(desc)).toBe(true);
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

  it("shows errors from flat format", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Email", fieldName: "Email", rules: [] }),
    ]);
    showServerErrors(desc, { Email: ["Invalid email"] });
    expect(errorSpan("Email")!.textContent).toBe("Invalid email");
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
    expect(errorSpan("Name")!.style.display).toBe("none");
  });
});

// ── live clearing ─────────────────────────────────────────

describe("live clearing", () => {
  it("input event clears field error", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "required", message: "required" }] }),
    ]);
    wireLiveClearing(desc);
    validate(desc);
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(true);

    document.getElementById("Name")!.dispatchEvent(new Event("input", { bubbles: true }));
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(false);
  });

  it("change event clears field error", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "required", message: "required" }] }),
    ]);
    wireLiveClearing(desc);
    validate(desc);
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(true);

    document.getElementById("Name")!.dispatchEvent(new Event("change", { bubbles: true }));
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(false);
  });

  it("does not wire duplicate listeners", () => {
    const desc = makeDesc("testForm", [
      field({ fieldId: "Name", rules: [{ rule: "required", message: "required" }] }),
    ]);
    wireLiveClearing(desc);
    wireLiveClearing(desc); // second call — should be a no-op
    validate(desc);

    document.getElementById("Name")!.dispatchEvent(new Event("input", { bubbles: true }));
    expect(document.getElementById("Name")!.classList.contains("alis-has-error")).toBe(false);
  });
});
```

### Step 2: Run tests to verify they fail

Run: `npm test`

Expected: All tests FAIL because `../validation` module does not exist.

### Step 3: Create `validation.ts` — full implementation

Create `Alis.Reactive.SandboxApp/Scripts/validation.ts`:

```typescript
// Validation — Stateless client-side validation engine + server error display
//
// No registration, no global state. Each function receives the ValidationDescriptor.
// Error spans found by: querySelector('span[data-valmsg-for="${fieldName}"]')
// scoped to the form container. 11 rule types, conditional rules, first-failing-rule-wins.

import type {
  ValidationDescriptor,
  ValidationField,
  ValidationRule,
  ValidationCondition,
} from "./types";
import { scope } from "./trace";

const log = scope("validation");
const ERR_CLASS = "alis-has-error";

// ── Public API ──────────────────────────────────────────────

export function validate(desc: ValidationDescriptor): boolean {
  clearAll(desc);
  const byName = buildByName(desc);
  let valid = true;

  for (const f of desc.fields) {
    const el = document.getElementById(f.fieldId);
    if (!el || isHidden(el)) continue;

    const value = readValue(f);
    for (const rule of f.rules) {
      if (rule.when && !evalCondition(rule.when, byName)) continue;
      if (ruleFails(rule, value, byName)) {
        showError(desc.formId, f, rule.message);
        valid = false;
        break; // first failing rule wins
      }
    }
  }

  log.debug("validate", { formId: desc.formId, valid });
  return valid;
}

export function showServerErrors(desc: ValidationDescriptor, data: unknown): void {
  clearAll(desc);
  const byName = buildByName(desc);
  const errors = extractErrors(data);
  if (!errors) return;

  for (const [name, msgs] of Object.entries(errors)) {
    const msg = Array.isArray(msgs) ? msgs.join(", ") : String(msgs);

    // Error span found by data-valmsg-for — always works (FieldBuilder renders it)
    const span = findErrorSpan(desc.formId, name);
    if (span) {
      span.textContent = msg;
      span.removeAttribute("hidden");
      span.style.display = "";
    }

    // Error class on field element — only if field is in descriptor
    const field = byName.get(name);
    if (field) {
      const el = document.getElementById(field.fieldId);
      if (el) el.classList.add(ERR_CLASS);
    }
  }

  log.debug("showServerErrors", { formId: desc.formId, fieldCount: Object.keys(errors).length });
}

export function clearAll(desc: ValidationDescriptor): void {
  for (const f of desc.fields) {
    clearFieldError(desc.formId, f);
  }
}

export function wireLiveClearing(desc: ValidationDescriptor): void {
  const container = document.getElementById(desc.formId);
  if (!container || container.dataset.alisValidated) return;
  container.dataset.alisValidated = "true";

  const handler = (e: Event) => {
    const target = e.target as HTMLElement;
    const field = desc.fields.find(f => f.fieldId === target.id);
    if (field) clearFieldError(desc.formId, field);
  };

  container.addEventListener("input", handler);
  container.addEventListener("change", handler);
}

// ── Internal ────────────────────────────────────────────────

function buildByName(desc: ValidationDescriptor): Map<string, ValidationField> {
  const map = new Map<string, ValidationField>();
  for (const f of desc.fields) map.set(f.fieldName, f);
  return map;
}

function findErrorSpan(formId: string, fieldName: string): HTMLElement | null {
  const container = document.getElementById(formId);
  if (!container) return null;
  return container.querySelector(`span[data-valmsg-for="${fieldName}"]`);
}

function showError(formId: string, field: ValidationField, message: string): void {
  const el = document.getElementById(field.fieldId);
  if (el) el.classList.add(ERR_CLASS);

  const span = findErrorSpan(formId, field.fieldName);
  if (span) {
    span.textContent = message;
    span.removeAttribute("hidden");
    span.style.display = "";
  }
}

function clearFieldError(formId: string, field: ValidationField): void {
  const span = findErrorSpan(formId, field.fieldName);
  if (span) {
    span.textContent = "";
    span.setAttribute("hidden", "");
    span.style.display = "none";
  }
  const el = document.getElementById(field.fieldId);
  if (el) el.classList.remove(ERR_CLASS);
}

function readValue(field: ValidationField): unknown {
  const el = document.getElementById(field.fieldId);
  if (!el) return null;
  if (field.vendor === "fusion" && field.readExpr) {
    try { return new Function("el", `return ${field.readExpr}`)(el); }
    catch { return null; }
  }
  if ((el as HTMLInputElement).type === "checkbox") return (el as HTMLInputElement).checked;
  return (el as HTMLInputElement).value;
}

function evalCondition(cond: ValidationCondition, byName: Map<string, ValidationField>): boolean {
  const srcField = byName.get(cond.field);
  if (!srcField) return true; // field not in descriptor → condition passes
  const val = readValue(srcField);
  const str = val == null ? "" : String(val);
  const empty = val == null || str === "" || val === false;
  switch (cond.op) {
    case "truthy": return !empty;
    case "falsy": return empty;
    case "eq": return str === String(cond.value ?? "");
    case "neq": return str !== String(cond.value ?? "");
    default: return true;
  }
}

function ruleFails(rule: ValidationRule, value: unknown, byName: Map<string, ValidationField>): boolean {
  const str = value == null ? "" : String(value);
  const empty = value == null || str === "" || value === false;

  switch (rule.rule) {
    case "required":
      return empty;
    case "minLength":
      return !empty && str.length < Number(rule.constraint);
    case "maxLength":
      return !empty && str.length > Number(rule.constraint);
    case "email":
      return !empty && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(str);
    case "regex": {
      try { return !empty && !new RegExp(String(rule.constraint)).test(str); }
      catch {
        log.warn("invalid validation regex", { constraint: rule.constraint });
        return false;
      }
    }
    case "url":
      return !empty && !/^https?:\/\/.+/.test(str);
    case "min":
      return !empty && Number(str) < Number(rule.constraint);
    case "max":
      return !empty && Number(str) > Number(rule.constraint);
    case "range": {
      const [lo, hi] = rule.constraint as [number, number];
      const n = Number(str);
      return !empty && (n < lo || n > hi);
    }
    case "equalTo": {
      const other = byName.get(String(rule.constraint));
      if (!other) return false;
      return String(value ?? "") !== String(readValue(other) ?? "");
    }
    case "atLeastOne":
      return Array.isArray(value) ? value.length === 0 : empty;
    default:
      return false;
  }
}

function isHidden(el: HTMLElement): boolean {
  let node: HTMLElement | null = el;
  while (node) {
    if (node.style?.display === "none") return true;
    node = node.parentElement;
  }
  return false;
}

function extractErrors(data: unknown): Record<string, unknown> | null {
  if (!data || typeof data !== "object") return null;
  if ("errors" in data && typeof (data as Record<string, unknown>).errors === "object") {
    return (data as Record<string, Record<string, unknown>>).errors;
  }
  return data as Record<string, unknown>;
}
```

### Step 4: Run tests to verify they pass

Run: `npm test`

Expected: ALL tests pass.

### Step 5: Commit

```bash
git add Alis.Reactive.SandboxApp/Scripts/validation.ts Alis.Reactive.SandboxApp/Scripts/__tests__/when-validating-form-fields.test.ts
git commit -m "feat: stateless validation.ts module — data-valmsg-for, no register, no fallbacks"
```

---

## Task 6: Wire `validation.ts` into `http.ts` and `commands.ts`, Delete `forms.ts`

**Files:**
- Modify: `Alis.Reactive.SandboxApp/Scripts/http.ts`
- Modify: `Alis.Reactive.SandboxApp/Scripts/commands.ts`
- Delete: `Alis.Reactive.SandboxApp/Scripts/forms.ts`

### Step 1: Update `http.ts`

Replace `forms` imports with `validation` imports. Change the validation flow to be stateless:

```typescript
import type { RequestDescriptor, StatusHandler, ExecContext } from "./types";
import { resolveGather } from "./gather";
import { executeCommands } from "./commands";
import { validate, wireLiveClearing } from "./validation";
import { scope } from "./trace";

const log = scope("http");

export async function execRequest(req: RequestDescriptor, ctx?: ExecContext): Promise<void> {
  // 0. Pre-request validation — validate directly (no register), abort if fails
  if (req.validation) {
    wireLiveClearing(req.validation);
    if (!validate(req.validation)) {
      log.debug("validation failed, aborting request");
      return;
    }
  }

  // 1. WhileLoading
  if (req.whileLoading) {
    executeCommands(req.whileLoading, ctx);
  }

  // 2. Gather
  const gatherResult = resolveGather(req.gather ?? [], req.verb);

  // 3. Build fetch options
  let url = req.url;
  const init: RequestInit = { method: req.verb };

  if (gatherResult.urlParams.length > 0) {
    const sep = url.includes("?") ? "&" : "?";
    url = url + sep + gatherResult.urlParams.join("&");
  }

  if (req.verb !== "GET" && Object.keys(gatherResult.body).length > 0) {
    init.headers = { "Content-Type": "application/json" };
    init.body = JSON.stringify(gatherResult.body);
  }

  log.debug("fetch", { verb: req.verb, url });

  try {
    const response = await fetch(url, init);

    if (response.ok) {
      let successBody: unknown;
      try { successBody = await response.text(); } catch { /* no body */ }
      const successCtx = successBody != null ? { ...ctx, responseBody: successBody } : ctx;
      routeHandlers(req.onSuccess, response.status, successCtx);
    } else {
      let errorBody: unknown;
      try { errorBody = await response.json(); } catch { /* no JSON body */ }
      // Thread validationDesc through context for validation-errors command
      const errorCtx: ExecContext = {
        ...ctx,
        responseBody: errorBody ?? undefined,
        validationDesc: req.validation,
      };
      routeHandlers(req.onError, response.status, errorCtx);
    }

    if (req.chained && response.ok) {
      await execRequest(req.chained, ctx);
    }
  } catch (err) {
    log.error("network error", { url, error: String(err) });
    routeHandlers(req.onError, 0, ctx);
  }
}

function routeHandlers(handlers: StatusHandler[] | undefined, status: number, ctx?: ExecContext): void {
  if (!handlers || handlers.length === 0) return;
  for (const h of handlers) {
    if (h.statusCode != null && h.statusCode === status) {
      executeCommands(h.commands, ctx);
      return;
    }
  }
  for (const h of handlers) {
    if (h.statusCode == null) {
      executeCommands(h.commands, ctx);
      return;
    }
  }
}
```

### Step 2: Update `commands.ts`

Replace `forms` import with `validation` import. Use `ctx.validationDesc` for server errors:

```typescript
import type { Command, ExecContext } from "./types";
import { showServerErrors } from "./validation";
import { scope } from "./trace";

const log = scope("commands");

export function executeCommands(commands: Command[], ctx?: ExecContext): void {
  for (const cmd of commands) {
    switch (cmd.kind) {
      case "dispatch":
        log.trace("dispatch", { event: cmd.event });
        document.dispatchEvent(new CustomEvent(cmd.event, { detail: cmd.payload ?? {} }));
        break;
      case "mutate-element": {
        const el = document.getElementById(cmd.target);
        if (!el) break;
        const val = cmd.value;
        new Function("el", "val", cmd.jsEmit)(el, val);
        break;
      }
      case "validation-errors": {
        if (ctx?.responseBody && ctx?.validationDesc) {
          showServerErrors(ctx.validationDesc, ctx.responseBody);
        }
        break;
      }
      case "into": {
        const container = document.getElementById(cmd.target);
        if (container && ctx?.responseBody != null) {
          container.innerHTML = String(ctx.responseBody);
        }
        break;
      }
    }
  }
}
```

### Step 3: Delete `forms.ts`

```bash
rm Alis.Reactive.SandboxApp/Scripts/forms.ts
```

### Step 4: Verify build

Run: `npm run build`

Expected: Bundle compiles. No references to `forms.ts` remain.

### Step 5: Run TS tests

Run: `npm test`

Expected: All tests pass.

### Step 6: Commit

```bash
git add Alis.Reactive.SandboxApp/Scripts/http.ts Alis.Reactive.SandboxApp/Scripts/commands.ts
git rm Alis.Reactive.SandboxApp/Scripts/forms.ts
git commit -m "refactor: wire validation.ts into http + commands, delete forms.ts"
```

---

## Task 7: Update C# Unit Test Snapshots

**Files:**
- Modify: `tests/Alis.Reactive.UnitTests/` — update `.verified.txt` files that contain `errorId`

### Step 1: Run C# unit tests to see which snapshots fail

Run: `dotnet test tests/Alis.Reactive.UnitTests`

Expected: Some snapshot tests fail because `errorId` no longer appears in rendered plan JSON.

### Step 2: Accept new snapshots

For each failing snapshot test, review the new `.received.txt` and confirm `errorId` is correctly absent. Then rename `.received.txt` to `.verified.txt`.

### Step 3: Run C# unit tests again

Run: `dotnet test tests/Alis.Reactive.UnitTests`

Expected: ALL tests pass.

### Step 4: Commit

```bash
git add tests/Alis.Reactive.UnitTests/
git commit -m "test: update C# verified snapshots after errorId removal"
```

---

## Task 8: Full Build + All Three Test Layers

### Step 1: Build everything

```bash
npm run build:all
dotnet build
```

Expected: Both compile cleanly.

### Step 2: Run TS unit tests

Run: `npm test`

Expected: ALL pass.

### Step 3: Run C# unit + schema tests

Run: `dotnet test tests/Alis.Reactive.UnitTests`

Expected: ALL pass.

### Step 4: Run Playwright browser tests

Run: `dotnet test tests/Alis.Reactive.PlaywrightTests`

Expected: Some tests may fail if they assert on error span IDs (e.g., looking for `#err_Name`). Fix by updating Playwright selectors to use `[data-valmsg-for="Name"]` instead.

### Step 5: Fix Playwright tests if needed

Open `tests/Alis.Reactive.PlaywrightTests/Validation/WhenValidatingFormFields.cs`.

Replace any selectors that use `#err_xxx` with `[data-valmsg-for="fieldName"]`:

```csharp
// Before:
await page.Locator("#err_Name").WaitForAsync();

// After:
await page.Locator("[data-valmsg-for='Name']").WaitForAsync();
```

### Step 6: Re-run Playwright tests

Run: `dotnet test tests/Alis.Reactive.PlaywrightTests`

Expected: ALL pass.

### Step 7: Commit

```bash
git add tests/Alis.Reactive.PlaywrightTests/
git commit -m "test: update Playwright selectors from errorId to data-valmsg-for"
```

---

## Task 9: Final Verification + Clean Commit

### Step 1: Verify no references to `forms.ts` remain

Search entire codebase for `forms.ts`, `forms.`, `register(`, `from "./forms"`, `errorId`:

```bash
grep -r "forms\.ts\|from.*./forms\|errorId\|err_" --include="*.ts" --include="*.cs" --include="*.cshtml" Alis.Reactive.SandboxApp/ Alis.Reactive/ tests/
```

Expected: Zero matches (except possibly comments or unrelated strings).

### Step 2: Run ALL tests one final time

```bash
npm test
dotnet test tests/Alis.Reactive.UnitTests
dotnet test tests/Alis.Reactive.PlaywrightTests
```

Expected: ALL green across all three layers.

### Step 3: Final commit (if any fixes)

```bash
git add -A
git commit -m "chore: final cleanup — validation module complete"
```

---

## Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| **Stateless API** (no `register()`, no global `Map`) | `validate(desc)` takes descriptor directly from the HTTP pipeline. No separate registration step. Simpler, testable, no state leaks. |
| **`data-valmsg-for` over `errorId`** | FieldBuilder already renders `data-valmsg-for`. Error spans found by `querySelector` scoped to form container. Eliminates ID conventions and fallbacks entirely. |
| **Descriptor flows through `ExecContext`** | `validationDesc` is added to `ExecContext` so the `validation-errors` command can call `showServerErrors()` without global state. |
| **Event delegation for live clearing** | One `input`/`change` listener on the form container (via `wireLiveClearing`), not per-field. Uses `data-alisValidated` attribute to prevent duplicate wiring. |
| **No fallback in `evalCondition()`** | If the condition references a field not in the descriptor, the condition passes (returns `true`). No DOM fallback, no guessing. |
| **Server errors: `data-valmsg-for` is the contract** | Server returns property names in Problem Details. Error spans have matching `data-valmsg-for` values. Direct match — no ID convention conversion needed. |
