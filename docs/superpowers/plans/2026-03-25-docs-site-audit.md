# Docs Site Audit — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Bring the docs site and CLAUDE.md into alignment with the current source code on main.

**Architecture:** Six tasks ordered by severity. Task 1 fixes CLAUDE.md (the framework's law document) which has multiple stale references. Tasks 2–5 fix docs-site content pages. Task 6 is a cosmetic test-count update.

**Tech Stack:** Markdown/MDX (Starlight), C# (reference reading), astro.config.mjs (sidebar)

---

## Evidence Summary

### Source of truth (verified on main @ 5583b77)

| Fact | Actual value |
|------|-------------|
| TS entry point | `Scripts/root.ts` |
| Data attribute | `data-reactive-plan` |
| CSS file | `design-system.css` |
| Resolver exports | `resolveSource()`, `resolveEventPath()`, `resolveSourceAs()` |
| Coercion types | `string`, `number`, `boolean`, `date`, `raw`, `array` |
| Trigger kinds (5) | `dom-ready`, `custom-event`, `component-event`, `server-push`, `signalr` |
| Command kinds (5) | `dispatch`, `mutate-element`, `mutate-event`, `validation-errors`, `into` |
| Reaction kinds (4) | `sequential`, `conditional`, `http`, `parallel-http` |
| TS module structure | Subdirectories: `core/`, `types/`, `resolution/`, `execution/`, `lifecycle/`, `conditions/`, `validation/`, `components/` |

---

## Task 1: Fix CLAUDE.md — Stale References (CRITICAL)

**Files:**
- Modify: `CLAUDE.md` (root of repo)

**What's wrong:** CLAUDE.md references file names, function names, data attributes, and CSS names that were renamed during the TS restructure and other refactors. Since CLAUDE.md is declared as "law," this is the highest priority fix.

### Specific changes needed

#### 1a. Entry point: `auto-boot.ts` → `root.ts`

CLAUDE.md says `Scripts/auto-boot.ts` in 3+ locations. Actual file is `Scripts/root.ts`.

**Locations to fix:**
- Line ~149: Key files table row `Scripts/auto-boot.ts`
- Line ~181–188: Auto-boot architecture section header and body
- Line ~185: Auto-boot table row
- Line ~281: Build commands comment `# JS runtime (ESM bundle — entry: auto-boot.ts)`
- Line ~327: Rule 2 text `auto-boot.ts handles discovery and boot automatically`

- [ ] **Step 1:** Find and replace all `auto-boot.ts` → `root.ts` in CLAUDE.md
- [ ] **Step 2:** Update the auto-boot architecture section heading to say "Auto-boot architecture" but reference `root.ts`
- [ ] **Step 3:** Verify no remaining references with grep

#### 1b. Data attribute: `data-alis-plan` → `data-reactive-plan`

CLAUDE.md says `data-alis-plan` in 3+ locations. Source uses `data-reactive-plan` (confirmed in `root.ts` line 19 and `PlanExtensions.cs`).

**Locations to fix:**
- Line ~40: Code sample `<script type="application/json" data-alis-plan ...>`
- Line ~149: Table description `auto-discovers [data-alis-plan]`
- Line ~178: Code sample
- Line ~185: Table description
- Line ~188: Body text

- [ ] **Step 4:** Find and replace all `data-alis-plan` → `data-reactive-plan` in CLAUDE.md
- [ ] **Step 5:** Verify no remaining references with grep

#### 1c. CSS file: `alis-modern-tailwind.css` → `design-system.css`

CLAUDE.md line ~174 says `alis-modern-tailwind.css`. Layout uses `design-system.css`.

**Locations to fix:**
- Line ~174: Layout code sample `href="~/css/alis-modern-tailwind.css"`
- Line ~285: Build command comment `# → wwwroot/css/alis-modern-tailwind.css`

- [ ] **Step 6:** Find and replace all `alis-modern-tailwind.css` → `design-system.css` in CLAUDE.md

#### 1d. Resolver exports: stale function names

CLAUDE.md says resolver.ts exports `resolve()`, `resolveAs()`, `resolveToString()`, `coerce()`.
Actual exports are `resolveSource()`, `resolveEventPath()`, `resolveSourceAs()`.

**Locations to fix:**
- Line ~130–137: Resolver description paragraph
- Line ~131: Table entry for `Scripts/resolver.ts`

- [ ] **Step 7:** Update resolver.ts description to list actual exports: `resolveSource()`, `resolveEventPath()`, `resolveSourceAs()`
- [ ] **Step 8:** Update the resolver table row to match actual function names

#### 1e. Coercion types: missing `date` and `array`

CLAUDE.md says coercion types are `string`, `number`, `boolean`, `raw`.
Actual types include `date` and `array` (6 total).

**Location to fix:**
- Line ~137: "Coercion types: `string` (null→""), `number` (NaN→0), `boolean` ("false"→false), `raw`."

- [ ] **Step 9:** Update coercion types to: `string`, `number`, `boolean`, `date`, `raw`, `array`

#### 1f. TS module structure: flat → subdirectories

CLAUDE.md Layer 3 key files table lists flat paths like `Scripts/trigger.ts`, `Scripts/execute.ts`, `Scripts/element.ts`, `Scripts/resolver.ts`. Actual structure uses subdirectories: `execution/trigger.ts`, `execution/execute.ts`, etc.

**Location to fix:**
- Lines ~145–156: Key files table

- [ ] **Step 10:** Update the TS key files table to reflect actual subdirectory paths:

| Old path | New path |
|----------|----------|
| `Scripts/auto-boot.ts` | `Scripts/root.ts` |
| `Scripts/boot.ts` | `Scripts/lifecycle/boot.ts` |
| `Scripts/trigger.ts` | `Scripts/execution/trigger.ts` |
| `Scripts/execute.ts` | `Scripts/execution/execute.ts` |
| `Scripts/element.ts` | `Scripts/execution/element.ts` |
| `Scripts/resolver.ts` | `Scripts/resolution/resolver.ts` |
| `Scripts/trace.ts` | `Scripts/core/trace.ts` |
| `Scripts/types.ts` | `Scripts/types/index.ts` (barrel) |

#### 1g. Code sample rendering pattern: `@Html.Raw(plan.Render())` → `@Html.RenderPlan(plan)`

CLAUDE.md line ~40 uses the old manual pattern:
```html
<script type="application/json" data-alis-plan data-trace="trace">@Html.Raw(plan.Render())</script>
```

The modern API is `@Html.RenderPlan(plan)` which emits the `<script>` tag, `data-reactive-plan` attribute, and validation summary div automatically. The code sample at line ~178 has the same issue.

- [ ] **Step 11:** Replace both manual `<script>` code samples with `@Html.RenderPlan(plan)`
- [ ] **Step 12:** Commit: `docs: fix stale references in CLAUDE.md`

---

## Task 2: Add ServerPush/SignalR to API Reference (CRITICAL)

**Files:**
- Modify: `docs-site/src/content/docs/reference/api-reference.md`

**What's wrong:** The TriggerBuilder section only lists 3 methods (DomReady, CustomEvent, CustomEvent<T>). Missing 5 overloads for ServerPush and SignalR that exist in source and are documented in the triggers-and-reactions page.

- [ ] **Step 1:** Read `Alis.Reactive/Builders/TriggerBuilder.cs` to get exact signatures for ServerPush and SignalR overloads

- [ ] **Step 2:** Add ServerPush overloads to the TriggerBuilder section:

```csharp
TriggerBuilder<TModel> t.ServerPush(string url, Action<PipelineBuilder<TModel>> configure)
TriggerBuilder<TModel> t.ServerPush(string url, string eventType, Action<PipelineBuilder<TModel>> configure)
TriggerBuilder<TModel> t.ServerPush<TPayload>(string url, string eventType, Action<TPayload, PipelineBuilder<TModel>> configure)
```

- [ ] **Step 3:** Add SignalR overloads to the TriggerBuilder section:

```csharp
TriggerBuilder<TModel> t.SignalR(string hubUrl, string methodName, Action<PipelineBuilder<TModel>> configure)
TriggerBuilder<TModel> t.SignalR<TPayload>(string hubUrl, string methodName, Action<TPayload, PipelineBuilder<TModel>> configure)
```

- [ ] **Step 4:** Commit: `docs: add ServerPush/SignalR to API reference`

---

## Task 3: Create Validation Documentation Page (CRITICAL)

**Files:**
- Create: `docs-site/src/content/docs/csharp-modules/reactivity/validation.md`
- Modify: `docs-site/astro.config.mjs` (add sidebar entry)

**What's wrong:** The framework has a complete validation module (18 rule types, WhenField conditions, coerceAs coercion, fail-closed orchestration) but zero dedicated documentation. Only a brief mention in the HTTP pipeline page.

### Page structure

The validation page should follow the same question-driven format as other reactivity pages:

1. **How do I write a validator?** — ReactiveValidator<T> base class, RuleFor
2. **How do I attach validation to a form?** — `.Validate<TValidator>("formId")` on HTTP pipeline
3. **What rule types are available?** — All 18 with descriptions and examples
4. **How do I add conditional rules?** — WhenField/WhenFieldNot with 4 operators
5. **How do cross-property rules work?** — equalTo, notEqualTo with peer fields
6. **How does coercion work?** — Automatic from C# types (number, date)
7. **What happens when validation fails?** — Fail-closed, inline errors, summary errors
8. **How does live clearing work?** — Errors clear on field input, revalidate on blur/change

### Content to include

- [ ] **Step 1:** Create `docs-site/src/content/docs/csharp-modules/reactivity/validation.md` with frontmatter:

```yaml
---
title: Validation
description: Client-side validation with FluentValidation extraction — 18 rule types, conditional rules, cross-property comparisons, and fail-closed orchestration.
---
```

> **Note:** The Reactivity sidebar group uses manual `items` in `astro.config.mjs`, not `autogenerate`. Ordering is controlled by position in the `items` array (Step 10), not by frontmatter.

- [ ] **Step 2:** Write the "How do I write a validator?" section

```csharp
public class ResidentIntakeValidator : ReactiveValidator<ResidentIntakeModel>
{
    public ResidentIntakeValidator()
    {
        RuleFor(x => x.ResidentName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AdmissionDate).NotEmpty();
        RuleFor(x => x.CareLevel).NotEmpty();
        RuleFor(x => x.Age).InclusiveBetween(18, 120);
        RuleFor(x => x.Email).EmailAddress();
    }
}
```

- [ ] **Step 3:** Write the "How do I attach validation?" section showing `.Validate<T>("formId")` integration with HTTP pipeline

- [ ] **Step 4:** Write the rule types reference table — all 18 rules with C# FluentValidation method, plan rule type, and constraint type:

| FluentValidation method | Plan rule type | Constraint |
|-------------------------|---------------|-----------|
| `NotEmpty()` / `NotNull()` | `required` | — |
| `Empty()` | `empty` | — |
| `MaximumLength(n)` | `maxLength` | number |
| `MinimumLength(n)` | `minLength` | number |
| `EmailAddress()` | `email` | — |
| `Matches(regex)` | `regex` | pattern |
| `CreditCard()` | `creditCard` | — |
| `InclusiveBetween(a, b)` | `range` | [min, max] |
| `ExclusiveBetween(a, b)` | `exclusiveRange` | [min, max] |
| `GreaterThanOrEqualTo(n)` | `min` | number |
| `LessThanOrEqualTo(n)` | `max` | number |
| `GreaterThan(n)` | `gt` | number |
| `LessThan(n)` | `lt` | number |
| `Equal(x => x.Other)` | `equalTo` | field name |
| `NotEqual(value)` | `notEqual` | value |
| `NotEqual(x => x.Other)` | `notEqualTo` | field name |
| (custom: AlisAtLeastOneValidator) | `atLeastOne` | — |

- [ ] **Step 5:** Write the WhenField conditional rules section with all 4 patterns:

```csharp
// Truthy: rule applies when field has a value
WhenField(x => x.IsEmployed, () =>
{
    RuleFor(x => x.EmployerId).NotEmpty();
});

// Equality: rule applies when field equals value
WhenField(x => x.CareLevel, "Memory Care", () =>
{
    RuleFor(x => x.CognitiveScore).NotEmpty().InclusiveBetween(0, 30);
});

// Falsy: rule applies when field is empty
WhenFieldNot(x => x.HasInsurance, () =>
{
    RuleFor(x => x.SelfPayAgreement).NotEmpty();
});

// Inequality: rule applies when field does not equal value
WhenFieldNot(x => x.Status, "Discharged", () =>
{
    RuleFor(x => x.RoomNumber).NotEmpty();
});
```

- [ ] **Step 6:** Write the cross-property rules section

```csharp
// Password confirmation
RuleFor(x => x.PasswordConfirm).Equal(x => x.Password);

// Discharge must be after admission
RuleFor(x => x.DischargeDate).GreaterThan(x => x.AdmissionDate);
```

- [ ] **Step 7:** Write the coercion section — automatic from C# types:

| C# property type | coerceAs | Comparison behavior |
|------------------|----------|-------------------|
| `int`, `decimal`, `double`, etc. | `"number"` | Numeric comparison |
| `DateTime`, `DateTimeOffset`, `DateOnly` | `"date"` | ISO 8601 string comparison |
| `string` | `null` | String comparison |

- [ ] **Step 8:** Write the fail-closed behavior section — unenriched fields block, hidden fields report to summary, missing form blocks

- [ ] **Step 9:** Write the live clearing section — errors clear on input, revalidate on blur/change

- [ ] **Step 10:** Add sidebar entry in `astro.config.mjs` under the Reactivity group, after HTTP Pipeline:

```javascript
{ label: "Validation", slug: "csharp-modules/reactivity/validation" },
```

- [ ] **Step 11:** Commit: `docs: add validation documentation page`

---

## Task 4: Update Architecture Pages — Missing Primitives (HIGH)

**Files:**
- Modify: `docs-site/src/content/docs/architecture/the-contract.mdx`
- Modify: `docs-site/src/content/docs/architecture/json-plan-schema.md`
- Modify: `docs-site/src/content/docs/architecture/runtime.mdx`
- Modify: `docs-site/src/content/docs/architecture/the-builders.mdx`

**What's wrong:** Architecture pages omit `server-push` and `signalr` trigger kinds entirely. The `the-builders.mdx` page only lists 3 TriggerBuilder methods.

> **IMPORTANT:** `Html.RenderPlan(plan)` references in docs pages are CORRECT — it is a real public API method in `PlanExtensions.cs`. Do NOT change these to `plan.Render()`. The CLAUDE.md fix (Task 1g) is the opposite direction — updating CLAUDE.md's old `@Html.Raw(plan.Render())` pattern to use `@Html.RenderPlan(plan)`.

### 4a. Add server-push and signalr to the-contract.mdx

- [ ] **Step 1:** Read `the-contract.mdx` and locate the triggers section
- [ ] **Step 2:** Add `server-push` trigger definition:

```json
{ "kind": "server-push", "url": "/api/stream", "eventType?": "named-event" }
```

- [ ] **Step 3:** Add `signalr` trigger definition:

```json
{ "kind": "signalr", "hubUrl": "/hubs/notifications", "methodName": "Receive" }
```

### 4b. Add server-push and signalr to json-plan-schema.md

- [ ] **Step 4:** Read `json-plan-schema.md` and locate the trigger kinds list
- [ ] **Step 5:** Add server-push and signalr to the trigger kinds reference table

### 4c. Add server-push and signalr to runtime.mdx

- [ ] **Step 6:** Read `runtime.mdx` and locate the trigger wiring section
- [ ] **Step 7:** Add server-push and signalr trigger runtime behavior (EventSource for SSE, HubConnection for SignalR)

### 4d. Add ServerPush/SignalR to the-builders.mdx

- [ ] **Step 8:** Read `the-builders.mdx` and locate the TriggerBuilder section
- [ ] **Step 9:** Add `ServerPush()` (3 overloads) and `SignalR()` (2 overloads) descriptions

- [ ] **Step 10:** Commit: `docs: add server-push/signalr to architecture pages`

---

## Task 5: Add NativeActionLink to Native Components Page (HIGH)

**Files:**
- Modify: `docs-site/src/content/docs/components/native-components.md`

**What's wrong:** NativeActionLink is listed in the API reference but has no entry in the native components guide page. Developers looking for "link that triggers a pipeline" won't find it.

- [ ] **Step 1:** Read the NativeActionLink source files to get builder API, events, extensions
- [ ] **Step 2:** Add NativeActionLink section to `native-components.md` after NativeButton, following the same format (property table, render example, reactive example, builder methods table):

```csharp
@(Html.NativeActionLink<MyModel>("View Resident", "/api/residents/42", pipeline =>
{
    pipeline.Element("status").SetText("Loading...");
}))
```

- [ ] **Step 3:** Commit: `docs: add NativeActionLink to native components page`

---

## Task 6: Update Test Counts (LOW)

**Files:**
- Modify: `docs-site/src/content/docs/testing/strategy.mdx`
- Modify: `docs-site/src/content/docs/reference/build-commands.md`

**What's wrong:** D2 diagram in strategy.mdx says "~500 C# tests" (actual ~510), final paragraph says "~1,900 tests" (actual ~1,937). build-commands.md says "~944 tests" for TS and "~483 tests" for Playwright — these are still approximately correct but could use a bump.

- [ ] **Step 1:** Update D2 diagram in strategy.mdx: "~500 tests" → "~510 tests"
- [ ] **Step 2:** Update final paragraph: "~1,900 tests" → "~1,900+ tests" (keep approximate)
- [ ] **Step 3:** Verify build-commands.md test counts are still in range (they are)
- [ ] **Step 4:** Commit: `docs: update test counts in testing strategy`

---

## Execution Order

| Order | Task | Priority | Est. size |
|-------|------|----------|-----------|
| 1 | Fix CLAUDE.md stale references | CRITICAL | Medium (many find/replace) |
| 2 | Add ServerPush/SignalR to API reference | CRITICAL | Small |
| 3 | Create Validation page | CRITICAL | Large (new page) |
| 4 | Update architecture pages | HIGH | Medium (5 files) |
| 5 | Add NativeActionLink | HIGH | Small |
| 6 | Update test counts | LOW | Trivial |
