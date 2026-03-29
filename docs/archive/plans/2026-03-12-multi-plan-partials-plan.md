# Multi-Plan Partials Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Multiple ReactivePlan instances (parent views + partials) coexist on a page, merge at runtime by `planId`, share a unified `components` map for gather and validation — zero DOM scanning.

**Architecture:** Each view/partial renders its own `<script data-alis-plan>` element. JS runtime merges plans by `planId` (`typeof(TModel).FullName`), then boots. Gather `kind: "all"` and validation resolve component info from the merged `components` map at runtime, not at C# render time.

**Tech Stack:** C# (.NET 8, System.Text.Json), TypeScript (ESM, esbuild), Vitest, NUnit, Playwright, FluentValidation

**Design doc:** `docs/plans/2026-03-12-multi-plan-partials-design.md`

---

## Task 1: PlanId + ComponentsMap Serialization

Add `PlanId` to `ReactivePlan<T>` and serialize `ComponentsMap` into plan JSON.

**Files:**
- Modify: `Alis.Reactive/IReactivePlan.cs`

**Step 1: Add PlanId property to ReactivePlan**

In `Alis.Reactive/IReactivePlan.cs`, add to `ReactivePlan<TModel>`:

```csharp
public string PlanId { get; } = typeof(TModel).FullName!;
```

**Step 2: Change Render() to serialize planId + components + entries**

Currently (lines 66-70):
```csharp
public string Render()
{
    ResolveAll();
    return JsonSerializer.Serialize(new { entries = _entries }, CompactOptions);
}
```

Change to:
```csharp
public string Render()
{
    ResolveAll();
    return JsonSerializer.Serialize(new
    {
        planId = PlanId,
        components = SerializeComponentsMap(),
        entries = _entries
    }, CompactOptions);
}
```

And `RenderFormatted()` (lines 72-76) similarly.

**Step 3: Add SerializeComponentsMap helper**

```csharp
private Dictionary<string, object> SerializeComponentsMap()
{
    var result = new Dictionary<string, object>();
    foreach (var kvp in _componentsMap)
    {
        result[kvp.Key] = new
        {
            id = kvp.Value.ComponentId,
            vendor = kvp.Value.Vendor,
            readExpr = kvp.Value.ReadExpr
        };
    }
    return result;
}
```

**Step 4: Run C# unit tests**

```bash
dotnet test tests/Alis.Reactive.UnitTests
```

Expected: Many snapshot tests will fail because plan JSON shape changed (now has `planId` and `components` at root). That's expected — update `.verified.txt` files.

**Step 5: Accept snapshot updates**

```bash
# From repo root
dotnet test tests/Alis.Reactive.UnitTests -- --verify.accept
```

Or manually review and accept each `.received.txt` → `.verified.txt`.

**Step 6: Commit**

```bash
git add -A
git commit -m "feat: serialize planId + components in plan JSON"
```

---

## Task 2: AllGather Serialization + Delete GatherResolver

Make `AllGather` serialize as `{"kind":"all"}` instead of being expanded at C# time. Delete `GatherResolver.cs`.

**Files:**
- Modify: `Alis.Reactive/Descriptors/Requests/GatherItem.cs`
- Delete: `Alis.Reactive/Resolvers/GatherResolver.cs`
- Modify: `Alis.Reactive/IReactivePlan.cs` (remove GatherResolver call from ResolveAll)

**Step 1: Add JsonDerivedType for AllGather**

In `GatherItem.cs`, add to the `GatherItem` base class attributes (after line 7):

```csharp
[JsonDerivedType(typeof(AllGather), "all")]
```

So it becomes:
```csharp
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(ComponentGather), "component")]
[JsonDerivedType(typeof(StaticGather), "static")]
[JsonDerivedType(typeof(AllGather), "all")]
public abstract class GatherItem
```

**Step 2: Remove GatherResolver.Resolve call from ResolveAll()**

In `IReactivePlan.cs`, `ResolveAll()` currently (lines 78-84):
```csharp
private void ResolveAll()
{
    GatherResolver.Resolve(_entries, _componentsMap);

    if (_extractor != null)
        ValidationResolver.Resolve(_entries, _extractor, _buildContexts, _componentsMap);
}
```

Remove the `GatherResolver.Resolve` line. Keep the validation call for now (will be simplified in Task 5).

**Step 3: Delete GatherResolver.cs**

```bash
rm Alis.Reactive/Resolvers/GatherResolver.cs
```

Remove `using Alis.Reactive.Resolvers;` from `IReactivePlan.cs` if it was only for GatherResolver (check if ValidationResolver still needs it).

**Step 4: Run tests**

```bash
dotnet test tests/Alis.Reactive.UnitTests
```

Expected: `WhenGatheringRegisteredComponents` tests will fail — gather entries now show `{"kind":"all"}` instead of expanded `ComponentGather` items. Update snapshots.

**Step 5: Update WhenGatheringRegisteredComponents tests**

In `tests/Alis.Reactive.UnitTests/Requests/WhenGatheringRegisteredComponents.cs`:

- Tests that used `plan.RegisterComponent(...)` need to use `plan.AddToComponentsMap(bindingPath, new ComponentRegistration(...))` instead
- Snapshot `.verified.txt` files need updating — gather now contains `{"kind":"all"}` not expanded components
- The `No_registered_components_produces_empty_gather` test (line 59) needs updating — gather will contain `[{"kind":"all"}]` not `[]`, since the AllGather marker passes through. The test assertion about `gather.GetArrayLength() == 0` changes — it will be `1` (the all marker).

**Step 6: Accept snapshot updates + commit**

```bash
dotnet test tests/Alis.Reactive.UnitTests
git add -A
git commit -m "feat: AllGather serializes as kind:all, delete GatherResolver"
```

---

## Task 3: ValidatorType on RequestDescriptor + Delete RequestBuildContext

Eliminate the `_buildContexts` indirection. Store `ValidatorType` directly on `RequestDescriptor`.

**Files:**
- Modify: `Alis.Reactive/Descriptors/Requests/RequestDescriptor.cs`
- Delete: `Alis.Reactive/Descriptors/Requests/RequestBuildContext.cs`
- Modify: `Alis.Reactive/Builders/Requests/HttpRequestBuilder.cs`
- Modify: `Alis.Reactive/Builders/Requests/ResponseBuilder.cs`
- Modify: `Alis.Reactive/IReactivePlan.cs`

**Step 1: Add ValidatorType to RequestDescriptor**

In `RequestDescriptor.cs`, add:
```csharp
[JsonIgnore]
internal Type? ValidatorType { get; set; }
```

**Step 2: Simplify HttpRequestBuilder**

In `HttpRequestBuilder.cs`:
- In `BuildRequestDescriptor()` (line 112-130): After creating `desc`, set `desc.ValidatorType = _validatorType` directly. Remove `CollectBuildContext(desc)` and `MergeChainedContexts()` calls.
- Delete `BuildContexts` property (line 110)
- Delete `CollectBuildContext()` method (lines 132-139)
- Delete `MergeChainedContexts()` method (lines 141-149)

For chained requests: the chained `HttpRequestBuilder` also stores its own `_validatorType` on its own `RequestDescriptor`. Since chaining creates a nested `RequestDescriptor`, the `ValidatorType` is already on the right descriptor. No merge needed.

**Step 3: Simplify ResponseBuilder**

In `Alis.Reactive/Builders/Requests/ResponseBuilder.cs`:
- Delete `ChainedBuildContexts` property
- In `Chained()` method: remove the `ChainedBuildContexts = chainedBuilder.BuildContexts;` line

**Step 4: Remove RegisterBuildContexts from ReactivePlan**

In `IReactivePlan.cs`:
- Delete `_buildContexts` field (line 37)
- Delete `RegisterBuildContexts()` method (lines 59-64)
- Update `ResolveAll()` — the `ValidationResolver.Resolve` call no longer passes `_buildContexts` (will be fully updated in Task 5)

**Step 5: Delete RequestBuildContext.cs**

```bash
rm Alis.Reactive/Descriptors/Requests/RequestBuildContext.cs
```

**Step 6: Build + test**

```bash
dotnet build
dotnet test tests/Alis.Reactive.UnitTests
```

Fix any compile errors. The reactive extensions that called `RegisterBuildContexts` will fail — fix them in Task 6.

**Step 7: Commit**

```bash
git add -A
git commit -m "refactor: store ValidatorType on RequestDescriptor, delete RequestBuildContext"
```

---

## Task 4: Simplify ValidationField + IValidationExtractor + FluentValidationAdapter

Remove component resolution from validation extraction. ValidationField carries only `fieldName` + `rules`.

**Files:**
- Modify: `Alis.Reactive/Validation/ValidationField.cs`
- Modify: `Alis.Reactive/Validation/IValidationExtractor.cs`
- Modify: `Alis.Reactive.FluentValidator/FluentValidationAdapter.cs`

**Step 1: Simplify ValidationField**

In `ValidationField.cs`, remove `FieldId`, `Vendor`, `ReadExpr`. Keep `FieldName` and `Rules`:

```csharp
public sealed class ValidationField
{
    public string FieldName { get; }
    public List<ValidationRule> Rules { get; }

    public ValidationField(string fieldName, List<ValidationRule> rules)
    {
        FieldName = fieldName;
        Rules = rules;
    }
}
```

**Step 2: Simplify IValidationExtractor**

In `IValidationExtractor.cs`, remove `componentsMap` parameter:

```csharp
public interface IValidationExtractor
{
    ValidationDescriptor? ExtractRules(Type validatorType, string formId);
}
```

**Step 3: Simplify FluentValidationAdapter.ExtractRules**

In `FluentValidationAdapter.cs`:
- Remove `componentsMap` parameter from `ExtractRules` signature
- Remove the `componentsMap.TryGetValue` lookups (lines 58-67) — just use `propertyPath` as `FieldName`
- Create `ValidationField(propertyPath, rules)` — no `fieldId`, `vendor`, `readExpr`
- Simplify `FindOrCreateField` — remove `componentsMap` parameter, create field with just `propertyPath` + empty rules list
- Delete the `throw new InvalidOperationException` for missing component — that's now a runtime concern

New ExtractRules field-building loop:
```csharp
foreach (var kvp in fieldRules)
{
    var propertyPath = kvp.Key;
    var rules = new List<ValidationRule>();
    foreach (var er in kvp.Value)
    {
        rules.Add(new ValidationRule(er.Rule, er.Message, er.Constraint, er.When));
    }
    fields.Add(new ValidationField(propertyPath, rules));
}
```

New FindOrCreateField:
```csharp
private static ValidationField FindOrCreateField(List<ValidationField> fields, string propertyName)
{
    foreach (var f in fields)
    {
        if (f.FieldName == propertyName) return f;
    }
    var field = new ValidationField(propertyName, new List<ValidationRule>());
    fields.Add(field);
    return field;
}
```

**Step 4: Build**

```bash
dotnet build
```

Expected: Many compile errors — all callers of `ExtractRules` pass `componentsMap`. Fix in Task 5 (ValidationResolver) and Task 7 (tests).

**Step 5: Commit (may not compile yet — that's OK, Task 5 completes it)**

```bash
git add -A
git commit -m "refactor: simplify ValidationField — rules only, no component info"
```

---

## Task 5: Simplify ValidationResolver + ReactivePlan.Render()

Update the resolver to use `req.ValidatorType` directly and not pass `componentsMap`.

**Files:**
- Modify: `Alis.Reactive/Resolvers/ValidationResolver.cs`
- Modify: `Alis.Reactive/IReactivePlan.cs`

**Step 1: Simplify ValidationResolver**

Replace the entire file with:

```csharp
using System.Collections.Generic;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Reactions;
using Alis.Reactive.Descriptors.Requests;
using Alis.Reactive.Validation;

namespace Alis.Reactive.Resolvers
{
    internal static class ValidationResolver
    {
        internal static void Resolve(List<Entry> entries, IValidationExtractor extractor)
        {
            foreach (var entry in entries)
                ResolveReaction(entry.Reaction, extractor);
        }

        private static void ResolveReaction(Reaction reaction, IValidationExtractor extractor)
        {
            switch (reaction)
            {
                case HttpReaction hr:
                    ResolveRequest(hr.Request, extractor);
                    break;
                case ParallelHttpReaction phr:
                    foreach (var req in phr.Requests)
                        ResolveRequest(req, extractor);
                    break;
                case ConditionalReaction cr:
                    foreach (var branch in cr.Branches)
                        ResolveReaction(branch.Reaction, extractor);
                    break;
            }
        }

        private static void ResolveRequest(RequestDescriptor req, IValidationExtractor extractor)
        {
            if (req.ValidatorType != null && req.Validation != null)
            {
                var formId = req.Validation.FormId;
                var extracted = extractor.ExtractRules(req.ValidatorType, formId);
                if (extracted != null)
                    req.Validation = extracted;
            }

            if (req.Chained != null)
                ResolveRequest(req.Chained, extractor);
        }
    }
}
```

**Step 2: Update ResolveAll() in ReactivePlan**

```csharp
private void ResolveAll()
{
    if (_extractor != null)
        ValidationResolver.Resolve(_entries, _extractor);
}
```

**Step 3: Build + test**

```bash
dotnet build
dotnet test tests/Alis.Reactive.UnitTests
```

The FluentValidator tests will fail — they pass `componentsMap` to `ExtractRules`. Fix in Task 7.

**Step 4: Commit**

```bash
git add -A
git commit -m "refactor: simplify ValidationResolver — no buildContexts, no componentsMap"
```

---

## Task 6: Remove RegisterBuildContexts from Reactive Extensions

All 5 reactive extensions call `(plan as ReactivePlan<TModel>)?.RegisterBuildContexts(pb.BuildContexts)`. This method is deleted. Remove these calls.

**Files:**
- Modify: `Alis.Reactive.Native/Components/NativeTextBox/NativeTextBoxReactiveExtensions.cs`
- Modify: `Alis.Reactive.Native/Components/NativeCheckBox/NativeCheckBoxReactiveExtensions.cs`
- Modify: `Alis.Reactive.Native/Components/NativeDropDown/NativeDropDownReactiveExtensions.cs`
- Modify: `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxReactiveExtensions.cs`
- Modify: `Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownListReactiveExtensions.cs`

**Step 1: Remove RegisterBuildContexts calls**

In each file, find and delete the line:
```csharp
(plan as ReactivePlan<TModel>)?.RegisterBuildContexts(pb.BuildContexts);
```

Also delete the `plan.RegisterComponent(...)` lines in NativeCheckBox, NativeDropDown, FusionNumericTextBox, FusionDropDownList reactive extensions (these are the compile errors from the RegisterComponent removal earlier in this session).

**Step 2: Build + test**

```bash
dotnet build
dotnet test tests/Alis.Reactive.UnitTests
```

**Step 3: Commit**

```bash
git add -A
git commit -m "refactor: remove RegisterBuildContexts + RegisterComponent from reactive extensions"
```

---

## Task 7: Update FluentValidator Tests

All FluentValidator tests pass `componentsMap` as 3rd arg to `ExtractRules`. Remove it.

**Files:**
- Modify: `tests/Alis.Reactive.FluentValidator.UnitTests/TestModels.cs`
- Modify: All 11 test files in `tests/Alis.Reactive.FluentValidator.UnitTests/`

**Step 1: Remove TestComponentsMap helper**

In `TestModels.cs`, delete the `TestComponentsMap` class entirely — no longer needed.

**Step 2: Update all ExtractRules calls**

In every FluentValidator test file, change:
```csharp
_adapter.ExtractRules(typeof(SomeValidator), "form", _map)
```
To:
```csharp
_adapter.ExtractRules(typeof(SomeValidator), "form")
```

Remove the `_map` field from each test class.

**Step 3: Update snapshot assertions**

The `.verified.txt` files for these tests will change because `ValidationField` no longer has `fieldId`, `vendor`, `readExpr`. Accept the new snapshots.

**Step 4: Run tests**

```bash
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests
```

**Step 5: Commit**

```bash
git add -A
git commit -m "test: update FluentValidator tests — remove componentsMap from ExtractRules"
```

---

## Task 8: ResolvePlan Extension + RenderPlan data-model Attribute

Add `Html.ResolvePlan<T>()` and emit `data-model` on plan script tag.

**Files:**
- Modify: `Alis.Reactive.Native/Extensions/PlanExtensions.cs`

**Step 1: Add ResolvePlan<T> method**

```csharp
/// <summary>
/// Creates a ReactivePlan for a partial view that participates in a parent plan.
/// Same planId as ReactivePlan — runtime merges by planId.
/// </summary>
public static IReactivePlan<TModel> ResolvePlan<TModel>(this IHtmlHelper<TModel> html)
    where TModel : class
{
    var extractor = html.ViewContext.HttpContext.RequestServices
        .GetService<IValidationExtractor>();
    return new ReactivePlan<TModel>(extractor);
}
```

**Step 2: Add data-model attribute to RenderPlanCore**

Change `RenderPlanCore` to include `data-model`:
```csharp
private static IHtmlContent RenderPlanCore<TModel>(IReactivePlan<TModel> plan, string planName)
    where TModel : class
{
    var id = $"alis-plan-{planName}";
    var dataModel = typeof(TModel).FullName;
    var json = plan.Render();
    return new HtmlString(
        $"<script type=\"application/json\" id=\"{id}\" data-alis-plan data-model=\"{dataModel}\" data-trace=\"trace\">{json}</script>");
}
```

**Step 3: Build + test**

```bash
dotnet build
dotnet test tests/Alis.Reactive.UnitTests
```

**Step 4: Commit**

```bash
git add -A
git commit -m "feat: add ResolvePlan<T>() + data-model attribute on plan script tag"
```

---

## Task 9: JSON Schema Update

Update the plan schema to reflect the new structure.

**Files:**
- Modify: `Alis.Reactive/Schemas/reactive-plan.schema.json`

**Step 1: Add planId and components to root**

Add to the root `properties`:
```json
"planId": {
  "type": "string",
  "description": "typeof(TModel).FullName — runtime merge key"
},
"components": {
  "type": "object",
  "additionalProperties": { "$ref": "#/$defs/ComponentEntry" },
  "description": "Binding path → component registration. Shared by gather and validation."
}
```

Add `ComponentEntry` to `$defs`:
```json
"ComponentEntry": {
  "type": "object",
  "required": ["id", "vendor", "readExpr"],
  "properties": {
    "id": { "type": "string", "description": "DOM element ID" },
    "vendor": { "$ref": "#/$defs/Vendor" },
    "readExpr": { "type": "string", "description": "Property path from vendor root" }
  },
  "additionalProperties": false
}
```

**Step 2: Add AllGather to GatherItem**

In `GatherItem.oneOf`, add:
```json
{ "$ref": "#/$defs/AllGather" }
```

Add `AllGather` to `$defs`:
```json
"AllGather": {
  "type": "object",
  "required": ["kind"],
  "properties": {
    "kind": { "const": "all" }
  },
  "additionalProperties": false
}
```

**Step 3: Simplify ValidationField**

Remove `fieldId`, `vendor`, `readExpr` from `required` and `properties`. Keep `fieldName` and `rules`:
```json
"ValidationField": {
  "type": "object",
  "required": ["fieldName", "rules"],
  "properties": {
    "fieldName": { "type": "string" },
    "rules": {
      "type": "array",
      "items": { "$ref": "#/$defs/ValidationRule" }
    }
  },
  "additionalProperties": false
}
```

**Step 4: Run schema tests**

```bash
dotnet test tests/Alis.Reactive.UnitTests --filter Schema
```

**Step 5: Commit**

```bash
git add -A
git commit -m "schema: add planId, components, AllGather kind, simplify ValidationField"
```

---

## Task 10: TS Types + Auto-Boot Merge

Update TypeScript interfaces and implement plan merging by `planId`.

**Files:**
- Modify: `Alis.Reactive.SandboxApp/Scripts/types.ts`
- Modify: `Alis.Reactive.SandboxApp/Scripts/auto-boot.ts`
- Modify: `Alis.Reactive.SandboxApp/Scripts/boot.ts`

**Step 1: Update types.ts**

Add to `Plan`:
```typescript
export interface Plan {
  planId: string;
  components: Record<string, ComponentEntry>;
  entries: Entry[];
}

export interface ComponentEntry {
  id: string;
  vendor: Vendor;
  readExpr: string;
}
```

Add `AllGather` to `GatherItem`:
```typescript
export type GatherItem = ComponentGather | StaticGather | AllGather;

export interface AllGather {
  kind: "all";
}
```

Simplify `ValidationField`:
```typescript
export interface ValidationField {
  fieldName: string;
  rules: ValidationRule[];
  // Enriched at boot from plan.components:
  fieldId?: string;
  vendor?: Vendor;
  readExpr?: string;
}
```

**Step 2: Update auto-boot.ts — merge by planId**

Replace the entire file:
```typescript
import { boot, trace } from "./boot";
import { init as initConfirm } from "./confirm";
import type { Plan } from "./types";
import type { TraceLevel } from "./trace";

initConfirm();

const planEls = document.querySelectorAll<HTMLElement>("[data-alis-plan]");
const byPlanId = new Map<string, Plan>();

for (const el of planEls) {
  const traceLevel = el.getAttribute("data-trace") as TraceLevel | null;
  if (traceLevel) trace.setLevel(traceLevel);

  const raw: Plan = JSON.parse(el.textContent!);
  const key = raw.planId;

  if (byPlanId.has(key)) {
    const existing = byPlanId.get(key)!;
    Object.assign(existing.components, raw.components);
    existing.entries.push(...raw.entries);
  } else {
    byPlanId.set(key, raw);
  }
}

for (const plan of byPlanId.values()) {
  boot(plan);
}
```

**Step 3: Update boot.ts — enrich validation from components**

After the two-phase boot, add validation enrichment. In `boot.ts`, add a helper:

```typescript
function enrichValidation(plan: Plan): void {
  for (const entry of plan.entries) {
    enrichReaction(entry.reaction, plan.components);
  }
}
```

Walk the reaction tree to find `RequestDescriptor.validation` and enrich each field:

```typescript
import type { Plan, ComponentEntry, Reaction, ValidationDescriptor } from "./types";

function enrichValidationFields(
  desc: ValidationDescriptor,
  components: Record<string, ComponentEntry>
): void {
  for (const f of desc.fields) {
    const comp = components[f.fieldName];
    if (comp) {
      f.fieldId = comp.id;
      f.vendor = comp.vendor;
      f.readExpr = comp.readExpr;
    }
  }
}
```

Call `enrichValidation(plan)` before the two-phase boot in `boot()`.

**Step 4: Build TS**

```bash
npm run build
npm run typecheck
```

**Step 5: Commit**

```bash
git add -A
git commit -m "feat(ts): plan merge by planId, validation enrichment from components"
```

---

## Task 11: TS Gather — Handle kind: "all"

Update `gather.ts` to handle `AllGather` items using `plan.components`.

**Files:**
- Modify: `Alis.Reactive.SandboxApp/Scripts/gather.ts`
- Modify: `Alis.Reactive.SandboxApp/Scripts/http.ts`

**Step 1: Update resolveGather signature**

Add `components` parameter:
```typescript
import type { GatherItem, ComponentEntry } from "./types";

export function resolveGather(
  items: GatherItem[],
  verb: string,
  components: Record<string, ComponentEntry>,
  contentType?: string
): GatherResult {
```

**Step 2: Add kind: "all" case**

In the `switch (g.kind)` block, add:
```typescript
case "all": {
  for (const [bindingPath, comp] of Object.entries(components)) {
    if (!document.getElementById(comp.id)) {
      log.warn("gather target not found", { componentId: comp.id });
      continue;
    }
    const raw = evalRead(comp.id, comp.vendor, comp.readExpr);
    const value = raw === "" ? null : raw;
    if (isGet) {
      urlParams.push(`${encodeURIComponent(bindingPath)}=${encodeURIComponent(String(raw))}`);
    } else if (formData) {
      formData.append(bindingPath, String(raw ?? ""));
    } else {
      setNested(body, bindingPath, value);
    }
    log.trace("component", { name: bindingPath, value });
  }
  break;
}
```

**Step 3: Update http.ts to pass components**

In `http.ts`, wherever `resolveGather` is called, pass `plan.components`. The `plan` reference needs to be threaded through. Add `components` to `ExecContext` or pass it as a parameter to `execRequest`.

The simplest approach: add `components` to `ExecContext`:
```typescript
export interface ExecContext {
  evt?: Record<string, unknown>;
  responseBody?: unknown;
  validationDesc?: ValidationDescriptor;
  components?: Record<string, ComponentEntry>;
}
```

In `execRequest`:
```typescript
const gatherResult = resolveGather(req.gather ?? [], req.verb, ctx?.components ?? {}, req.contentType);
```

In `boot.ts`, when executing reactions, pass `components` via context:
```typescript
// In the dom-ready execution and trigger wiring
const ctx: ExecContext = { components: plan.components };
```

**Step 4: Build + run TS tests**

```bash
npm run build
npm test
```

**Step 5: Commit**

```bash
git add -A
git commit -m "feat(ts): gather handles kind:all from plan.components"
```

---

## Task 12: Update TS Unit Tests

Update vitest tests for new plan shape, AllGather, and validation enrichment.

**Files:**
- Modify: `Scripts/__tests__/` — multiple test files

**Step 1: Update plan fixtures**

All test plans need `planId` and `components` fields. Add to every plan fixture:
```typescript
const plan: Plan = {
  planId: "Test.Model",
  components: {},
  entries: [...]
};
```

**Step 2: Update gather tests**

Tests that use `ComponentGather` items keep working. Add new tests for `kind: "all"` that verify components map expansion.

**Step 3: Update validation tests**

Validation test fixtures need `ValidationField` without `fieldId`/`vendor`/`readExpr`. Verify boot-time enrichment populates them from `plan.components`.

**Step 4: Run tests**

```bash
npm test
```

**Step 5: Commit**

```bash
git add -A
git commit -m "test(ts): update vitest tests for multi-plan shape"
```

---

## Task 13: Update C# Unit Tests + Schema Tests

Fix all C# tests for new plan shape and gather behavior.

**Files:**
- Modify: `tests/Alis.Reactive.UnitTests/Requests/WhenGatheringRegisteredComponents.cs`
- Modify: All schema test verified files
- Modify: All snapshot `.verified.txt` files

**Step 1: Fix WhenGatheringRegisteredComponents**

- Replace `plan.RegisterComponent(...)` with `plan.AddToComponentsMap(bindingPath, new ComponentRegistration(...))`
- Update snapshot expectations — gather now shows `{"kind":"all"}` not expanded components
- Update `No_registered_components` test — gather will show `[{"kind":"all"}]`

**Step 2: Run all C# unit tests**

```bash
dotnet test tests/Alis.Reactive.UnitTests
```

Accept updated snapshots.

**Step 3: Run FluentValidator tests**

```bash
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests
```

**Step 4: Commit**

```bash
git add -A
git commit -m "test(cs): update unit tests for multi-plan shape + AllGather passthrough"
```

---

## Task 14: Full Build + Playwright Tests

Build everything, run all test layers.

**Files:** None (verification only)

**Step 1: Full build**

```bash
npm run build:all
dotnet build
```

**Step 2: Run TS tests**

```bash
npm test
```

**Step 3: Run C# unit tests**

```bash
dotnet test tests/Alis.Reactive.UnitTests
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests
```

**Step 4: Kill port + run Playwright tests**

```bash
lsof -ti:5220 | xargs kill -9 2>/dev/null; true
dotnet test tests/Alis.Reactive.PlaywrightTests
```

**Step 5: Fix any failures**

Playwright tests exercise the full stack. The validation tests will verify that the new plan shape (with `components` and simplified validation fields) works end-to-end with runtime enrichment and gather.

**Step 6: Final commit**

```bash
git add -A
git commit -m "test: all three test layers pass with multi-plan architecture"
```

---

## Task 15: Update _AddressPartial to Use ResolvePlan

Demonstrate the multi-plan pattern in the existing partial.

**Files:**
- Modify: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Validation/_AddressPartial.cshtml`

**Step 1: Update partial**

Change:
```csharp
var plan = new ReactivePlan<ValidationShowcaseModel>();
```
To:
```csharp
var plan = Html.ResolvePlan<ValidationShowcaseModel>();
```

Add at the bottom of the partial:
```csharp
@Html.RenderPlan(plan)
```

**Step 2: Run Playwright validation tests**

```bash
lsof -ti:5220 | xargs kill -9 2>/dev/null; true
dotnet test tests/Alis.Reactive.PlaywrightTests --filter Validation
```

Verify the partial's components appear in the merged plan and address fields work correctly.

**Step 3: Commit**

```bash
git add -A
git commit -m "feat: _AddressPartial uses ResolvePlan + RenderPlan — multi-plan demo"
```
