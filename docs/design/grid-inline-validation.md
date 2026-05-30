# Validation × Syncfusion Grid Inline Editing

Status: research + evaluation (pick-up doc). No code change yet.
Date: 2026-05-30.

This captures (1) why `ReactiveValidator` client validation cannot bind to a
Syncfusion grid **in-cell** editor today, and (2) an evaluation of the proposed
bridge: **generate Syncfusion-native column validation rules from our existing
FluentValidation metadata**, so the DSL source stays single and we get
client-side in-cell validation "for free".

---

## 1. How our client validation binds (today)

Recorded at **C# render time**, resolved in the browser by **`getElementById`
only**:

1. `ReactiveValidator<T>.ClientRule(m => m.Field)` records browser rule metadata
   into a `ClientValidationRuleSet`, keyed by model property path. Each
   `ClientValidationField` carries field path + `Shape` + `ValidationRule[]` +
   activation (`Always` or a `WhenField` condition). Server-only rules
   (`RuleFor`, `When/Unless`, `MustAsync`) are **not** recorded as client rules.
   (`ReactiveValidator.cs`, `ClientValidationFieldRuleBuilder.cs`)
2. A request opts in with `.Validate<TValidator>(formId)` — it sets the request's
   validation target to the form container and registers a `ValidationJob`.
3. At the **end of `Render()`**, `ClientValidationRuleBinder.BindQueuedJobs()`
   maps each field path to a concrete element id: the rendered `Html.InputField`'s
   `ComponentId`, else the deterministic `IdGenerator.For(modelType, path)`. The
   result — `{ component: elementId, value, rules, serverFieldName }` — is merged
   onto the form container's `ContainerScope` and serialized into the plan.
4. `Html.InputField` also emits the inline `<span id="{id}_error" data-valmsg-for>`
   slot and a per-plan summary div.
5. Runtime: at **boot**, `wireContainerValidation` wires `input`/`blur`/`change`
   per field by `getElementById` of the fixed id. On submit, `validateContainer`
   re-resolves each field by id and runs the rules; `ValidationErrors(formId)`
   shows inline (`{id}_error`) or routes to the summary.

**Binding key: fixed element IDs known at render time, resolved by `getElementById`.**

## 2. Why in-cell grid editing can't use it

> The validator has one id; the grid has one editor **per row**.

1. **No plan element id.** A cell editor is not an `Html.InputField`. Our
   `FusionGridEditTemplates.Select/DateInput` emit `<select name="careLevel">` /
   `<input type="date" name="nextReview">` — Syncfusion identifies them by
   `name="{field}"` (a per-row field name), not a stable, model-expression id.
   No `ComponentRegistration` exists, so the binder has nothing to map and
   `getElementById` has no id to find. Built-in editors (`numericedit`) are
   worse — SF mints an auto id at edit time.
2. **No `{id}_error` slot and no container scope.** Only `InputField`/`Field`
   emit the inline error span; the grid is a *display* component, so
   `.Validate<T>()` never targets it and no rules attach to it.
3. **Transient DOM.** Cell editors are created on edit and destroyed on save;
   they don't exist at boot when validation wires, and they live outside any
   validated form container, so `container.contains()` and the boot-time
   `getElementById` wiring miss them.

## 3. Why the dialog *does* work

The Billing/CareOps admit dialog uses real `Html.InputField` inputs in a standing
form with a summary slot. Each input registers a render-time `ComponentId`,
emits its `{id}_error` span, and the form carries a `ContainerScope`. So
`.Validate<TValidator>(formId)` resolves every field by id and maps server-side
400 errors back by field name. This is the surface the model is designed for.

## 4. Integration options (verified)

| Option | Feasible | Hack | Verdict |
|---|---|---|---|
| **Server validate-on-save** (`BeforeBatchSave`/`cellSave` → gather → POST → 400 → `args.Cancel` + summary) | ✅ | ❌ | Native in-cell path; FluentValidation stays the single authority |
| `actionBegin`/`cellSave` + `args.Cancel` client guard | ✅ | ❌ | A hand-authored `When` guard per cell — not `ReactiveValidator` metadata |
| `EditTemplate` hosts `Html.InputField` | ❌ | ✅ | One id vs one-editor-per-row collides; violates no-fallback-id; lifecycle breaks id lookup. **Forbidden** |
| SF native column rules (hand-declared) | ✅ | ❌ | Idiomatic SF, but a *second* hand-written engine → FluentValidation no longer single authority (**drift**) |
| Validate standing form → update row | ✅ | ❌ | Cleanest; edits *outside* the cell (the dialog pattern) |

---

## 5. Evaluation — emit SF-native column rules **from** our FluentValidation metadata

The "SF native column rules" option above is only a hack/drift risk **when
hand-declared twice**. If instead we **generate** `column.validationRules`
from the *same* `ReactiveValidator` metadata we already capture, the source
stays single (`ClientRule` once), there is no second authority to drift, and
Syncfusion's own `FormValidator` does the in-cell client validation for free.

### What we already have
`ClientValidationFieldRuleBuilder` records, per field, typed rules with messages:
`Required, Empty, Email, Url, CreditCard, AtLeastOne, MinLength, MaxLength,
Regex, Range, ExclusiveRange, Min, Max, Gt, Lt, EqualTo, NotEqual` (literal and
peer-field), each with `Shape` and `Always`/`When` activation. This is exposed
through `IClientValidationRuleSource` (DI) — the same source the binder reads.

### Mapping our rule names → EJ2 `column.validationRules`
EJ2 grid columns accept a `validationRules` object (EJ2 `FormValidator` rules),
with messages as `{ rule: [value, "message"] }`:

| Our `ValidationRuleName` | EJ2 rule | Notes |
|---|---|---|
| `Required` | `required: [true, msg]` | direct |
| `MinLength` | `minLength: [n, msg]` | direct |
| `MaxLength` | `maxLength: [n, msg]` | direct |
| `Range` | `range: [[lo, hi], msg]` | numeric range |
| `Min` / `GreaterThanOrEqualTo` | `min: [n, msg]` | direct |
| `Max` / `LessThanOrEqualTo` | `max: [n, msg]` | direct |
| `Email` | `email: [true, msg]` | direct |
| `Url` | `url: [true, msg]` | direct |
| `Regex` | `regex: [pattern, msg]` | direct |
| `EqualTo` (peer) | `equalTo: ['otherFieldId', msg]` | peer = another cell in the **same row** — needs same-row field name; usable but row-scoped |
| `Gt`/`Lt`/`ExclusiveRange`/`NotEqual` | custom rule fn | EJ2 has no built-in; emit a small custom validator |
| `CreditCard`/`AtLeastOne`/`Empty` | — | no native equivalent; leave to server |
| `When(...)` activation | — | EJ2 column rules are unconditional; conditional rules can't map |

**Coverage:** the common single-field rules (`Required`, lengths, `Range`,
`Min`/`Max`, `Email`/`Url`, `Regex`) map **directly** and cover the large
majority of real field validation. Cross-field, conditional (`WhenField`), and a
few exotic rules do **not** map — those stay server-authoritative.

### Proposed design (no hack, single source)
1. **C# emitter** in the FusionGrid slice, e.g.
   `column.ValidationFrom<TValidator, TRow>(r => r.CareLevel)` — resolves the
   `ClientValidationField` for that model path from `IClientValidationRuleSource`,
   maps each unconditional rule to the EJ2 rule key/value (+ message), and sets
   `GridColumn.ValidationRules` to the emitted object. Conditional rules are
   skipped (logged) and deferred to the server.
2. **Syncfusion runs it natively** — EJ2 `editModule` validates the cell against
   `column.validationRules` on cell save/blur, showing its own inline tooltip.
   Zero new runtime TS; zero new validation engine; the grid stays a typed slice.
3. **Server stays the authority for the rest** — on `BeforeBatchSave`, gather the
   batch and POST to a controller that runs the *full* FluentValidation
   (`TryValidate`); on 400, `args.Cancel` the commit and surface messages in the
   plan validation summary (the existing dialog error contract). This covers
   cross-field/conditional/`MustAsync`/server-only rules that can't live client-side.

### Net result
- **Source stays the same**: developer writes `ReactiveValidator<T>` once.
- **Client-side in-cell validation "for free"**: generated from the metadata into
  SF's native column rules — no second hand-written ruleset, so no drift.
- **No hack**: no per-row id minting, no `InputField`-in-`EditTemplate`, no
  rebuilt rules — we translate the *one* truth into SF's native shape.
- **Honest gaps**: conditional + cross-field + exotic + async rules fall through
  to server-on-save, which is the correct authority for them anyway.

### Risks / open questions
- Exact EJ2 message syntax + custom-rule registration for `Gt`/`Lt`/exclusive —
  confirm against EJ2 32.x `FormValidator` during implementation.
- Peer (`EqualTo` other-field) is row-scoped in a grid — map to the same-row
  field name; verify EJ2 `equalTo` resolves within the edit form.
- `Shape`/culture: numeric vs date `range` must serialize in the editor's culture.
- Decide the trigger: explicit `column.ValidationFrom(...)` (typed, opt-in) vs
  auto-derive from the bound `TRow` validator. Opt-in is safer and more typed.

## 6. Recommendation
1. **Now**: in-cell editing → **server validate-on-save** (`BeforeBatchSave` →
   gather → POST → 400 → `args.Cancel` + summary). Rich client feedback → the
   **templated dialog / standing form** (already in Billing/CareOps). Do **not**
   host `InputField` in a string `EditTemplate` or hand-declare native rules.
2. **Next (recommended feature)**: build the **metadata→EJ2 emitter**
   (`column.ValidationFrom<TValidator, TRow>(...)`) so the common single-field
   rules are generated from FluentValidation into SF-native column rules — single
   source, client-side free — with server-on-save as the authority for the gaps.
