# Multi-Plan Partials Design

**Goal:** Multiple `ReactivePlan<T>` instances (parent views + partials) coexist on the same page, merge at runtime by `planId`, and share a unified `components` map for gather and validation — with zero DOM scanning.

**Architecture:** Each view/partial creates its own plan, renders its own `<script class="alis-plan">` element. The JS runtime discovers all plan elements, merges by `planId` (= `typeof(TModel).FullName`), then boots the merged plan. Gather and validation resolve component info from the merged `components` map at runtime, not at C# render time.

---

## 1. Plan Identity & Rendering

`ReactivePlan<T>` gets a `PlanId` derived from `typeof(TModel).FullName`.

Two extension methods:

- `Html.ReactivePlan<T>()` — existing, creates the main plan
- `Html.ResolvePlan<T>()` — new, creates a plan for partials (same planId, different instance)
- Both return `new ReactivePlan<T>()` with `PlanId = typeof(T).FullName`
- No shared state, no `HttpContext`, no `ViewData`

`Html.RenderPlan(plan)` — new extension, emits:

```html
<script type="application/json" class="alis-plan"
        data-model="MyApp.Models.ValidationShowcaseModel">
  { "planId": "...", "components": {...}, "entries": [...] }
</script>
```

Every view/partial: `ResolvePlan<T>()` at top, `@Html.RenderPlan(plan)` at bottom. Same code regardless of server-side or AJAX context.

## 2. ComponentsMap Serialization

Currently `ComponentsMap` is C#-only (used by resolvers at render time, never serialized). It must now serialize into the plan JSON as `components`:

```json
{
  "planId": "MyApp.Models.ValidationShowcaseModel",
  "components": {
    "AllRules.Name": { "id": "allrules-name", "vendor": "native", "readExpr": "value" },
    "Nested.Address.Street": { "id": "nested-address-street", "vendor": "native", "readExpr": "value" }
  },
  "entries": [...]
}
```

Each mini plan carries only its own components. Runtime merges by `planId`.

## 3. Gather — Runtime Resolution, Zero DOM Scanning

`AllGather` marker survives into the plan JSON (currently expanded by `GatherResolver` at C# time — that resolver is deleted).

Plan JSON:

```json
"gather": [
  { "kind": "all" },
  { "kind": "static", "param": "extra", "value": "fixed" }
]
```

Runtime gather resolution iterates `plan.components` — explicit `id`, explicit `vendor`, explicit `readExpr`:

```typescript
if (g.kind === "all") {
  for (const [path, comp] of Object.entries(plan.components)) {
    const el = document.getElementById(comp.id);
    const root = resolveRoot(el, comp.vendor);
    payload[path] = walk(root, comp.readExpr);
  }
}
```

**Guarantee:** Every component read goes through `plan.components`. The runtime never calls `querySelectorAll`, never inspects `el.type`, never guesses `readExpr`. If it's not in `plan.components`, it doesn't exist.

## 4. Validation — Runtime Component Resolution

Validation fields carry rules only. Runtime resolves component info from merged `plan.components`.

Plan JSON:

```json
"validation": {
  "formId": "allRulesForm",
  "fields": [
    {
      "fieldName": "AllRules.Name",
      "rules": [{ "rule": "required", "message": "Name is required." }]
    }
  ]
}
```

No `fieldId`, no `vendor`, no `readExpr` in the validation descriptor. Runtime enriches validation fields from `plan.components` at boot time:

```typescript
for (const f of desc.fields) {
  const comp = plan.components[f.fieldName];
  if (comp) {
    f.fieldId = comp.id;
    f.vendor = comp.vendor;
    f.readExpr = comp.readExpr;
  }
}
```

After enrichment, `validation.ts` is unchanged — still reads `f.fieldId`, `f.vendor`, `f.readExpr`.

C# side: `IValidationExtractor.ExtractRules` simplified to `(Type validatorType, string formId)` — no `componentsMap` parameter. `FluentValidationAdapter` extracts rules only, no component lookup.

## 5. Dead Code Removal

### Deleted (no role in new design)

| File / Member | Reason |
|---------------|--------|
| `GatherResolver.cs` | `AllGather` passes through to JSON. No C#-side expansion. |
| `RequestBuildContext.cs` | Indirection eliminated. `ValidatorType` moves to `RequestDescriptor` as `[JsonIgnore]`. |
| `ReactivePlan._buildContexts` | Existed for `RequestBuildContext` mapping. |
| `ReactivePlan.RegisterBuildContexts()` | Called by reactive extensions for build contexts. |
| `HttpRequestBuilder.BuildContexts` | Gone with `RequestBuildContext`. |
| `HttpRequestBuilder.CollectBuildContext()` | Gone. |
| `HttpRequestBuilder.MergeChainedContexts()` | Gone. |
| `ResponseBuilder.ChainedBuildContexts` | Gone. |
| Legacy `#alis-plan` fallback in `auto-boot.ts` | All views use `@Html.RenderPlan()` which emits `class="alis-plan"`. |

### Survives but Changes

| What | Current | New |
|------|---------|-----|
| `ValidationResolver.cs` | Walks tree, uses `_buildContexts` + `componentsMap` | Walks tree, reads `req.ValidatorType` directly, no `componentsMap` |
| `RequestDescriptor` | No validator type knowledge | Adds `[JsonIgnore] internal Type? ValidatorType` |
| `IValidationExtractor.ExtractRules` | `(Type, string, IReadOnlyDictionary)` | `(Type, string)` |
| `FluentValidationAdapter` | Looks up vendor/readExpr/fieldId from componentsMap | Extracts fieldName + rules only |
| `ValidationField` (C#) | `FieldId`, `FieldName`, `Vendor`, `ReadExpr`, `Rules` | `FieldName`, `Rules` only |
| `AllGather` (C#) | No `[JsonDerivedType]` — never serialized | Add `[JsonDerivedType(typeof(AllGather), "all")]` — serializes as `{"kind":"all"}` |
| `ReactivePlan.Render()` | `ResolveAll()` → serialize `{ entries }` | `ResolveValidation()` → serialize `{ planId, components, entries }` |
| `ReactivePlan.ResolveAll()` | Calls `GatherResolver` + `ValidationResolver` | Renamed `ResolveValidation()` — only calls simplified `ValidationResolver` |

## 6. TS Runtime Changes

| File | Change |
|------|--------|
| `types.ts` | `Plan` gets `planId`, `components`. `GatherItem` union gets `AllGather` (`kind: "all"`). `ValidationField` loses `fieldId`, `vendor`, `readExpr`. |
| `auto-boot.ts` | Merge plans by `planId` before booting. Delete `#alis-plan` fallback. |
| `boot.ts` | `boot()` receives merged `Plan` with `components`. Enriches validation fields from `components`. |
| `gather.ts` | New `kind: "all"` case — iterates `plan.components`. |
| `validation.ts` | Unchanged after boot-time enrichment. |
| `http.ts` | Passes `plan.components` to `resolveGather()` for `kind: "all"` expansion. |

### Unchanged

`component.ts`, `walk.ts`, `trigger.ts`, `resolver.ts`, `element.ts`, all commands, `confirm.ts`.

## 7. Field Name as Universal Key

`fieldName` is the join key between validation rules and the components map. Both sides use the same full nested dot-path.

Components map — keyed by binding path from `ExpressionPathHelper`:

```
m => m.Nested!.Address!.Street  →  "Nested.Address.Street"
```

Validation rules — keyed by FluentValidation's prefix-accumulated path:

```
ValidationShowcaseValidator
  → NestedSectionValidator (prefix: "Nested")
    → ValidationAddressValidator (prefix: "Nested.Address")
      → RuleFor(x => x.Street) → fieldName: "Nested.Address.Street"
```

IdGenerator produces: `ModelScope__Nested_Address_Street`. The `__` separator keeps model scope separate from property path. After `__`, replace `_` with `.` to recover the binding path.

**Rule:** Partials use the same `TModel` as the parent. One TModel = one validator = one payload shape. The `__` pattern inherently aligns component IDs with validation field names because both derive from the same model's expression tree.

## 8. Multi-Plan Merge Rules

| Scenario | planId | Behavior |
|----------|--------|----------|
| Server-side partial (`Html.PartialAsync`) — same TModel | Same | Plans merge. Shared components, shared validation/gather. |
| AJAX partial (controller action) — same TModel | Same | Plan injected via `into` command. Runtime merges. |
| AJAX partial — different TModel | Different | Independent plan. Own request chain, own validation, own gather. |
| Editor template (`Html.EditorFor`) — same TModel | Same | Plans merge. |
| View Component (`Component.InvokeAsync`) | N/A | Read-only. No plan needed. |

**One form per TModel** — enforced by design. No two forms share a planId.

## 9. AJAX Partial Injection

No MutationObserver. The `into` command already handles HTML injection. When injected HTML contains a `<script class="alis-plan">`, the `into` handler:

1. Scans injected content for `.alis-plan` elements
2. Parses plan JSON
3. Merges `components` and `entries` into the existing boot by `planId`
4. Re-wires new triggers (two-phase boot for new entries)

If a partial is removed and re-loaded, the old plan element is gone (inside replaced container). The new plan element arrives and merges cleanly.

## 10. Edge Cases

**Component removed from DOM:** `document.getElementById(comp.id)` returns null. Gather skips. Validation skips. Correct — missing element means "not on page."

**`IncludeAll` has no form scope:** Gathers ALL components in the merged plan. If needed in future, `IncludeByForm("formId")` scopes to a specific form container. Not needed now — one form per TModel.

**Validation rule for field not yet loaded (AJAX):** Runtime skips — `plan.components[fieldName]` returns undefined. When partial loads and merges, the component appears and validation works.

**No fallbacks:** If a component is expected but not found in `plan.components` after merge, that's a developer error (forgot to use plan-aware builder). Runtime logs a warning, does not guess.
