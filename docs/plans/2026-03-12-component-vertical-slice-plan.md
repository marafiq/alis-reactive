# Component Vertical Slice Architecture — Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Eliminate reflection, unify vendor root resolution, fix broken source bindings, make ComponentsMap the single source of truth, and deliver complete FusionNumericTextBox + FusionDropDownList vertical slices.

**Architecture:** Components declare identity via instance interface properties (`Vendor`, `ReadExpr`) — C# 8.0 compatible, no `static abstract`. `ComponentRef<TComponent>` uses `new()` constraint with cached instance. The plan's ComponentsMap is populated at builder creation time. MutateElementCommand carries vendor for root resolution. BindSource is structured (EventSource + ComponentSource) everywhere — commands, guards, and conditions use the same pipeline. `TypedComponentSource<TProp>` preserves compile-time typed conditions.

**Tech Stack:** C# (.NET 8, System.Text.Json), TypeScript (Vitest, jsdom), Playwright, NUnit + Verify, JSON Schema 2020-12

**Design doc:** `docs/plans/2026-03-12-component-vertical-slice-design.md`

---

## Phase 1: Core Infrastructure (Tasks 1-6)

These tasks change the foundational types. Everything else depends on them.

---

### Task 1: IComponent.Vendor + IInputComponent.ReadExpr — Replace Reflection with Instance Interface Properties

**Files:**
- Modify: `Alis.Reactive/IComponent.cs`
- Modify: `Alis.Reactive.Native/Components/NativeCheckBox/NativeCheckBox.cs`
- Modify: `Alis.Reactive.Native/Components/NativeDropDown/NativeDropDown.cs`
- Modify: `Alis.Reactive.Native/Components/NativeButton/NativeButton.cs`
- Modify: `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBox.cs`
- Modify: `Alis.Reactive.Fusion/Components/TestWidgetSyncFusion/TestWidgetSyncFusion.cs`
- Modify: `Alis.Reactive.Fusion/AppLevel/FusionConfirm/FusionConfirm.cs`
- Test: `dotnet build` (compile check)

**Step 1: Modify IComponent.cs**

Replace `IReadableComponent`, `ReadExprAttribute`, and `ComponentHelper` with interface contracts (C# 8.0 — instance properties, not `static abstract`):

```csharp
public interface IComponent
{
    string Vendor { get; }
}

public interface IInputComponent : IComponent
{
    string ReadExpr { get; }
}

public interface IAppLevelComponent : IComponent
{
    string DefaultId { get; }
}

// DELETE: ReadExprAttribute class (entire [AttributeUsage] block)
// DELETE: ComponentHelper class (entire static class)
// DELETE: IReadableComponent interface
```

Keep `FusionComponent` and `NativeComponent` base classes for project organization but they no longer serve as vendor markers — each component declares vendor directly.

**Step 2: Update all component classes**

Each component adds instance `Vendor` and (for input components) `ReadExpr` properties:

```csharp
// FusionNumericTextBox.cs
public sealed class FusionNumericTextBox : FusionComponent, IInputComponent
{
    public string Vendor => "fusion";
    public string ReadExpr => "value";
}

// NativeCheckBox.cs
public sealed class NativeCheckBox : NativeComponent, IInputComponent
{
    public string Vendor => "native";
    public string ReadExpr => "checked";
}

// NativeDropDown.cs
public sealed class NativeDropDown : NativeComponent, IInputComponent
{
    public string Vendor => "native";
    public string ReadExpr => "value";
}

// NativeButton.cs (NOT IInputComponent — no form value)
public sealed class NativeButton : NativeComponent, IComponent
{
    public string Vendor => "native";
}

// TestWidgetSyncFusion.cs
public sealed class TestWidgetSyncFusion : FusionComponent, IInputComponent
{
    public string Vendor => "fusion";
    public string ReadExpr => "value";
}

// FusionConfirm.cs — check if it implements IComponent or IAppLevelComponent
// Add: public string Vendor => "fusion";
```

Also check and update `TestWidgetNative.cs` if it exists.

**Step 3: Replace all ComponentHelper.GetReadExpr<T>() calls**

Find all ~11 call sites and replace with instance access via `new TComponent().ReadExpr`:

- `Alis.Reactive.Fusion/Extensions/FusionGatherExtensions.cs` — 2 calls
- `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxReactiveExtensions.cs` — 2 calls
- `Alis.Reactive.Native/Extensions/NativeGatherExtensions.cs` — 2 calls
- `Alis.Reactive.Native/Components/NativeDropDown/NativeDropDownReactiveExtensions.cs` — 2 calls
- `Alis.Reactive.Native/Components/NativeCheckBox/NativeCheckBoxReactiveExtensions.cs` — 1 call

Pattern: `ComponentHelper.GetReadExpr<TComponent>()` → `new TComponent().ReadExpr`
(or use a cached static instance if method is called frequently)

For gather extensions that have `where TComponent : IReadableComponent`, change to `where TComponent : IInputComponent, new()`.

**Step 4: Build and verify**

Run: `dotnet build`
Expected: All projects compile. Zero reflection for component metadata.

**Step 5: Run existing tests**

Run: `dotnet test tests/Alis.Reactive.UnitTests && dotnet test tests/Alis.Reactive.Native.UnitTests && dotnet test tests/Alis.Reactive.Fusion.UnitTests`
Expected: All pass (behavior unchanged, only mechanism changed).

**Step 6: Commit**

```bash
git add -A && git commit -m "refactor: replace ReadExprAttribute reflection with instance interface properties

IComponent.Vendor and IInputComponent.ReadExpr enforce contracts via C# 8.0
instance properties + new() constraint. Delete ReadExprAttribute, ComponentHelper,
IReadableComponent."
```

---

### Task 2: ComponentSource + BindSource Extension

**Files:**
- Modify: `Alis.Reactive/Builders/Conditions/BindSource.cs`
- Create: `Alis.Reactive/Builders/Conditions/TypedComponentSource.cs`
- Modify: `Alis.Reactive/Schemas/reactive-plan.schema.json`
- Modify: `Alis.Reactive.SandboxApp/Scripts/types.ts`
- Test: `dotnet build && npm run typecheck`

**Step 1: Add ComponentSource to BindSource.cs**

```csharp
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(EventSource), "event")]
[JsonDerivedType(typeof(ComponentSource), "component")]
public abstract class BindSource { }

public sealed class EventSource : BindSource
{
    public string Path { get; }
    public EventSource(string path) { Path = path; }
}

public sealed class ComponentSource : BindSource
{
    public string ComponentId { get; }
    public string Vendor { get; }
    public string ReadExpr { get; }
    public ComponentSource(string componentId, string vendor, string readExpr)
    {
        ComponentId = componentId;
        Vendor = vendor;
        ReadExpr = readExpr;
    }
}
```

**Step 2: Create TypedComponentSource.cs**

```csharp
namespace Alis.Reactive.Builders.Conditions;

public sealed class TypedComponentSource<TProp> : TypedSource<TProp>
{
    private readonly string _componentId;
    private readonly string _vendor;
    private readonly string _readExpr;

    public TypedComponentSource(string componentId, string vendor, string readExpr)
    {
        _componentId = componentId;
        _vendor = vendor;
        _readExpr = readExpr;
    }

    public override BindSource ToBindSource()
        => new ComponentSource(_componentId, _vendor, _readExpr);
}
```

**Step 3: Update JSON schema — add ComponentSource to BindSource oneOf**

In `reactive-plan.schema.json`, find the `BindSource` definition and add `ComponentSource`:

```json
"BindSource": {
    "oneOf": [
        { "$ref": "#/$defs/EventSource" },
        { "$ref": "#/$defs/ComponentSource" }
    ]
},
"ComponentSource": {
    "type": "object",
    "required": ["kind", "componentId", "vendor", "readExpr"],
    "additionalProperties": false,
    "properties": {
        "kind": { "const": "component" },
        "componentId": { "type": "string", "minLength": 1 },
        "vendor": { "$ref": "#/$defs/Vendor" },
        "readExpr": { "type": "string", "minLength": 1 }
    }
}
```

**Step 4: Update types.ts — add ComponentSource**

```typescript
export type BindSource = EventSource | ComponentSource;

export interface EventSource {
    kind: "event";
    path: string;
}

export interface ComponentSource {
    kind: "component";
    componentId: string;
    vendor: Vendor;
    readExpr: string;
}
```

**Step 5: Build and typecheck**

Run: `dotnet build && npm run typecheck`
Expected: Compiles. No test changes yet — these types are additive.

**Step 6: Commit**

```bash
git add -A && git commit -m "feat: add ComponentSource to BindSource + TypedComponentSource<TProp>

Structured source for reading component values. TypedComponentSource
preserves typed condition pipeline alongside EventArgSource."
```

---

### Task 3: MutateElementCommand — Add Vendor + Change Source to BindSource

**Files:**
- Modify: `Alis.Reactive/Descriptors/Commands/Command.cs`
- Modify: `Alis.Reactive/ComponentRef.cs`
- Modify: `Alis.Reactive/Builders/ElementBuilder.cs`
- Modify: `Alis.Reactive/Schemas/reactive-plan.schema.json`
- Modify: `Alis.Reactive.SandboxApp/Scripts/types.ts`
- Test: `dotnet build && npm run typecheck`

**Step 1: Modify MutateElementCommand in Command.cs**

```csharp
public sealed class MutateElementCommand : Command
{
    public string Target { get; }
    public string JsEmit { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Value { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public BindSource? Source { get; }    // CHANGED: was string?

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Vendor { get; }        // NEW

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guard? When { get; }

    public MutateElementCommand(
        string target,
        string jsEmit,
        string? value = null,
        BindSource? source = null,        // CHANGED: was string?
        string? vendor = null,            // NEW
        Guard? when = null)
    {
        Target = target;
        JsEmit = jsEmit;
        Value = value;
        Source = source;
        Vendor = vendor;
        When = when;
    }
}
```

**Step 2: Update ComponentRef.cs — carry vendor, pass to command**

```csharp
public class ComponentRef<TComponent, TModel>
    where TComponent : IComponent, new()
    where TModel : class
{
    private static readonly TComponent _instance = new TComponent();

    internal string TargetId { get; }
    internal PipelineBuilder<TModel> Pipeline { get; }

    internal ComponentRef(string targetId, PipelineBuilder<TModel> pipeline)
    {
        TargetId = targetId;
        Pipeline = pipeline;
    }

    internal ComponentRef<TComponent, TModel> Emit(
        string jsEmit,
        string? value = null,
        BindSource? source = null)
    {
        Pipeline.AddCommand(new MutateElementCommand(
            TargetId, jsEmit, value, source, vendor: _instance.Vendor));
        return this;
    }

    public TypedComponentSource<TProp> ReadProperty<TProp>(string property)
        => new TypedComponentSource<TProp>(TargetId, _instance.Vendor, property);
}
```

C# 8.0 pattern: `new()` constraint + `static readonly` cached instance reads Vendor at type-load time. Expression-bodied properties return constants — no allocation overhead per call.

Note: `ReadProperty<TProp>` is generic to carry the property type for conditions.

**Step 3: Update ElementBuilder.cs — wrap source strings in EventSource**

Every place that passes a source string path to MutateElementCommand, wrap it:

```csharp
// Before:
new MutateElementCommand(_targetId, "el.textContent = val", source: sourcePath)

// After:
new MutateElementCommand(_targetId, "el.textContent = val", source: new EventSource(sourcePath))
```

Also add overloads that accept `BindSource` directly:

```csharp
public ElementBuilder<TModel> SetText(BindSource source)
{
    _pipeline.AddCommand(new MutateElementCommand(
        _targetId, "el.textContent = val", source: source));
    return this;
}

public ElementBuilder<TModel> SetHtml(BindSource source)
{
    _pipeline.AddCommand(new MutateElementCommand(
        _targetId, "el.innerHTML = val", source: source));
    return this;
}
```

And overloads accepting TypedSource<TProp> for consistency:

```csharp
public ElementBuilder<TModel> SetText<TProp>(TypedSource<TProp> source)
{
    _pipeline.AddCommand(new MutateElementCommand(
        _targetId, "el.textContent = val", source: source.ToBindSource()));
    return this;
}
```

**Step 4: Update schema — MutateElementCommand gains vendor, source changes ref**

```json
"MutateElementCommand": {
    "type": "object",
    "required": ["kind", "target", "jsEmit"],
    "additionalProperties": false,
    "properties": {
        "kind": { "const": "mutate-element" },
        "target": { "type": "string", "minLength": 1 },
        "jsEmit": { "type": "string", "minLength": 1 },
        "value": { "type": "string" },
        "source": { "$ref": "#/$defs/BindSource" },
        "vendor": { "$ref": "#/$defs/Vendor" },
        "when": { "$ref": "#/$defs/Guard" }
    }
}
```

**Step 5: Update types.ts — MutateElementCommand**

```typescript
export interface MutateElementCommand {
    kind: "mutate-element";
    target: string;
    jsEmit: string;
    value?: string;
    source?: BindSource;    // CHANGED: was string
    vendor?: Vendor;        // NEW
    when?: Guard;
}
```

**Step 6: Build and typecheck**

Run: `dotnet build && npm run typecheck`
Expected: Compiles. Tests will fail because snapshots have old source format — that's expected, fixed in Task 4.

**Step 7: Commit**

```bash
git add -A && git commit -m "refactor: MutateElementCommand gains vendor, source becomes BindSource

vendor enables runtime root resolution. source unifies with Guard's
BindSource pattern. ComponentRef passes vendor to commands via cached instance."
```

---

### Task 4: Update C# Tests — Regenerate Snapshots + Fix Schema Tests

**Files:**
- Modify: `tests/Alis.Reactive.UnitTests/Commands/WhenResolvingPayloadSource.cs` (if assertions need updating)
- Regenerate: All `WhenResolvingPayloadSource.*.verified.txt` files (~9 files)
- Regenerate: Any other `.verified.txt` files affected by source format change
- Modify: Schema validation tests if needed
- Test: `dotnet test tests/Alis.Reactive.UnitTests`

**Step 1: Delete old verified files for source tests**

```bash
rm tests/Alis.Reactive.UnitTests/Commands/WhenResolvingPayloadSource.*.verified.txt
```

**Step 2: Run tests to regenerate snapshots**

Run: `dotnet test tests/Alis.Reactive.UnitTests`

Verify will create `.received.txt` files. Review them — source should now be:
```json
"source": {
    "kind": "event",
    "path": "evt.intValue"
}
```
Instead of:
```json
"source": "evt.intValue"
```

**Step 3: Accept new snapshots**

Rename all `.received.txt` to `.verified.txt` for source test files.

**Step 4: Run ALL unit tests including schema validation**

Run: `dotnet test tests/Alis.Reactive.UnitTests`
Expected: All pass — snapshots match new format, schema validates with updated MutateElementCommand.

Also run: `dotnet test tests/Alis.Reactive.Native.UnitTests && dotnet test tests/Alis.Reactive.Fusion.UnitTests`

**Step 5: Commit**

```bash
git add -A && git commit -m "test: regenerate snapshots for structured BindSource on MutateElementCommand"
```

---

### Task 5: Update JS Runtime — element.ts Resolves Vendor Root + Source

**Files:**
- Modify: `Scripts/element.ts`
- Modify: `Scripts/resolver.ts` (add component source resolution)
- Modify: `Scripts/__tests__/when-resolving-payload-source.test.ts`
- Modify: `Scripts/__tests__/when-mutating-an-element.test.ts` (if source tests exist)
- Test: `npm test`

**Step 1: Update element.ts — vendor root + structured source**

```typescript
import type { MutateElementCommand, ExecContext, BindSource } from "./types";
import { resolveRoot } from "./component";
import { resolveSource } from "./resolver";
import { scope } from "./trace";

const log = scope("element");

export function mutateElement(cmd: MutateElementCommand, ctx?: ExecContext): void {
    const domEl = document.getElementById(cmd.target);
    if (!domEl) {
        log.warn("target not found", { target: cmd.target });
        return;
    }

    const el = cmd.vendor ? resolveRoot(domEl, cmd.vendor) : domEl;
    const val = cmd.source ? resolveSource(cmd.source, ctx) : cmd.value;

    log.trace("exec", { target: cmd.target, jsEmit: cmd.jsEmit, val });
    new Function("el", "val", cmd.jsEmit).call(null, el, val);
}
```

**Step 2: Update resolver.ts — add component source kind**

```typescript
import { evalRead } from "./component";

export function resolveSource(source: BindSource, ctx?: ExecContext): unknown {
    switch (source.kind) {
        case "event":
            return resolveEventPath(source.path, ctx);
        case "component":
            return evalRead(source.componentId, source.vendor, source.readExpr);
        default:
            return undefined;
    }
}
```

**Step 3: Update TS tests**

Tests that use `source: "evt.foo"` (string) must change to `source: { kind: "event", path: "evt.foo" }`.

In `when-resolving-payload-source.test.ts`, update all test fixtures:

```typescript
// Before:
{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", source: "evt.intValue" }

// After:
{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val",
  source: { kind: "event", path: "evt.intValue" } }
```

**Step 4: Run TS tests**

Run: `npm test`
Expected: All pass.

**Step 5: Commit**

```bash
git add -A && git commit -m "feat: element.ts resolves vendor root + structured BindSource

Runtime resolves component root via cmd.vendor before jsEmit execution.
resolveSource handles both event and component source kinds."
```

---

### Task 6: Update Playwright Tests

**Files:**
- Modify: `tests/Alis.Reactive.PlaywrightTests/Payload/WhenPayloadPropertiesResolve.cs`
- Modify: Sandbox views if plan JSON format assertions exist
- Test: `npm run build:all && dotnet build && dotnet test tests/Alis.Reactive.PlaywrightTests`

**Step 1: Build runtime**

Run: `npm run build:all && dotnet build`

**Step 2: Update Playwright assertions**

In `WhenPayloadPropertiesResolve.cs`, update plan JSON assertions from string source to object source:

```csharp
// Before:
Assert.That(planJson, Does.Contain("\"source\": \"evt.intValue\""));

// After — assert structured source:
Assert.That(planJson, Does.Contain("\"source\""));
Assert.That(planJson, Does.Contain("\"kind\": \"event\""));
Assert.That(planJson, Does.Contain("\"path\": \"evt.intValue\""));
```

**Step 3: Run Playwright tests**

Run: `dotnet test tests/Alis.Reactive.PlaywrightTests`
Expected: All pass — browser behavior unchanged, only plan JSON format changed.

**Step 4: Commit**

```bash
git add -A && git commit -m "test: update Playwright assertions for structured BindSource"
```

---

## Phase 2: ComponentsMap + Validation Fix (Tasks 7-9)

---

### Task 7: ComponentsMap on ReactivePlan — Populated at Builder Creation

**Files:**
- Modify: `Alis.Reactive/IReactivePlan.cs` — add ComponentsMap, keep RegisterComponent for backward compat
- Modify: `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxExtensions.cs` — pass plan, register
- Modify: `Alis.Reactive.Native/Components/NativeDropDown/NativeDropDownExtensions.cs` (or wherever builder For method is)
- Modify: `Alis.Reactive.Native/Components/NativeCheckBox/NativeCheckBoxExtensions.cs`
- Test: `dotnet test tests/Alis.Reactive.UnitTests`

**Step 1: Add ComponentsMap to ReactivePlan**

In `IReactivePlan.cs`, change `_components` from `List<ComponentRegistration>` to `Dictionary<string, ComponentRegistration>` keyed by bindingPath:

```csharp
private readonly Dictionary<string, ComponentRegistration> _componentsMap = new();

public IReadOnlyDictionary<string, ComponentRegistration> ComponentsMap => _componentsMap;

public void AddToComponentsMap(string bindingPath, ComponentRegistration entry)
{
    _componentsMap[bindingPath] = entry;
}

// Keep RegisterComponent for backward compat during migration
public void RegisterComponent(string componentId, string vendor, string bindingPath, string readExpr)
{
    _componentsMap[bindingPath] = new ComponentRegistration(componentId, vendor, bindingPath, readExpr);
}
```

Update `GatherResolver.Resolve()` to accept `IReadOnlyDictionary` — it currently takes `IReadOnlyList<ComponentRegistration>`. Change to iterate `.Values`.

**Step 2: Update builder extensions to take plan and register**

For each component's `*For()` method (the HTML builder creation method), add a plan overload:

```csharp
// FusionNumericTextBoxExtensions.cs — add plan overload
public static NumericTextBoxBuilder NumericTextBoxFor<TModel, TProp>(
    this IHtmlHelper<TModel> html,
    IReactivePlan<TModel> plan,
    Expression<Func<TModel, TProp>> expression)
    where TModel : class
{
    var uniqueId = IdGenerator.For<TModel, TProp>(expression);
    var name = html.NameFor(expression).ToString();

    plan.AddToComponentsMap(name, new ComponentRegistration(
        uniqueId,
        FusionNumericTextBox.Vendor,
        name,
        FusionNumericTextBox.ReadExpr));

    return html.EJS().NumericTextBoxFor(expression)
        .HtmlAttributes(new Dictionary<string, object> { ["id"] = uniqueId, ["name"] = name });
}
```

Keep existing plan-less overload for non-reactive pages.

Same pattern for NativeDropDown, NativeCheckBox builder creation methods.

**Step 3: Update reactive extensions to stop redundant RegisterComponent**

In `.Reactive()` extensions, remove the `plan.RegisterComponent()` call — component is already in map from builder creation. Keep `.Reactive()` for wiring events only.

**Step 4: Test**

Run: `dotnet test tests/Alis.Reactive.UnitTests && dotnet test tests/Alis.Reactive.Native.UnitTests && dotnet test tests/Alis.Reactive.Fusion.UnitTests`
Expected: All pass.

**Step 5: Commit**

```bash
git add -A && git commit -m "feat: ComponentsMap populated at builder creation time

Builder extensions take plan parameter and register component immediately.
Single source of truth for component metadata."
```

---

### Task 8: Validation Uses ComponentsMap — Fix Hardcoded Defaults

**Files:**
- Modify: `Alis.Reactive/Validation/IValidationExtractor.cs` — pass ComponentsMap
- Modify: `Alis.Reactive/Resolvers/ValidationResolver.cs` — pass ComponentsMap to extractor
- Modify: `Alis.Reactive.FluentValidator/FluentValidationAdapter.cs` — lookup map
- Modify: `Alis.Reactive/IReactivePlan.cs` — pass map to resolver in ResolveAll
- Test: `dotnet test tests/Alis.Reactive.FluentValidator.UnitTests`

**Step 1: Update IValidationExtractor interface**

```csharp
public interface IValidationExtractor
{
    ValidationDescriptor? ExtractRules(
        Type validatorType,
        string formId,
        IReadOnlyDictionary<string, ComponentRegistration>? componentsMap = null);
}
```

**Step 2: Update FluentValidationAdapter — lookup ComponentsMap**

```csharp
public ValidationDescriptor? ExtractRules(
    Type validatorType,
    string formId,
    IReadOnlyDictionary<string, ComponentRegistration>? componentsMap = null)
{
    // ... extract rules as before ...

    foreach (var kvp in fieldRules)
    {
        var propertyPath = kvp.Key;
        string elementId;
        string vendor;
        string readExpr;

        if (componentsMap != null && componentsMap.TryGetValue(propertyPath, out var entry))
        {
            elementId = entry.ComponentId;
            vendor = entry.Vendor;
            readExpr = entry.ReadExpr;
        }
        else
        {
            elementId = IdGenerator.For(modelType, propertyPath);
            vendor = "native";
            readExpr = "value";
        }

        fields.Add(new ValidationField(elementId, propertyPath, vendor, readExpr, rules));
    }
}
```

**Step 3: Add IdGenerator.For(Type, string) overload**

In `IdGenerator.cs`:

```csharp
public static string For(Type modelType, string propertyPath)
{
    var scope = TypeScope(modelType);
    return scope + "__" + propertyPath.Replace(".", "_");
}
```

**Step 4: Update ValidationResolver — pass ComponentsMap**

In `ValidationResolver.Resolve()`, accept and pass ComponentsMap to extractor:

```csharp
internal static void Resolve(
    List<Entry> entries,
    IValidationExtractor extractor,
    IReadOnlyDictionary<RequestDescriptor, RequestBuildContext> buildContexts,
    IReadOnlyDictionary<string, ComponentRegistration>? componentsMap = null)
{
    // ... walk entries ...
    var extracted = extractor.ExtractRules(ctx.ValidatorType, formId, componentsMap);
}
```

**Step 5: Update ResolveAll in ReactivePlan — pass ComponentsMap**

```csharp
private void ResolveAll()
{
    GatherResolver.Resolve(_entries, _componentsMap.Values.ToList());
    if (_extractor != null)
        ValidationResolver.Resolve(_entries, _extractor, _buildContexts, _componentsMap);
}
```

**Step 6: Delete ReadExprOverrides**

- Remove `ReadExprOverrides` from `RequestBuildContext.cs`
- Remove `.ReadExpr()` method from `HttpRequestBuilder.cs`
- Remove `WithReadExpr()` from `ValidationDescriptor.cs`
- Remove the readExpr override loop from `ValidationResolver.cs`
- Remove ReadExpr override calls from any sandbox views

**Step 7: Test**

Run: `dotnet test tests/Alis.Reactive.FluentValidator.UnitTests && dotnet test tests/Alis.Reactive.UnitTests`

If FluentValidator tests used ReadExprOverrides, update them to use ComponentsMap instead.

**Step 8: Commit**

```bash
git add -A && git commit -m "fix: validation uses ComponentsMap for vendor + readExpr

FluentValidationAdapter looks up component metadata from plan's
ComponentsMap. No more hardcoded 'native'/'value' defaults.
Delete ReadExprOverrides, WithReadExpr, HttpRequestBuilder.ReadExpr."
```

---

### Task 9: PipelineBuilder.When(TypedSource) — Component Conditions

**Files:**
- Modify: `Alis.Reactive/Builders/PipelineBuilder.cs` — add When overload
- Create: `tests/Alis.Reactive.UnitTests/Conditions/WhenConditionReadsComponent.cs`
- Test: `dotnet test tests/Alis.Reactive.UnitTests`

**Step 1: Add When overload for TypedSource**

In `PipelineBuilder.cs`:

```csharp
public ConditionSourceBuilder<TModel, TProp> When<TProp>(TypedSource<TProp> source)
{
    SetMode(PipelineMode.Conditional);
    return new ConditionSourceBuilder<TModel, TProp>(source, this);
}
```

**Step 2: Write test**

```csharp
[TestFixture]
public class WhenConditionReadsComponent : PlanTestBase
{
    [Test]
    public Task Condition_reads_component_value() =>
        VerifyJson(Build(p =>
        {
            var amountRef = p.Component<FusionNumericTextBox>(m => m.Amount);
            p.When(amountRef.ReadProperty<decimal>("value"))
             .Gt(0)
             .Then(then => then.Element("status").SetText("positive"));
        }).Render());

    [Test]
    public void Condition_reading_component_conforms_to_schema()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var amountRef = p.Component<FusionNumericTextBox>(m => m.Amount);
            p.When(amountRef.ReadProperty<decimal>("value"))
             .Gt(0)
             .Then(then => then.Element("status").SetText("positive"));
        });
        AssertSchemaValid(plan.Render());
    }
}
```

**Step 3: Run test, verify snapshot, validate schema**

Run: `dotnet test tests/Alis.Reactive.UnitTests`
Expected: Snapshot shows `source: { kind: "component", componentId: "...", vendor: "fusion", readExpr: "value" }` in guard.

**Step 4: Commit**

```bash
git add -A && git commit -m "feat: When(TypedSource) enables conditions reading component values

Typed condition pipeline works with component sources — preserves
compile-time coercion inference and guard operator type safety."
```

---

## Phase 3: Clean jsEmit + Full FusionNumericTextBox API (Tasks 10-12)

---

### Task 10: Clean Fusion jsEmit — Remove el.ej2_instances[0] from Extensions

**Files:**
- Modify: `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxExtensions.cs`
- Modify: `Alis.Reactive.Fusion/Components/TestWidgetSyncFusion/TestWidgetSyncFusionExtensions.cs`
- Modify: `Alis.Reactive.Fusion/AppLevel/FusionConfirm/FusionConfirmExtensions.cs`
- Regenerate: Affected `.verified.txt` snapshots
- Test: `dotnet test && npm test && npm run build:all && dotnet test tests/Alis.Reactive.PlaywrightTests`

**Step 1: Clean FusionNumericTextBox jsEmit strings**

```csharp
// Before:
self.Emit("var c=el.ej2_instances[0]; c.value=Number(val); c.dataBind()", value)

// After (el IS the ej2 instance, runtime resolved root):
self.Emit("el.value=Number(val)", value)
```

```csharp
// Before:
self.Emit("el.ej2_instances[0].focusIn()")

// After:
self.Emit("el.focusIn()")
```

Apply to ALL Fusion extension methods.

**Step 2: Clean TestWidgetSyncFusion jsEmit**

Same pattern — remove `el.ej2_instances[0]` prefix from all jsEmit strings.

**Step 3: Clean FusionConfirm jsEmit**

Same pattern.

**Step 4: Rebuild and regenerate snapshots**

```bash
npm run build:all && dotnet build
```

Delete affected `.verified.txt` files, run tests to regenerate with new jsEmit strings.

**Step 5: Run ALL tests**

```bash
npm test
dotnet test tests/Alis.Reactive.UnitTests
dotnet test tests/Alis.Reactive.Native.UnitTests
dotnet test tests/Alis.Reactive.Fusion.UnitTests
dotnet test tests/Alis.Reactive.PlaywrightTests
```

Expected: All pass. Playwright tests verify browser behavior — the actual DOM behavior is unchanged because runtime now resolves root before jsEmit.

**Step 6: Commit**

```bash
git add -A && git commit -m "refactor: clean Fusion jsEmit — remove el.ej2_instances[0]

Runtime resolves vendor root before passing to jsEmit. Vertical slice
extensions operate on component instance directly."
```

---

### Task 11: Expand FusionNumericTextBox JS API

**Files:**
- Modify: `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxExtensions.cs`
- Modify: `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxEvents.cs`
- Create: `Alis.Reactive.Fusion/Components/FusionNumericTextBox/Events/FusionNumericTextBoxOnFocus.cs`
- Create: `Alis.Reactive.Fusion/Components/FusionNumericTextBox/Events/FusionNumericTextBoxOnBlur.cs`
- Create: `tests/Alis.Reactive.Fusion.UnitTests/Components/WhenUsingNumericTextBoxFullApi.cs`
- Test: `dotnet test tests/Alis.Reactive.Fusion.UnitTests`

**Step 1: Add event descriptors**

```csharp
// FusionNumericTextBoxEvents.cs
public TypedEventDescriptor<FusionNumericTextBoxFocusArgs> Focus =>
    new("focus", new FusionNumericTextBoxFocusArgs());

public TypedEventDescriptor<FusionNumericTextBoxBlurArgs> Blur =>
    new("blur", new FusionNumericTextBoxBlurArgs());

// FusionNumericTextBoxOnFocus.cs
public class FusionNumericTextBoxFocusArgs { }

// FusionNumericTextBoxOnBlur.cs
public class FusionNumericTextBoxBlurArgs { }
```

**Step 2: Add method extensions**

```csharp
// FusionNumericTextBoxExtensions.cs — add missing methods
public static ComponentRef<FusionNumericTextBox, TModel> FocusOut<TModel>(
    this ComponentRef<FusionNumericTextBox, TModel> self)
    where TModel : class
    => self.Emit("el.focusOut()");

public static ComponentRef<FusionNumericTextBox, TModel> Increment<TModel>(
    this ComponentRef<FusionNumericTextBox, TModel> self)
    where TModel : class
    => self.Emit("el.increment()");

public static ComponentRef<FusionNumericTextBox, TModel> Decrement<TModel>(
    this ComponentRef<FusionNumericTextBox, TModel> self)
    where TModel : class
    => self.Emit("el.decrement()");
```

**Step 3: Add property read extensions (typed)**

```csharp
public static TypedComponentSource<decimal> Value<TModel>(
    this ComponentRef<FusionNumericTextBox, TModel> self)
    where TModel : class
    => self.ReadProperty<decimal>(FusionNumericTextBox.ReadExpr);

public static TypedComponentSource<decimal> Min<TModel>(
    this ComponentRef<FusionNumericTextBox, TModel> self)
    where TModel : class
    => self.ReadProperty<decimal>("min");
```

**Step 4: Add property write extensions**

```csharp
public static ComponentRef<FusionNumericTextBox, TModel> SetMin<TModel>(
    this ComponentRef<FusionNumericTextBox, TModel> self, decimal min)
    where TModel : class
    => self.Emit("el.min=Number(val)", min.ToString(CultureInfo.InvariantCulture));
```

**Step 5: Write tests and commit**

Test every new extension — snapshot + schema validation. Follow existing test patterns.

```bash
git add -A && git commit -m "feat: FusionNumericTextBox full JS API — events, methods, properties"
```

---

### Task 12: FusionNumericTextBox Sandbox Page

**Files:**
- Create: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/NumericTextBox.cshtml`
- Modify: `Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/` — add controller/action
- Create: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/WhenUsingNumericTextBox.cs`
- Test: `dotnet test tests/Alis.Reactive.PlaywrightTests`

**Step 1: Create sandbox page**

The page should exercise ALL supported API:
- Property read + write (value, min)
- All methods (focusIn, focusOut, increment, decrement)
- All events (change with typed payload, focus, blur)
- Condition reading component value
- Gather with IncludeAll
- Validation with FluentValidation

**Step 2: Create Playwright tests**

Test every interaction path shown on the sandbox page. Verify DOM state after each operation.

**Step 3: Commit**

```bash
git add -A && git commit -m "feat: NumericTextBox sandbox page — exercises full JS API"
```

---

## Phase 4: FusionDropDownList Vertical Slice (Tasks 13-15)

---

### Task 13: FusionDropDownList Component Class + Events

**Files:**
- Create: `Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownList.cs`
- Create: `Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownListEvents.cs`
- Create: `Alis.Reactive.Fusion/Components/FusionDropDownList/Events/FusionDropDownListOnChanged.cs`
- Create: `Alis.Reactive.Fusion/Components/FusionDropDownList/Events/FusionDropDownListOnFocus.cs`
- Create: `Alis.Reactive.Fusion/Components/FusionDropDownList/Events/FusionDropDownListOnBlur.cs`

Follow exact naming conventions from FusionNumericTextBox. Component declares Vendor + ReadExpr. Events use TypedEventDescriptor pattern.

**Step 1: Create component class**

```csharp
public sealed class FusionDropDownList : FusionComponent, IInputComponent
{
    public string Vendor => "fusion";
    public string ReadExpr => "value";
}
```

**Step 2: Create events — research actual Syncfusion DropDownList JS API first**

Check Syncfusion docs for actual event names and payload properties. Create typed args classes.

**Step 3: Commit**

---

### Task 14: FusionDropDownList Extensions + Builder + Reactive

**Files:**
- Create: `Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownListExtensions.cs`
- Create: `Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownListReactiveExtensions.cs`
- Create: `tests/Alis.Reactive.Fusion.UnitTests/Components/WhenUsingDropDownList.cs`

**Step 1: Create extensions**

Property writes, method calls, property reads — all following the clean jsEmit pattern (no el.ej2_instances[0]).

**Step 2: Create builder For method with plan registration**

```csharp
public static DropDownListBuilder DropDownListFor<TModel, TProp>(
    this IHtmlHelper<TModel> html,
    IReactivePlan<TModel> plan,
    Expression<Func<TModel, TProp>> expression)
    where TModel : class
{
    var uniqueId = IdGenerator.For<TModel, TProp>(expression);
    var name = html.NameFor(expression).ToString();

    plan.AddToComponentsMap(name, new ComponentRegistration(
        uniqueId,
        FusionDropDownList.Vendor,
        name,
        FusionDropDownList.ReadExpr));

    return html.EJS().DropDownListFor(expression)
        .HtmlAttributes(new Dictionary<string, object> { ["id"] = uniqueId, ["name"] = name });
}
```

**Step 3: Create reactive extensions**

Follow FusionNumericTextBox pattern. Use `FusionDropDownList.Vendor` and `FusionDropDownList.ReadExpr` — zero reflection.

**Step 4: Write tests and commit**

---

### Task 15: FusionDropDownList Sandbox Page + Playwright Tests

**Files:**
- Create: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/DropDownList.cshtml`
- Create: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/WhenUsingDropDownList.cs`

Same pattern as Task 12. Exercise all supported API. Playwright tests verify browser behavior.

---

## Phase 5: Cleanup (Task 16)

---

### Task 16: Final Cleanup + CLAUDE.md Update

**Files:**
- Modify: `CLAUDE.md` — document new architecture rules
- Delete: Any dead code, unused imports, obsolete comments
- Test: Full test suite

**Step 1: Update CLAUDE.md**

Add to architecture rules:
- IComponent declares Vendor, IInputComponent declares ReadExpr — instance interface properties (C# 8.0), no reflection
- ComponentsMap is single source of truth — populated at builder creation time
- MutateElementCommand carries vendor — runtime resolves root before jsEmit
- jsEmit operates on vendor-resolved root, never on raw DOM element for components
- BindSource is structured everywhere — EventSource + ComponentSource, no raw strings
- TypedComponentSource preserves typed condition pipeline

**Step 2: Full test suite**

```bash
npm test
dotnet test tests/Alis.Reactive.UnitTests
dotnet test tests/Alis.Reactive.Native.UnitTests
dotnet test tests/Alis.Reactive.Fusion.UnitTests
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests
dotnet test tests/Alis.Reactive.PlaywrightTests
```

All must pass.

**Step 3: Commit**

```bash
git add -A && git commit -m "docs: update CLAUDE.md with vertical slice architecture rules"
```
