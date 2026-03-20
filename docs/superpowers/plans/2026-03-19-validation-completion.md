# Validation Module Completion — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 100% extraction of FluentValidation rules with type-aware coercion, cross-property comparisons, date support, and complete Playwright coverage. Zero silent drops.

**Architecture:** Add `coerceAs` (from TProperty) and `field` (cross-property binding path) to ValidationRule schema. Rule engine uses existing `coerce()` from `core/coerce.ts` instead of blind `Number()`. Adapter determines coercion from property type at extraction time. All rules work identically inside `WhenField()` conditional blocks.

**Tech Stack:** C# (ValidationRule, FluentValidationAdapter), TypeScript (rule-engine.ts, types/validation.ts), JSON Schema, Vitest + jsdom, Playwright

**Design spec:** Discussed in session — no separate spec file. This plan IS the spec.

**Test baseline:** 944 TS, 53 FluentValidator C#, 492 Playwright. All green.

---

## Files Modified

| File | Change |
|------|--------|
| `Alis.Reactive/Validation/ValidationRule.cs` | Add `CoerceAs` and `Field` properties |
| `Alis.Reactive/Schemas/reactive-plan.schema.json` | Add `coerceAs` and `field` to ValidationRule |
| `Alis.Reactive.FluentValidator/FluentValidationAdapter.cs` | Extract all FV rules, determine coerceAs from TProperty, handle MemberToCompare for all comparisons, serialize DateTime constraints as date-only ISO |
| `Alis.Reactive.SandboxApp/Scripts/types/validation.ts` | Add `coerceAs`, `field` to ValidationRule, expand ValidationRuleType |
| `Alis.Reactive.SandboxApp/Scripts/validation/rule-engine.ts` | Use `coerce()` for comparisons, add new rule types, cross-property via field |
| `Alis.Reactive.SandboxApp/Scripts/validation/orchestrator.ts` | Update peerReader to return raw values |
| `tests/Alis.Reactive.FluentValidator.UnitTests/` | 100% extraction tests for all rule types (unconditional + conditional) |
| `Alis.Reactive.SandboxApp/Scripts/__tests__/` | 100% rule engine tests for all rule types with all coercion types |
| `tests/Alis.Reactive.PlaywrightTests/Validation/` | New parallel fixture: date validation, cross-property, all rule types |

## Complete Rule Types (deterministic — derived from FluentValidation + TProperty)

| # | FV DSL | Rule Type | constraint | field | coerceAs | empty behavior |
|---|--------|-----------|-----------|-------|----------|----------------|
| 1 | NotEmpty/NotNull | `required` | — | — | — | fails |
| 2 | Empty | `empty` | — | — | — | passes only when empty |
| 3 | MinimumLength | `minLength` | N | — | — | skips empty |
| 4 | MaximumLength | `maxLength` | N | — | — | skips empty |
| 5 | EmailAddress | `email` | — | — | — | skips empty |
| 6 | Matches | `regex` | pattern | — | — | skips empty |
| 7 | url | `url` | — | — | — | skips empty |
| 8 | CreditCard | `creditCard` | — | — | — | skips empty |
| 9 | InclusiveBetween | `range` | [lo,hi] | — | from TProperty | skips empty |
| 10 | ExclusiveBetween | `exclusiveRange` | [lo,hi] | — | from TProperty | skips empty |
| 11 | GreaterThanOrEqualTo(val) | `min` | val | — | from TProperty | skips empty |
| 12 | GreaterThanOrEqualTo(prop) | `min` | — | fieldName | from TProperty | skips empty |
| 13 | LessThanOrEqualTo(val) | `max` | val | — | from TProperty | skips empty |
| 14 | LessThanOrEqualTo(prop) | `max` | — | fieldName | from TProperty | skips empty |
| 15 | GreaterThan(val) | `gt` | val | — | from TProperty | fails (implies required) |
| 16 | GreaterThan(prop) | `gt` | — | fieldName | from TProperty | fails (implies required) |
| 17 | LessThan(val) | `lt` | val | — | from TProperty | skips empty |
| 18 | LessThan(prop) | `lt` | — | fieldName | from TProperty | skips empty |
| 19 | Equal(prop) | `equalTo` | — | fieldName | — | skips empty |
| 20 | Equal(val) | `equalTo` | val | — | from TProperty | skips empty |
| 21 | NotEqual(prop) | `notEqualTo` | — | fieldName | — | skips empty |
| 22 | NotEqual(val) | `notEqual` | val | — | from TProperty | skips empty |
| 23 | atLeastOne | `atLeastOne` | — | — | — | fails when empty array |

**coerceAs derivation from TProperty:**

| C# Type | coerceAs |
|---------|----------|
| `decimal`, `int`, `long`, `double`, `float`, `byte`, `short` | `"number"` |
| `DateTime`, `DateTime?`, `DateTimeOffset`, `DateTimeOffset?`, `DateOnly`, `DateOnly?` | `"date"` |
| `string`, all others | omitted (default string comparison) |

**Date constraint serialization:**
- `DateTime` with `TimeOfDay == Zero` → `"YYYY-MM-DD"` (date-only ISO, parsed as local midnight by `toDate()`)
- `DateTime` with time → `"YYYY-MM-DDTHH:mm:ss"` (ISO without timezone, parsed as local by `toDate()`)
- This ensures the constraint represents the SAME local date/time regardless of server/browser timezone

**Not extracted (server-only by nature):**
Must, MustAsync, Custom, CustomAsync, ForEach, PolymorphicValidator, PrecisionScale, IsInEnum, IsEnumName

---

### Task 1: Schema — Add `coerceAs` and `field` to ValidationRule (C#)

**Files:**
- Modify: `Alis.Reactive/Validation/ValidationRule.cs`
- Modify: `Alis.Reactive/Schemas/reactive-plan.schema.json`

- [ ] **Step 1: Add properties to ValidationRule.cs**

```csharp
public sealed class ValidationRule
{
    public string Rule { get; }
    public string Message { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Constraint { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Field { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CoerceAs { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ValidationCondition? When { get; }

    public ValidationRule(string rule, string message, object? constraint = null,
        ValidationCondition? when = null, string? field = null, string? coerceAs = null)
    {
        Rule = rule;
        Message = message;
        Constraint = constraint;
        When = when;
        Field = field;
        CoerceAs = coerceAs;
    }
}
```

- [ ] **Step 2: Update JSON schema — add `coerceAs` and `field` to ValidationRule, expand ValidationRuleType enum**

In `reactive-plan.schema.json`, update `ValidationRuleType`:
```json
"ValidationRuleType": {
  "type": "string",
  "enum": ["required", "empty", "minLength", "maxLength", "email", "regex", "url",
           "creditCard", "range", "exclusiveRange", "min", "max", "gt", "lt",
           "equalTo", "notEqual", "notEqualTo", "atLeastOne"]
}
```

Add to `ValidationRule.properties`:
```json
"field": { "type": "string", "minLength": 1, "description": "Cross-property: binding path of field to compare against" },
"coerceAs": { "type": "string", "enum": ["string", "number", "boolean", "date"], "description": "Coercion type for comparison — derived from TProperty" }
```

- [ ] **Step 3: Update ExtractedRule in adapter to carry field + coerceAs**

```csharp
private sealed class ExtractedRule
{
    public string Rule { get; }
    public string Message { get; }
    public object? Constraint { get; }
    public string? Field { get; }
    public string? CoerceAs { get; }
    public ValidationCondition? When { get; }

    public ExtractedRule(string rule, string message, object? constraint,
        ValidationCondition? when = null, string? field = null, string? coerceAs = null)
    {
        Rule = rule; Message = message; Constraint = constraint;
        When = when; Field = field; CoerceAs = coerceAs;
    }
}
```

Update the `fieldRules` → `ValidationRule` mapping (line 56-59) to pass `field` and `coerceAs`:
```csharp
rules.Add(new ValidationRule(er.Rule, er.Message, er.Constraint, er.When, er.Field, er.CoerceAs));
```

- [ ] **Step 4: Run `dotnet build` — verify compile**
- [ ] **Step 5: Run existing C# tests — verify no breakage**

Run: `dotnet test tests/Alis.Reactive.UnitTests && dotnet test tests/Alis.Reactive.FluentValidator.UnitTests`

- [ ] **Step 6: Commit**

```bash
git commit -m "feat: add coerceAs and field to ValidationRule schema"
```

---

### Task 2: Schema — Update TS types

**Files:**
- Modify: `Alis.Reactive.SandboxApp/Scripts/types/validation.ts`

- [ ] **Step 1: Update ValidationRule and ValidationRuleType**

```typescript
export type ValidationRuleType =
  | "required" | "empty"
  | "minLength" | "maxLength"
  | "email" | "regex" | "url" | "creditCard"
  | "range" | "exclusiveRange"
  | "min" | "max" | "gt" | "lt"
  | "equalTo" | "notEqual" | "notEqualTo"
  | "atLeastOne";

export interface ValidationRule {
  rule: ValidationRuleType;
  message: string;
  constraint?: unknown;
  field?: string;
  coerceAs?: CoercionType;
  when?: ValidationCondition;
}
```

Add import for `CoercionType`:
```typescript
import type { CoercionType } from "../core/coerce";
```

- [ ] **Step 2: Run `npm run typecheck` — verify compile**
- [ ] **Step 3: Run `npx vitest run` — verify no breakage**
- [ ] **Step 4: Commit**

```bash
git commit -m "feat: add coerceAs, field, and new rule types to TS validation types"
```

---

### Task 3: Rule Engine — Replace `Number()` with `coerce()`, add all new rules

**Files:**
- Modify: `Alis.Reactive.SandboxApp/Scripts/validation/rule-engine.ts`
- Create: `Alis.Reactive.SandboxApp/Scripts/__tests__/when-evaluating-all-validation-rules.test.ts`

- [ ] **Step 1: Write BDD tests for ALL rule types with ALL coercion types**

Create `when-evaluating-all-validation-rules.test.ts` — comprehensive tests:

Each rule type × each applicable coercion type:
- `min`/`max`/`gt`/`lt` with `coerceAs: "number"` (existing behavior, no regression)
- `min`/`max`/`gt`/`lt` with `coerceAs: "date"` (Date objects, ISO strings)
- `range`/`exclusiveRange` with `coerceAs: "number"` and `"date"`
- `equalTo` with `field` (cross-property, existing)
- `notEqual`/`notEqualTo` (new)
- `empty` (new — inverse of required)
- `creditCard` (Luhn algorithm)
- Cross-property `min`/`max`/`gt`/`lt` with `field` + `coerceAs: "date"` (DischargeDate > AdmissionDate)

The peerReader for cross-property tests returns raw values (Date objects, numbers), not strings.

- [ ] **Step 2: Run tests — verify they FAIL (new rules not implemented yet)**

- [ ] **Step 3: Update peerReader interface — return `unknown` instead of `string`**

```typescript
export interface PeerReader {
  readPeer(fieldName: string): unknown;
}
```

- [ ] **Step 4: Implement the complete rule engine**

```typescript
import { coerce } from "../core/coerce";
import type { CoercionType } from "../core/coerce";
import type { ValidationRule } from "../types";

export interface PeerReader {
  readPeer(fieldName: string): unknown;
}

function compareValues(a: unknown, b: unknown, coerceAs?: CoercionType): number {
  const type = coerceAs ?? "number";
  const ca = coerce(a, type) as number;
  const cb = coerce(b, type) as number;
  if (Number.isNaN(ca) || Number.isNaN(cb)) return NaN;
  return ca - cb;
}

function resolveTarget(rule: ValidationRule, peerReader: PeerReader): unknown {
  if (rule.field) {
    const peer = peerReader.readPeer(rule.field);
    if (peer == null) return undefined; // peer unresolvable
    return peer;
  }
  return rule.constraint;
}

export function ruleFails(
  rule: ValidationRule,
  value: unknown,
  peerReader: PeerReader
): boolean {
  const str = value == null ? "" : String(value);
  const empty = value == null || str === "" || value === false;

  switch (rule.rule) {
    case "required":
      return empty;
    case "empty":
      return !empty;
    case "minLength":
      return !empty && str.length < Number(rule.constraint);
    case "maxLength":
      return !empty && str.length > Number(rule.constraint);
    case "email":
      return !empty && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(str);
    case "regex": {
      try { return !empty && !new RegExp(String(rule.constraint)).test(str); }
      catch { return true; }
    }
    case "url":
      return !empty && !/^https?:\/\/.+/.test(str);
    case "creditCard":
      return !empty && !luhn(str.replace(/\D/g, ""));

    case "min": {
      const target = resolveTarget(rule, peerReader);
      if (target === undefined) return true;
      return !empty && compareValues(value, target, rule.coerceAs) < 0;
    }
    case "max": {
      const target = resolveTarget(rule, peerReader);
      if (target === undefined) return true;
      return !empty && compareValues(value, target, rule.coerceAs) > 0;
    }
    case "gt": {
      const target = resolveTarget(rule, peerReader);
      if (target === undefined) return true;
      return empty || compareValues(value, target, rule.coerceAs) <= 0;
    }
    case "lt": {
      const target = resolveTarget(rule, peerReader);
      if (target === undefined) return true;
      return !empty && compareValues(value, target, rule.coerceAs) >= 0;
    }

    case "range": {
      const [lo, hi] = rule.constraint as [unknown, unknown];
      if (empty) return false;
      return compareValues(value, lo, rule.coerceAs) < 0
          || compareValues(value, hi, rule.coerceAs) > 0;
    }
    case "exclusiveRange": {
      const [lo, hi] = rule.constraint as [unknown, unknown];
      if (empty) return false;
      return compareValues(value, lo, rule.coerceAs) <= 0
          || compareValues(value, hi, rule.coerceAs) >= 0;
    }

    case "equalTo": {
      const target = resolveTarget(rule, peerReader);
      if (target === undefined) return true;
      if (rule.coerceAs) {
        return compareValues(value, target, rule.coerceAs) !== 0;
      }
      return String(value ?? "") !== String(target ?? "");
    }
    case "notEqual":
      return !empty && String(value) === String(rule.constraint);
    case "notEqualTo": {
      const target = resolveTarget(rule, peerReader);
      if (target === undefined) return true;
      if (rule.coerceAs) {
        return !empty && compareValues(value, target, rule.coerceAs) === 0;
      }
      return !empty && String(value ?? "") === String(target ?? "");
    }

    case "atLeastOne":
      return Array.isArray(value) ? value.length === 0 : empty;

    default:
      return true; // Unknown rule type → fail-closed
  }
}

function luhn(digits: string): boolean {
  if (digits.length < 13) return false;
  let sum = 0;
  let alt = false;
  for (let i = digits.length - 1; i >= 0; i--) {
    let n = parseInt(digits[i], 10);
    if (alt) { n *= 2; if (n > 9) n -= 9; }
    sum += n;
    alt = !alt;
  }
  return sum % 10 === 0;
}
```

- [ ] **Step 5: Update orchestrator peerReader to return raw values**

In `orchestrator.ts`, change `domPeerReader`:
```typescript
function domPeerReader(byName: Map<string, ValidationField>): PeerReader {
  return {
    readPeer(fieldName: string): unknown {
      const other = byName.get(fieldName);
      if (!other?.fieldId || !other.vendor || !other.readExpr) return undefined;
      const otherEl = document.getElementById(other.fieldId);
      if (!otherEl) return undefined;
      const otherRoot = resolveRoot(otherEl, other.vendor);
      return walk(otherRoot, other.readExpr);
    },
  };
}
```

- [ ] **Step 6: Run all tests**

Run: `npx vitest run`
Expected: ALL pass (new tests + existing tests)

- [ ] **Step 7: Commit**

```bash
git commit -m "feat: type-aware rule engine with coerceAs, cross-property, and all rule types"
```

---

### Task 4: Adapter — Complete extraction with coerceAs, field, date serialization

**Files:**
- Modify: `Alis.Reactive.FluentValidator/FluentValidationAdapter.cs`
- Create: `tests/Alis.Reactive.FluentValidator.UnitTests/WhenExtractingAllRuleTypes.cs`

- [ ] **Step 1: Write BDD test — 100% extraction coverage**

Create `WhenExtractingAllRuleTypes.cs` with a `FullCoverageValidator` and a `FullCoverageConditionalValidator` (same rules under `WhenField`).

Each test asserts: rule type, constraint value, field value, coerceAs value.

```csharp
public class FullCoverageValidator : AbstractValidator<FullCoverageModel>
{
    public FullCoverageValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Name).MinimumLength(3);
        RuleFor(x => x.Name).MaximumLength(100);
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.Phone).Matches(@"^\d{3}-\d{3}-\d{4}$");
        RuleFor(x => x.CreditCardNumber).CreditCard();
        RuleFor(x => x.Age).InclusiveBetween(0, 120);
        RuleFor(x => x.Score).ExclusiveBetween(0m, 100m);
        RuleFor(x => x.Salary).GreaterThanOrEqualTo(0m);
        RuleFor(x => x.Salary).LessThanOrEqualTo(500000m);
        RuleFor(x => x.MonthlyRate).GreaterThan(0m);
        RuleFor(x => x.MonthlyRate).LessThan(1000000m);
        RuleFor(x => x.ConfirmEmail).Equal(x => x.Email);
        RuleFor(x => x.AlternateEmail).NotEqual(x => x.Email);
        RuleFor(x => x.Status).NotEqual("deleted");
        RuleFor(x => x.AdmissionDate).GreaterThanOrEqualTo(new DateTime(2020, 1, 1));
        RuleFor(x => x.DischargeDate).GreaterThan(x => x.AdmissionDate);
        RuleFor(x => x.Nickname).Empty();
    }
}
```

`FullCoverageModel`:
```csharp
public class FullCoverageModel
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? ConfirmEmail { get; set; }
    public string? AlternateEmail { get; set; }
    public string? Phone { get; set; }
    public string? CreditCardNumber { get; set; }
    public int Age { get; set; }
    public decimal Score { get; set; }
    public decimal Salary { get; set; }
    public decimal MonthlyRate { get; set; }
    public string? Status { get; set; }
    public string? Nickname { get; set; }
    public DateTime AdmissionDate { get; set; }
    public DateTime DischargeDate { get; set; }
    public bool IsEmployed { get; set; }
}
```

Same validator wrapped in `WhenField(x => x.IsEmployed, () => { ... })` for conditional parity test.

- [ ] **Step 2: Run tests — verify they FAIL**

- [ ] **Step 3: Add `InferCoerceAs` helper to adapter**

```csharp
private static string? InferCoerceAs(Type propertyType)
{
    var t = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
    if (t == typeof(decimal) || t == typeof(int) || t == typeof(long) ||
        t == typeof(double) || t == typeof(float) || t == typeof(byte) || t == typeof(short))
        return "number";
    if (t == typeof(DateTime) || t == typeof(DateTimeOffset) || t == typeof(DateOnly))
        return "date";
    return null;
}

private static object SerializeDateConstraint(object value)
{
    if (value is DateTime dt)
        return dt.TimeOfDay == TimeSpan.Zero
            ? dt.ToString("yyyy-MM-dd")
            : dt.ToString("s"); // ISO 8601 without timezone
    if (value is DateTimeOffset dto)
        return dto.TimeOfDay == TimeSpan.Zero
            ? dto.ToString("yyyy-MM-dd")
            : dto.ToString("s");
    return value;
}
```

- [ ] **Step 4: Update MapComponent to extract ALL rules with coerceAs + field**

Pass `IValidationRule` (which has property type info) to MapComponent so it can call `InferCoerceAs`. Handle:
- `INotEmptyValidator` → `required` (done)
- `INotNullValidator` → `required` (done)
- `EmptyValidator` → `empty` (NEW — check for `IEmptyValidator` or concrete type)
- `ILengthValidator` → `minLength`/`maxLength` (done)
- `IEmailValidator` → `email` (done)
- `IRegularExpressionValidator` → `regex` (done)
- `ICreditCardValidator` → `creditCard` (NEW)
- `IBetweenValidator` → check `exclusive` flag: `range` or `exclusiveRange` with coerceAs, serialize date constraints
- `IComparisonValidator` → ALL comparisons:
  - Check `MemberToCompare != null` → cross-property: emit with `field = MemberToCompare.Name`
  - Otherwise → fixed value: emit with `constraint = SerializeDateConstraint(ValueToCompare)`, `coerceAs`
  - Handle ALL `Comparison` enum values: Equal, NotEqual, GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual

- [ ] **Step 5: Run tests — verify ALL pass**
- [ ] **Step 6: Run full C# test suite**

Run: `dotnet test tests/Alis.Reactive.FluentValidator.UnitTests && dotnet test tests/Alis.Reactive.UnitTests`

- [ ] **Step 7: Commit**

```bash
git commit -m "feat: complete FluentValidation extraction — all rules, coerceAs, cross-property, dates"
```

---

### Task 5: Playwright — Date validation and cross-property in browser

**Files:**
- Create: `tests/Alis.Reactive.PlaywrightTests/Validation/WhenValidatingDatesAndCrossProperty.cs`
- Create: sandbox page + model + validator for testing (or extend existing Validation page)

This task creates a new Playwright test fixture that exercises date validation and cross-property comparisons end-to-end in the browser. Uses Syncfusion DatePicker components.

- [ ] **Step 1: Create test model + validator with date rules**

Model with `AdmissionDate` (DateTime?, required, >= 2020-01-01) and `DischargeDate` (DateTime?, > AdmissionDate).

- [ ] **Step 2: Create/extend sandbox Validation page section for date validation**

Two DatePicker fields with cross-property validation. Submit button triggers validation.

- [ ] **Step 3: Write Playwright BDD tests**

```
- empty dates show required errors on submit
- admission date before 2020 shows min error on blur
- valid admission date clears error on blur
- discharge date before admission shows gt error on blur
- valid discharge date clears error on blur
- cross-property: changing admission date to after discharge re-validates discharge on next blur
```

- [ ] **Step 4: Run Playwright tests**

Run: `dotnet test tests/Alis.Reactive.PlaywrightTests --filter "WhenValidatingDatesAndCrossProperty"`

- [ ] **Step 5: Run full Playwright suite — verify no regression**

Run: `dotnet test tests/Alis.Reactive.PlaywrightTests`

- [ ] **Step 6: Commit**

```bash
git commit -m "test: Playwright coverage for date validation and cross-property comparisons"
```

---

### Task 6: Full verification

- [ ] **Step 1: `npm run typecheck`** — zero errors (pre-existing test-widget.ts OK)
- [ ] **Step 2: `npx vitest run`** — all TS tests pass
- [ ] **Step 3: `npm run build:all`** — bundle builds
- [ ] **Step 4: `dotnet build`** — all C# compiles
- [ ] **Step 5: All C# tests** — `dotnet test tests/Alis.Reactive.UnitTests && dotnet test tests/Alis.Reactive.Native.UnitTests && dotnet test tests/Alis.Reactive.Fusion.UnitTests && dotnet test tests/Alis.Reactive.FluentValidator.UnitTests`
- [ ] **Step 6: Restart app + Playwright** — all browser tests pass
- [ ] **Step 7: Commit if any fixes needed**

---

## Summary

| Task | What | Proves |
|------|------|--------|
| 1 | C# schema: `coerceAs` + `field` on ValidationRule | Schema deterministic |
| 2 | TS types mirror C# schema | Schema flows to runtime |
| 3 | Rule engine: `coerce()`, cross-property, all rules | Runtime handles all types |
| 4 | Adapter: 100% extraction, no silent drops | Extraction complete |
| 5 | Playwright: dates, cross-property in browser | End-to-end proof |
| 6 | Full verification | All 3 layers green |

**Rule coverage: 23 rule types × 2 modes (unconditional + conditional) = 46 extraction paths, all tested.**

**Coercion coverage: number, date, string — all comparison rules tested with each applicable coercion type.**

**Zero silent drops: every FV validator either extracts to a client rule or is documented as server-only.**
