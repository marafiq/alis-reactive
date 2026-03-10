# Validation Cleanup v2 — Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Replace `forms.ts` with a stateless `validation.ts`, use `Html.Field()` everywhere (with prefix support), complete the SOLID loop across all three test layers.

**Architecture:** FluentValidationAdapter extracts rules → `.Validate<TValidator>()` injects them into the plan → JS `validation.ts` executes rules client-side (stateless, no register, no fallbacks) → error spans found by `data-valmsg-for` (rendered by `Html.Field()`). Server 400 ProblemDetails errors display at fields using the same `data-valmsg-for` contract. Validation rules (11 types + ValidationCondition) are completely separate from the Conditions/Guards module.

**Tech Stack:** C# (.NET 9), TypeScript (ESM + esbuild), Vitest + jsdom, NUnit + Verify + JsonSchema.Net, Playwright

---

## Status of Prior Tasks

Tasks 1-3 from the original plan are **DONE** in this worktree:
- **Task 1** ✅ C# — Removed `errorId` from `ValidationField`, `ValidationDescriptor.WithPrefix()`, `FluentValidationAdapter`
- **Task 2** ✅ JSON Schema — Added `ValidationDescriptor`, `ValidationField`, `ValidationRule`, `ValidationCondition`, `ValidationErrorsCommand`, `IntoCommand` to schema
- **Task 3** ✅ TS types — Removed `errorId` from `ValidationField`, added `validationDesc` to `ExecContext`

**This plan starts at Task 4.**

---

## Task 4: Add Prefix-Aware `Html.Field()` Overload

**Files:**
- Modify: `Alis.Reactive.Native/Extensions/FieldExtensions.cs`
- Modify: `tests/Alis.Reactive.Native.UnitTests/Field/WhenRenderingAField.cs`

**Why:** Sections 2, 5, 6, 7, 8 of the validation view use prefixed element IDs (`srv_Name`, `live_Name`, etc.) to avoid DOM collisions. The existing `Html.Field()` derives the element ID from the expression (`IdFor`), which is always `"Name"`. A prefix-aware overload lets the caller pass `"srv_"` and receive the computed ID in the callback.

### Step 1: Write the failing test

Add to `tests/Alis.Reactive.Native.UnitTests/Field/WhenRenderingAField.cs`:

```csharp
[Test]
public void Renders_prefixed_for_attribute()
{
    var writer = new StringWriter();
    var b = new FieldBuilder(writer, "Name").Label("Name").ForId("srv_Name");
    b.Required();
    using (b.Begin()) { writer.Write("<input id=\"srv_Name\" />"); }
    var output = writer.ToString();
    Assert.That(output, Does.Contain("for=\"srv_Name\""));
    Assert.That(output, Does.Contain("data-valmsg-for=\"Name\""));
}
```

### Step 2: Run test to verify it passes (already works — FieldBuilder already supports separate ForId and name)

Run: `dotnet test tests/Alis.Reactive.Native.UnitTests`

Expected: PASS — FieldBuilder already separates `ForId` (label's `for`) from `_name` (`data-valmsg-for`).

### Step 3: Add the prefix-aware overload to FieldExtensions

Open `Alis.Reactive.Native/Extensions/FieldExtensions.cs`. Add a second `Field()` overload below the existing one:

```csharp
/// <summary>
/// Renders a model-bound form field with a prefix applied to the element ID.
/// The label's <c>for</c> attribute and the callback's <c>id</c> parameter
/// use the prefixed ID, while <c>data-valmsg-for</c> uses the unprefixed model name.
/// </summary>
public static void Field<TModel, TProp>(
    this IHtmlHelper<TModel> html,
    string label, bool isRequired,
    Expression<Func<TModel, TProp>> expression,
    string idPrefix,
    Func<Expression<Func<TModel, TProp>>, string, IHtmlContent> inputBuilder)
{
    var writer = html.ViewContext.Writer;
    var name = html.NameFor(expression).ToString();
    var id = idPrefix + html.IdFor(expression);
    var b = new FieldBuilder(writer, name)
        .Label(label)
        .ForId(id);
    if (isRequired) b.Required();
    using (b.Begin()) { inputBuilder(expression, id).WriteTo(writer, HtmlEncoder.Default); }
}
```

### Step 4: Write the integration test for the prefix overload

Add to `WhenRenderingAField.cs`:

```csharp
[Test]
public void Prefix_overload_generates_prefixed_for_and_unprefixed_valmsg()
{
    // This test verifies the FieldExtensions.Field() prefix overload behavior
    // by using FieldBuilder directly (FieldExtensions needs IHtmlHelper which is hard to mock)
    var writer = new StringWriter();
    var name = "Name";          // NameFor result
    var id = "srv_" + "Name";   // prefix + IdFor result
    var b = new FieldBuilder(writer, name).Label("Name").ForId(id);
    b.Required();
    using (b.Begin()) { writer.Write($"<input id=\"{id}\" name=\"{name}\" />"); }
    var output = writer.ToString();

    // Label for should point to prefixed id
    Assert.That(output, Does.Contain("for=\"srv_Name\""));
    // Validation span should use unprefixed model name
    Assert.That(output, Does.Contain("data-valmsg-for=\"Name\""));
    // Input should have prefixed id
    Assert.That(output, Does.Contain("id=\"srv_Name\""));
}
```

### Step 5: Run tests

Run: `dotnet test tests/Alis.Reactive.Native.UnitTests`

Expected: ALL pass.

### Step 6: Commit

```bash
git add Alis.Reactive.Native/Extensions/FieldExtensions.cs tests/Alis.Reactive.Native.UnitTests/Field/WhenRenderingAField.cs
git commit -m "feat: add prefix-aware Html.Field() overload for prefixed form sections"
```

---

## Task 5: Rewrite Sandbox View Using `Html.Field()`

**Files:**
- Modify: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Validation/Index.cshtml`
- Modify: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Validation/_AddressPartial.cshtml`

**Why:** Replace raw `<div>/<label>/<input>/<span id="err_xxx">` blocks with `Html.Field()` calls. This uses the FieldBuilder abstraction that renders `data-valmsg-for` on the error span automatically.

### Step 1: Add required using to Index.cshtml

Add at the top of the file (if not already present):

```csharp
@using Microsoft.AspNetCore.Mvc.Rendering
```

### Step 2: Rewrite Section 1 (all-rules-form, no prefix)

Replace the 6 raw field blocks with `Html.Field()` calls. Example for Name:

**Before:**
```html
<div class="flex flex-col gap-1.5">
    <label for="Name" class="text-xs font-medium text-text-secondary">Name *</label>
    <input type="text" id="Name" name="Name"
           class="rounded-md border border-border px-3 py-1.5 text-sm" placeholder="Full name" />
    <span id="err_Name" class="text-xs text-red-600" hidden></span>
</div>
```

**After:**
```csharp
@{ Html.Field("Name", true, m => m.Name, expr =>
    Html.TextBoxFor(expr, new { @class = "rounded-md border border-border px-3 py-1.5 text-sm", placeholder = "Full name" })
); }
```

Apply to all 6 fields in Section 1:
- `Name` — `Html.TextBoxFor(expr, new { @class = "...", placeholder = "Full name" })`
- `Email` — `Html.TextBoxFor(expr, new { @class = "...", placeholder = "you@@example.com" })`
- `Age` — `Html.TextBoxFor(expr, new { type = "number", @class = "...", placeholder = "0\u2013120" })`
- `Phone` — `Html.TextBoxFor(expr, new { @class = "...", placeholder = "123-456-7890" })`
- `Salary` — `Html.TextBoxFor(expr, new { type = "number", @class = "...", placeholder = "0\u2013500,000" })`
- `Password` — `Html.PasswordFor(expr, new { @class = "...", placeholder = "Min 8 characters" })`

### Step 3: Rewrite Section 2 (server-form, `srv_` prefix)

Use the prefix overload. Example:

```csharp
@{ Html.Field("Name", true, m => m.Name, "srv_", (expr, id) =>
    Html.TextBoxFor(expr, new { id, @class = "rounded-md border border-border px-3 py-1.5 text-sm", placeholder = "Leave empty to trigger error" })
); }
```

Apply to: `Name`, `Email`.

### Step 4: Rewrite Section 3 (nested-form, no prefix, nested properties)

```csharp
@{ Html.Field("Street", true, m => m.Address!.Street, expr =>
    Html.TextBoxFor(expr, new { @class = "rounded-md border border-border px-3 py-1.5 text-sm", placeholder = "Street address" })
); }
```

`Html.IdFor(m => m.Address!.Street)` = `"Address_Street"`, `Html.NameFor` = `"Address.Street"`.
So: `for="Address_Street"`, `id="Address_Street"`, `data-valmsg-for="Address.Street"`. All correct.

Apply to: `Address.Street`, `Address.City`, `Address.ZipCode`.

### Step 5: Rewrite Section 4 (conditional-form)

- `IsEmployed` checkbox: Keep as raw HTML (inline flex layout, not a stacked field).
- `JobTitle`: Use `Html.Field()` with no prefix.

```csharp
@{ Html.Field("Job Title", false, m => m.JobTitle, expr =>
    Html.TextBoxFor(expr, new { @class = "rounded-md border border-border px-3 py-1.5 text-sm", placeholder = "Required when employed" })
); }
```

### Step 6: Rewrite Section 5 (live-form, `live_` prefix)

```csharp
@{ Html.Field("Name", true, m => m.Name, "live_", (expr, id) =>
    Html.TextBoxFor(expr, new { id, @class = "rounded-md border border-border px-3 py-1.5 text-sm", placeholder = "Type after error to clear" })
); }
```

Apply to: `Name`, `Email`.

### Step 7: Rewrite Section 6 (combined-form, `cmb_` prefix)

Apply prefix overload to: `Name`, `Email`, `Age` (type=number), `Phone`.

### Step 8: Rewrite Section 7 (hidden-fields-form, `hf_` prefix)

- `Name`: prefix overload with `"hf_"` — always visible.
- `Phone`, `Salary`: prefix overload with `"hf_"` — inside `div#hf_extras` (hidden by default). The `Html.Field()` div goes inside the extras div.
- `hf_toggle` checkbox: Keep as-is (NativeCheckBox component, inline layout).

### Step 9: Rewrite Section 8 (db-form, `db_` prefix)

Apply prefix overload to: `Name`, `Email`.

### Step 10: Update `_AddressPartial.cshtml`

This partial is loaded dynamically via AJAX and has no `@model`. Replace `id="err_xxx"` spans with `data-valmsg-for` attributes:

```html
<!-- Before -->
<span id="err_partial_Address_Street" class="text-xs text-red-600" hidden></span>
<!-- After -->
<span data-valmsg-for="Address.Street" class="text-xs text-red-600" hidden></span>
```

Apply to: `Address.Street`, `Address.City`, `Address.ZipCode`.

### Step 11: Verify no `id="err_"` remains

Search the view files for `id="err_` — should find zero matches.

### Step 12: Build to verify compilation

Run: `dotnet build`

Expected: Compiles cleanly.

### Step 13: Commit

```bash
git add Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Validation/
git commit -m "view: rewrite validation showcase to use Html.Field() with prefix support"
```

---

## Task 6: Create `validation.ts` — Stateless Validator (TDD)

**Files:**
- Create: `Alis.Reactive.SandboxApp/Scripts/validation.ts`
- Rewrite: `Alis.Reactive.SandboxApp/Scripts/__tests__/when-validating-form-fields.test.ts`

**Design principles (from architecture prompt — saved in memory/validation-architecture-prompt.md):**
- **Stateless**: No `register()`, no global `Map`. `validate(desc)` takes descriptor directly.
- **Vendor-aware**: Native (`.value`, `.checked`) vs Fusion (`readExpr` via `new Function("el", readExpr)`)
- **Error spans by `data-valmsg-for`**: `querySelector('span[data-valmsg-for="${fieldName}"]')` scoped to container
- **No fallbacks**: If field not in descriptor, condition passes. No DOM-guessing.
- **First-failing-rule-wins**: Stop at first failure per field
- **11 rule types**: required, minLength, maxLength, email, regex, url, range, min, max, equalTo, atLeastOne
- **ValidationCondition**: 4 ops (truthy, falsy, eq, neq) — NOT pipeline Guards
- **Container, not form**: Works with any container ID (future-proof)
- **Event delegation for live clearing**: One listener on container, not per-field
- **Interactive**: Clear server error on user interaction/blur

### Step 1: Write the failing test file

Create `Alis.Reactive.SandboxApp/Scripts/__tests__/when-validating-form-fields.test.ts`:

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

// Helper: build error span with data-valmsg-for (matches Html.Field() output)
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

// Helper to find error span by data-valmsg-for (scoped to form)
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

Expected: ALL tests FAIL because `../validation` module does not exist.

### Step 3: Create `validation.ts`

Create `Alis.Reactive.SandboxApp/Scripts/validation.ts`:

```typescript
// Validation — Stateless client-side validation engine + server error display
//
// No registration, no global state. Each function receives the ValidationDescriptor.
// Error spans found by: querySelector('span[data-valmsg-for="${fieldName}"]')
// scoped to the container element. 11 rule types, conditional rules, first-failing-rule-wins.
//
// Validation rules (11 types + ValidationCondition with truthy/falsy/eq/neq)
// are completely separate from the Conditions/Guards pipeline module.

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

    const span = findErrorSpan(desc.formId, name);
    if (span) {
      span.textContent = msg;
      span.removeAttribute("hidden");
      span.style.display = "";
    }

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

function findErrorSpan(containerId: string, fieldName: string): HTMLElement | null {
  const container = document.getElementById(containerId);
  if (!container) return null;
  return container.querySelector(`span[data-valmsg-for="${fieldName}"]`);
}

function showError(containerId: string, field: ValidationField, message: string): void {
  const el = document.getElementById(field.fieldId);
  if (el) el.classList.add(ERR_CLASS);

  const span = findErrorSpan(containerId, field.fieldName);
  if (span) {
    span.textContent = message;
    span.removeAttribute("hidden");
    span.style.display = "";
  }
}

function clearFieldError(containerId: string, field: ValidationField): void {
  const span = findErrorSpan(containerId, field.fieldName);
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
  if (!srcField) return true; // field not in descriptor → condition passes (no fallback)
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

### Step 4: Run tests

Run: `npm test`

Expected: ALL pass.

### Step 5: Commit

```bash
git add Alis.Reactive.SandboxApp/Scripts/validation.ts Alis.Reactive.SandboxApp/Scripts/__tests__/when-validating-form-fields.test.ts
git commit -m "feat: stateless validation.ts module — data-valmsg-for, no register, no fallbacks"
```

---

## Task 7: Wire `validation.ts` into `http.ts` + `commands.ts`, Delete `forms.ts`

**Files:**
- Modify: `Alis.Reactive.SandboxApp/Scripts/http.ts`
- Modify: `Alis.Reactive.SandboxApp/Scripts/commands.ts`
- Delete: `Alis.Reactive.SandboxApp/Scripts/forms.ts`

### Step 1: Update `http.ts`

Replace `import { register, validate } from "./forms"` with `import { validate, wireLiveClearing } from "./validation"`. Change the validation flow:

```typescript
import type { RequestDescriptor, StatusHandler, ExecContext } from "./types";
import { resolveGather } from "./gather";
import { executeCommands } from "./commands";
import { validate, wireLiveClearing } from "./validation";
import { scope } from "./trace";

const log = scope("http");

export async function execRequest(req: RequestDescriptor, ctx?: ExecContext): Promise<void> {
  // 0. Pre-request validation — stateless, no register step
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

Replace `import { showFieldErrors } from "./forms"` with `import { showServerErrors } from "./validation"`. Use `ctx.validationDesc`:

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

### Step 4: Build and test

Run: `npm run build && npm test`

Expected: Bundle compiles. All TS tests pass.

### Step 5: Commit

```bash
git add Alis.Reactive.SandboxApp/Scripts/http.ts Alis.Reactive.SandboxApp/Scripts/commands.ts
git rm Alis.Reactive.SandboxApp/Scripts/forms.ts
git commit -m "refactor: wire validation.ts into http + commands, delete forms.ts"
```

---

## Task 8: C# Unit Test — Validation Descriptor in Plan (SOLID Loop)

**Files:**
- Modify: `tests/Alis.Reactive.UnitTests/Requests/WhenRequestingFromServer.cs`

**Why:** The SOLID loop requires a C# unit test that verifies the validation descriptor appears correctly in the rendered plan JSON. This test verifies the full chain: `.Validate<TValidator>(formId)` → FluentValidationAdapter extracts rules → plan.Render() serializes them.

### Step 1: Add snapshot test for validation in request

Add to `WhenRequestingFromServer.cs`:

```csharp
[Test]
public Task Post_with_validation_includes_descriptor() =>
    VerifyJson(Build(p =>
        p.Post("/api/save", g => g.Static("name", "test"))
         .Validate<TestModelValidator>("testForm")
         .Response(r => r
            .OnSuccess(s => s.Element("result").SetText("saved"))
            .OnError(400, e => e.ValidationErrors("testForm")))
    ).Render());

[Test]
public void Post_with_validation_conforms_to_schema()
{
    var plan = CreatePlan();
    Trigger(plan).DomReady(p =>
        p.Post("/api/save", g => g.Static("name", "test"))
         .Validate<TestModelValidator>("testForm")
         .Response(r => r
            .OnSuccess(s => s.Element("x").SetText("ok"))
            .OnError(400, e => e.ValidationErrors("testForm"))));
    AssertSchemaValid(plan.Render());
}
```

**Note:** This requires a `TestModelValidator` FluentValidation validator in the test project. Check if one exists — if not, create a simple one.

### Step 2: Run tests to see if snapshot needs accepting

Run: `dotnet test tests/Alis.Reactive.UnitTests`

Expected: New snapshot test creates a `.received.txt` file. Review and accept as `.verified.txt`.

### Step 3: Update any other failing snapshots

If `errorId` removal caused existing `.verified.txt` files to fail, accept the new snapshots.

### Step 4: Commit

```bash
git add tests/Alis.Reactive.UnitTests/
git commit -m "test: add C# unit test verifying validation descriptor in plan JSON"
```

---

## Task 9: Full Build + All Three Test Layers

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

Expected: Some Playwright tests WILL FAIL because they use `#err_xxx` selectors.

### Step 5: Update Playwright selectors

Open `tests/Alis.Reactive.PlaywrightTests/Validation/WhenValidatingFormFields.cs`.

Replace all `#err_xxx` selectors with `[data-valmsg-for='fieldName']` selectors:

```csharp
// Section 1 (no prefix) — Html.Field() renders spans inside form, scoped by form
// Before: Page.Locator("#err_Name")
// After:  Page.Locator("#all-rules-form [data-valmsg-for='Name']")

// Section 4 (conditional) — no prefix
// Before: Page.Locator("#err_JobTitle")
// After:  Page.Locator("#conditional-form [data-valmsg-for='JobTitle']")

// Section 5 (live prefix) — data-valmsg-for uses unprefixed name
// Before: Page.Locator("#err_live_Name")
// After:  Page.Locator("#live-form [data-valmsg-for='Name']")

// Section 6 (cmb prefix)
// Before: Page.Locator("#err_cmb_Name")
// After:  Page.Locator("#combined-form [data-valmsg-for='Name']")

// Section 7 (hf prefix)
// Before: Page.Locator("#err_hf_Name")
// After:  Page.Locator("#hidden-fields-form [data-valmsg-for='Name']")
// Before: Page.Locator("#err_hf_Phone")
// After:  Page.Locator("#hidden-fields-form [data-valmsg-for='Phone']")
// Before: Page.Locator("#err_hf_Salary")
// After:  Page.Locator("#hidden-fields-form [data-valmsg-for='Salary']")

// Section 8 (db prefix)
// Before: Page.Locator("#err_db_Name")
// After:  Page.Locator("#db-form [data-valmsg-for='Name']")
// Before: Page.Locator("#err_db_Email")
// After:  Page.Locator("#db-form [data-valmsg-for='Email']")

// Section 9 (partial)
// Before: Page.Locator("#err_partial_Address_Street")
// After:  Page.Locator("#partial-form [data-valmsg-for='Address.Street']")
```

Apply ALL selector replacements in the file.

### Step 6: Re-run Playwright tests

Run: `dotnet test tests/Alis.Reactive.PlaywrightTests`

Expected: ALL pass.

### Step 7: Commit

```bash
git add tests/Alis.Reactive.PlaywrightTests/
git commit -m "test: update Playwright selectors from err_ IDs to data-valmsg-for"
```

---

## Task 10: Final Verification — No References to `forms.ts` or `errorId`

### Step 1: Search for stale references

```bash
grep -r "forms\.ts\|from.*./forms\|errorId\|err_\|ErrorId" --include="*.ts" --include="*.cs" --include="*.cshtml" --include="*.json" Alis.Reactive.SandboxApp/ Alis.Reactive/ Alis.Reactive.Native/ Alis.Reactive.FluentValidator/ tests/
```

Expected: Zero matches in source files. `wwwroot/js/alis-reactive.js` (bundled output) may still contain old code until rebuilt.

### Step 2: Rebuild bundle

```bash
npm run build:all
dotnet build
```

### Step 3: Run ALL tests one final time

```bash
npm test
dotnet test tests/Alis.Reactive.UnitTests
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests
dotnet test tests/Alis.Reactive.Native.UnitTests
dotnet test tests/Alis.Reactive.PlaywrightTests
```

Expected: ALL green across all layers.

### Step 4: Final commit (if any fixes)

```bash
git add -A
git commit -m "chore: final cleanup — validation module complete"
```

---

## Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| **`Html.Field()` with prefix overload** | Renders label + input + `data-valmsg-for` span. Prefix overload passes computed ID to callback so caller doesn't hardcode it. Non-breaking — existing overload unchanged. |
| **Stateless API** (no `register()`, no global `Map`) | `validate(desc)` takes descriptor directly from the HTTP pipeline. Simpler, testable, no state leaks. |
| **`data-valmsg-for` over `errorId`** | FieldBuilder renders `data-valmsg-for`. Error spans found by `querySelector` scoped to container. Eliminates ID conventions and fallbacks entirely. |
| **Container, not form** | `findErrorSpan(containerId, fieldName)` uses any container ID. Future-proof for non-form containers. |
| **Descriptor flows through `ExecContext`** | `validationDesc` in `ExecContext` so `validation-errors` command calls `showServerErrors()` without global state. |
| **Event delegation for live clearing** | One `input`/`change` listener on the container, not per-field. `data-alisValidated` prevents duplicate wiring. |
| **No fallbacks anywhere** | If field not in descriptor, condition passes. No DOM fallback. No ID guessing. Explicit IDs only. |
| **Validation rules ≠ Conditions/Guards** | 11 rule types + 4 ValidationCondition ops. Completely separate from pipeline Guards (20+ ops). |
| **Vendor-aware value reading** | Native: `.value`/`.checked`. Fusion: `readExpr` via `new Function`. Extensible for future SF components. |
| **SOLID loop verified** | C# unit test → schema test → TS unit test → Playwright browser test. All layers verify validation. |
