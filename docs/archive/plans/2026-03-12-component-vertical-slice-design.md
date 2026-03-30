# Component Vertical Slice Architecture — Design

## Problem

The current component architecture has fragmented knowledge, unnecessary reflection, broken runtime paths, and hardcoded defaults that produce incorrect behavior.

**Specific issues:**

1. `[ReadExpr("value")]` attribute + `ComponentHelper.GetReadExpr<T>()` uses reflection to read what should be a compile-time contract.
2. `FusionNumericTextBoxReactiveExtensions.ExtractProperty()` uses reflection to read ID/Name from Syncfusion builders.
3. Every Fusion jsEmit string embeds `el.ej2_instances[0]` — vendor root resolution that the runtime already handles in `component.ts`.
4. `.Value()` returns `"ref:id.value"` — a magic string the runtime **cannot resolve** (no `ref:` handler exists).
5. `FluentValidationAdapter` hardcodes `vendor: "native"` and `readExpr: "value"` for ALL fields — wrong for Fusion components and checkboxes.
6. `ReadExprOverrides` is a bandaid patching issue #5.
7. Components only enter the plan when `.Reactive()` is called — if a component is used for gather/validation without `.Reactive()`, it's missing from the plan.
8. `MutateElementCommand.source` is a raw `BindExpr` string while `Guard.source` is a structured `BindSource` — inconsistent.

## Solution

A component is a **complete, self-contained vertical slice**. It declares its identity (vendor, primary read property) via interface contracts. The plan's **ComponentsMap** is the single source of truth for all component metadata, populated at builder creation time.

## Design

### 1. Interface Contracts (No Reflection) — C# 8.0 Compatible

```csharp
public interface IComponent
{
    string Vendor { get; }
}

public interface IInputComponent : IComponent
{
    string ReadExpr { get; }
}

// Example
public sealed class FusionNumericTextBox : FusionComponent, IInputComponent
{
    public string Vendor => "fusion";
    public string ReadExpr => "value";
}

public sealed class NativeCheckBox : NativeComponent, IInputComponent
{
    public string Vendor => "native";
    public string ReadExpr => "checked";
}

public sealed class NativeButton : NativeComponent, IComponent
{
    public string Vendor => "native";
    // NOT IInputComponent — buttons have no form value
}
```

**C# 8.0 constraint:** Instance properties (not `static abstract` — requires C# 11). Builder extensions instantiate the component to read Vendor/ReadExpr at registration time. `ComponentRef<TComponent>` uses `where TComponent : IComponent, new()` to access vendor via `new TComponent().Vendor`.

**Deleted:** `ReadExprAttribute`, `ComponentHelper`, `IReadableComponent`.

### 2. ComponentsMap — Populated at Builder Creation Time

```csharp
// Builder extension now takes plan
Html.NumericTextBoxFor(plan, m => m.Amount)

// Inside the extension:
var component = new FusionNumericTextBox();
var id = IdGenerator.For<TModel, TProp>(expression);
var bindingPath = html.NameFor(expression).ToString();
plan.ComponentsMap.Add(bindingPath, new ComponentMapEntry(
    id,
    component.Vendor,    // "fusion"
    bindingPath,          // "Amount"
    component.ReadExpr   // "value"
));
```

The component is in the map **the moment it's created in the view**. Not when `.Reactive()` is called. Not when gather runs.

**Consumed by:**
- `GatherResolver` — expands `AllGather` using map entries (already works, just uses map instead of list)
- `ValidationResolver` — looks up vendor + readExpr by field name (replaces hardcoded defaults)

**Deleted:** `ReadExprOverrides`, `.ReadExpr()` on `HttpRequestBuilder`, `WithReadExpr()` on `ValidationDescriptor`.

### 3. ComponentRef Carries Vendor (C# 8.0 — `new()` constraint)

```csharp
public class ComponentRef<TComponent, TModel>
    where TComponent : IComponent, new()
    where TModel : class
{
    private static readonly TComponent _instance = new TComponent();

    internal string TargetId { get; }
    internal string Vendor => _instance.Vendor;
    internal PipelineBuilder<TModel> Pipeline { get; }

    internal ComponentRef Emit(string jsEmit, string? value = null)
    {
        Pipeline.AddCommand(new MutateElementCommand(
            TargetId, jsEmit, value, vendor: Vendor));
        return this;
    }

    internal ComponentSource ReadProperty(string property)
        => new ComponentSource(TargetId, _instance.Vendor, property);
}
```

**C# 8.0 pattern:** `new()` constraint + static cached instance. The component is a lightweight phantom type — one shared instance per `TComponent` is sufficient since Vendor/ReadExpr are constant expression-bodied properties.

### 4. Vertical Slice Extensions — jsEmit Operates on Component Root

The runtime resolves vendor root **before** passing to jsEmit. `el` in jsEmit is always the vendor-resolved root.

```csharp
// PROPERTY WRITE
static SetValue(self, decimal value)
    => self.Emit("el.value=Number(val)", value.ToString());
    // el = ej2_instances[0] for fusion, el = DOM element for native

static SetMin(self, decimal min)
    => self.Emit("el.min=Number(val)", min.ToString());

// METHOD CALL
static FocusIn(self)  => self.Emit("el.focusIn()");
static FocusOut(self) => self.Emit("el.focusOut()");
static Increment(self) => self.Emit("el.increment()");
static Decrement(self) => self.Emit("el.decrement()");

// PROPERTY READ — returns structured ComponentSource
static Value(self) => self.ReadProperty(FusionNumericTextBox.ReadExpr);
static Min(self)   => self.ReadProperty("min");
```

No `el.ej2_instances[0]` anywhere in vertical slice code. One declaration of `"value"` on the component class, derived everywhere.

### 5. MutateElementCommand — Vendor + Structured Source

```csharp
public sealed class MutateElementCommand : Command
{
    public string Target { get; }
    public string JsEmit { get; }
    public string? Value { get; }
    public BindSource? Source { get; }     // structured, not raw string
    public string? Vendor { get; }         // NEW — null for plain elements
    public Guard? When { get; }
}
```

**BindSource** gains a ComponentSource variant:

```csharp
[JsonDerivedType(typeof(EventSource), "event")]
[JsonDerivedType(typeof(ComponentSource), "component")]
public abstract class BindSource { }

public sealed class EventSource : BindSource
{
    public string Path { get; }
}

public sealed class ComponentSource : BindSource
{
    public string ComponentId { get; }
    public string Vendor { get; }
    public string ReadExpr { get; }
}
```

### 6. Schema Changes

```json
"MutateElementCommand": {
    "properties": {
        "kind": { "const": "mutate-element" },
        "target": { "type": "string" },
        "jsEmit": { "type": "string" },
        "value": { "type": "string" },
        "source": { "$ref": "#/$defs/BindSource" },
        "vendor": { "$ref": "#/$defs/Vendor" },
        "when": { "$ref": "#/$defs/Guard" }
    }
}

"BindSource": {
    "oneOf": [
        { "$ref": "#/$defs/EventSource" },
        { "$ref": "#/$defs/ComponentSource" }
    ]
}

"ComponentSource": {
    "type": "object",
    "required": ["kind", "componentId", "vendor", "readExpr"],
    "properties": {
        "kind": { "const": "component" },
        "componentId": { "type": "string" },
        "vendor": { "$ref": "#/$defs/Vendor" },
        "readExpr": { "type": "string" }
    }
}
```

### 7. JS Runtime Changes

**element.ts** — resolves vendor root before jsEmit execution:

```typescript
function mutateElement(cmd: MutateElementCommand, ctx?: ExecContext): void {
    const domEl = document.getElementById(cmd.target);
    if (!domEl) return;

    const el = cmd.vendor
        ? resolveRoot(domEl, cmd.vendor)   // component → vendor root
        : domEl;                            // plain element → DOM element

    const val = resolveValue(cmd, ctx);
    new Function("el", "val", cmd.jsEmit).call(null, el, val);
}

function resolveValue(cmd: MutateElementCommand, ctx?: ExecContext): unknown {
    if (!cmd.source) return cmd.value;
    switch (cmd.source.kind) {
        case "event":
            return walk(ctx, cmd.source.path);
        case "component":
            return evalRead(cmd.source.componentId, cmd.source.vendor, cmd.source.readExpr);
    }
}
```

**conditions.ts** — resolveSource gains component support:

```typescript
function resolveSource(source: BindSource, ctx?: ExecContext): unknown {
    switch (source.kind) {
        case "event":
            return walk(ctx, source.path);
        case "component":
            return evalRead(source.componentId, source.vendor, source.readExpr);
    }
}
```

**types.ts** — MutateElementCommand gains vendor, source becomes BindSource:

```typescript
interface MutateElementCommand {
    kind: "mutate-element";
    target: string;
    jsEmit: string;
    value?: string;
    source?: BindSource;    // was string, now structured
    vendor?: Vendor;        // NEW
    when?: Guard;
}

type BindSource = EventSource | ComponentSource;

interface ComponentSource {
    kind: "component";
    componentId: string;
    vendor: Vendor;
    readExpr: string;
}
```

**Unchanged modules:** `component.ts`, `walk.ts`, `gather.ts`, `validation.ts`, `trigger.ts`, `boot.ts`.

### 8. Validation — From ComponentsMap

`FluentValidationAdapter` changes from hardcoded defaults to ComponentsMap lookup:

```csharp
// Before (broken):
fields.Add(new ValidationField(elementId, propertyPath, "native", "value", rules));

// After (correct):
if (componentsMap.TryGetValue(propertyPath, out var entry))
{
    fields.Add(new ValidationField(entry.ComponentId, propertyPath,
        entry.Vendor, entry.ReadExpr, rules));
}
else
{
    // Fallback for fields not in map (e.g., plain HTML inputs without builder)
    var elementId = IdGenerator.For(modelType, propertyPath);
    fields.Add(new ValidationField(elementId, propertyPath, "native", "value", rules));
}
```

`IValidationExtractor.ExtractRules` gains access to the ComponentsMap.

### 9. IdGenerator Alignment

`FluentValidationAdapter` currently duplicates IdGenerator format:

```csharp
var scope = IdGenerator.TypeScope(modelType) + "__";
var elementId = scope + propertyPath.Replace(".", "_");
```

Add an overload to IdGenerator:

```csharp
public static string For(Type modelType, string propertyPath)
{
    var scope = TypeScope(modelType);
    return scope + "__" + propertyPath.Replace(".", "_");
}
```

FluentValidationAdapter fallback uses: `IdGenerator.For(modelType, propertyPath)`.

## Public DSL Changes

**Changed:**
- `Html.NumericTextBoxFor(m => m.Amount)` → `Html.NumericTextBoxFor(plan, m => m.Amount)` (plan parameter added; plan-less overload kept for non-reactive pages)
- Same for all builder extensions: `NativeDropDownFor`, `NativeCheckBox`, etc.
- `.Value()` / `.Checked()` / `.Min()` return `ComponentSource` instead of `string`
- `IReadableComponent` → `IInputComponent` (rename + gains `ReadExpr` + `Vendor`)

**Added (new overloads):**
- `ElementBuilder.SetText(ComponentSource)`, `SetHtml(ComponentSource)`
- `PipelineBuilder.When(ComponentSource, guard, then)` — condition from component
- `SetValue(ComponentSource)` on component extensions — write from another component

**Removed:**
- `[ReadExpr]` attribute — replaced by `IInputComponent.ReadExpr`
- `ComponentHelper` class — no more reflection
- `.ReadExpr()` on `HttpRequestBuilder` — unnecessary with ComponentsMap
- `WithReadExpr()` on `ValidationDescriptor` — unnecessary with ComponentsMap
- `ExtractProperty()` reflection in Fusion reactive extensions

**No features lost.** The `ref:` string pattern was non-functional. `ReadExprOverrides` was patching a design flaw.

## Scope — Components to Implement

### FusionNumericTextBox (existing, expand JS API)

**Events:** change (Value, PreviousValue, IsInteracted), focus (empty), blur (empty)

**Methods:** focusIn(), focusOut(), increment(), decrement()

**Properties (read + write):** value, min

### FusionDropDownList (new vertical slice)

New Fusion component following same naming conventions. Full JS API scope TBD during implementation planning.

### Existing Components (update to new architecture)

NativeCheckBox, NativeDropDown, NativeButton, TestWidgetSyncFusion — all updated to use `IInputComponent` / `IComponent` with instance interface properties, ComponentsMap registration, clean jsEmit.

## Sandbox Pages

`/Sandbox/Components/Fusion/NumericTextBox.cshtml` — exercises all supported API: property read/write, methods, events, conditions, gather, validation.

`/Sandbox/Components/Fusion/DropDownList.cshtml` — same for new component.

## Known Limitation (Deferred)

Method return values as sources (e.g., `getText()`) — `walk()` reads properties, doesn't invoke methods. Not needed for current scope (user excluded `getText()`). Can extend `evalRead` later if needed.
