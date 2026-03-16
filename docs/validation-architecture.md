# Validation Architecture

> Code-first documentation of the validation system. Describes how it works, not how it should work.

## Overview

Validation is request-chained: `Validate<TValidator>(formId)` or `Validate(descriptor)` attaches a validation descriptor to an HTTP request. Before the request fires, the runtime runs `validate(desc)`. If it returns `false`, the request is aborted. Server errors are displayed via the `validation-errors` command in `OnError` handlers.

**Key design:** Two-phase enrichment (C# at render + runtime at boot/merge) so partial-owned fields can be enriched when their partial loads.

---

## Data Flow

```
C# Authoring                          C# Render                         Runtime
────────────────────────────────────────────────────────────────────────────────────
Validate<TValidator>(formId)    →    ResolveAll()                    →   pipeline.ts
  placeholder descriptor               ExtractRules()                    passesValidation()
  AttachValidator(type)                EnrichValidation (if non-null)     wireLiveClearing()
                                       EnrichFieldsFromComponents         validate()
                                       StampPlanId

Validate(descriptor)            →    ResolveAll()                    →   (same)
  explicit descriptor                  EnrichFieldsFromComponents
  no ValidatorType                     StampPlanId
```

---

## C# Side

### Entry Points

| Method | Creates | File |
|--------|---------|------|
| `Validate<TValidator>(formId)` | `_validation = new ValidationDescriptor(formId, [])`, `_validatorType = typeof(TValidator)` | `HttpRequestBuilder.cs:93-98` |
| `Validate(ValidationDescriptor)` | Uses provided descriptor | `HttpRequestBuilder.cs:82-86` |

### ResolveAll (ReactivePlan.cs:95-116)

```
if (extractor != null)
  ValidationResolver.Resolve(_entries, extractor, _componentsMap)
else if (HasValidatorTypes)
  throw "no validation extractor registered"
else if (_componentsMap.Count > 0)
  ValidationResolver.EnrichFromComponents(_entries, _componentsMap)
always: StampPlanId(_entries, PlanId)
```

### ValidationResolver.ResolveRequest (ValidationResolver.cs:81-99)

1. **Extract:** If `ValidatorType` and `Validation` set → `extractor.ExtractRules()` → if non-null, `EnrichValidation(extracted)`.
2. **Enrich:** If `Validation` and `componentsMap` set → `EnrichFieldsFromComponents(req.Validation, componentsMap)`.
3. **Recurse:** If `Chained` → `ResolveRequest(req.Chained, ...)`.

### EnrichFieldsFromComponents (ValidationResolver.cs:138-151)

Maps `field.FieldName` → `componentsMap[fieldName]`:
- Found → set `field.FieldId`, `field.Vendor`, `field.ReadExpr`
- Not found → leave null (field stays unenriched)

### FluentValidationAdapter.ExtractRules

Returns `null` when:
- `_factory(validatorType)` returns null
- `fields.Count == 0` (no extractable rules, e.g. `EmptyValidator`)

---

## Runtime Side

### Boot & Merge

- **Boot:** `enrichEntries(plan.entries, plan.components)` enriches validation fields from `plan.components`.
- **Merge:** When a partial loads, `applyMergedPlan` runs `enrichEntries` again. Previously unenriched fields (e.g. from a partial that hadn't loaded) can now be enriched.

### Pre-Request Gate (pipeline.ts:10-18)

```typescript
function passesValidation(req): boolean {
  if (!req.validation) return true;
  wireLiveClearing(req.validation);
  if (!validate(req.validation)) return false;
  return true;
}
```

### validate() Orchestrator (validation/orchestrator.ts)

**Initialization:** Clear inline + summary, find summary element by `planId`.

**Form container missing:**
- `fields.length > 0` → return false (block)
- `fields.length === 0` → return true (nothing to validate)

**Per-field loop:**
- **Unenriched** (no fieldId/vendor/readExpr): If `allRulesConditionallySkipped` → skip. Else → add first rule to summary, block.
- **Enriched but element missing:** Same logic (component removed or partial unloaded).
- **Field outside form:** Skip (trace only).
- **Enriched + element present:** Read value via `resolveRoot` + `walk`, evaluate rules with conditions, route errors to inline or summary.

**allRulesConditionallySkipped:** Returns true only if every rule has `when` and `evalCondition` returns `false` for all. Otherwise block.

### Server Errors (validation-errors command)

- `ctx.validationDesc ?? { formId: cmd.formId, fields: [] }` — fallback when no descriptor in context.
- `showServerErrors(desc, ctx.responseBody)` — `extractErrors` accepts only ProblemDetails `{ errors: Record<string, string[]> }`; otherwise returns null and logs.

### Live Clear (validation/live-clear.ts)

One-time wiring on form container: `input` and `change` events. Matches `field.fieldId === target.id` to clear inline error for that field.

---

## Module Responsibilities

| Module | Responsibility |
|--------|----------------|
| `orchestrator.ts` | Fail-closed validation, routing inline vs summary |
| `rule-engine.ts` | Pure rule evaluation (required, minLength, equalTo, etc.) |
| `condition.ts` | Pure condition evaluation (truthy, falsy, eq, neq) |
| `error-display.ts` | DOM manipulation (inline spans, summary div) |
| `live-clear.ts` | One-time input/change wiring on form container |
| `enrichment.ts` | Enrich validation fields from `plan.components` |

---

## Rule Types (rule-engine.ts)

| Rule | Fail-closed behavior |
|------|----------------------|
| required, minLength, maxLength, email, regex, url, min, max, range | Standard checks |
| equalTo | `peerReader.readPeer() === undefined` → block |
| atLeastOne | Array length check |
| unknown | `return true` (block) |
| regex (broken) | `catch` → block |

---

## Condition Operators (condition.ts)

| Op | Returns |
|----|---------|
| truthy | `!empty` |
| falsy | `empty` |
| eq | `empty ? false : str === String(cond.value)` |
| neq | `empty ? false : str !== String(cond.value)` |
| source undefined | `null` (caller blocks) |

**Design:** For eq/neq, empty source returns false ("not yet determined") — rule is skipped. Tests: `when-evaluating-pure-rules.test.ts`.

---

## Design Decisions

1. **Two-phase enrichment** — C# enriches from ComponentsMap; runtime enriches from plan.components. Partials can add components after boot; merge re-enriches.
2. **Unenriched fields** — Block unless all rules conditionally skipped. Supports partial-owned fields that load later.
3. **Empty extraction** — `ExtractRules` returns null when no rules → placeholder stays → `validate()` iterates nothing → pass. Server is authoritative when client has no rules.
4. **ProblemDetails only** — Server errors must be `{ errors }` shape.
5. **planId scoping** — Summary div: `[data-alis-validation-summary="${planId}"]`. `findSummaryElement(undefined)` returns null (refuse to guess).

---

## Key Files

| Layer | File |
|-------|------|
| C# | `HttpRequestBuilder.cs`, `ValidationResolver.cs`, `FluentValidationAdapter.cs`, `ReactivePlan.cs` |
| Runtime | `pipeline.ts`, `validation/orchestrator.ts`, `validation/rule-engine.ts`, `validation/condition.ts`, `validation/error-display.ts`, `validation/live-clear.ts`, `enrichment.ts` |
| Commands | `commands.ts` (validation-errors), `http.ts` (threads validationDesc to error handlers) |

---

## Notes

- **Live-clear matching:** Uses `fieldId === target.id`. Native inputs have id on the element; wrapper-based components (e.g. TestWidget) may fire from an inner element without id. Worth verifying for real Fusion components.
- **ValidationErrors without descriptor:** Fallback `{ formId, fields: [] }` has no planId → no summary div. Server-only validation (no client Validate) cannot show summary errors.
- **Extraction null:** `claudeissue/008` argues `Validate<TValidator>` with extractor returning null should throw at render time. Current behavior: pass through to server.
