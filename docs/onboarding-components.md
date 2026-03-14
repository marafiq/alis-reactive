# Onboarding a New Component

> Surgical guide for adding a Syncfusion or native component with a complex JS API.
> Every component is a vertical slice — 8 files, zero runtime changes.

## The Pattern: One Component = One Folder

```
Alis.Reactive.Fusion/Components/FusionDatePicker/
├── FusionDatePicker.cs                      # 1. Sealed phantom type
├── FusionDatePickerExtensions.cs            # 2. Mutation + read methods
├── FusionDatePickerReactiveExtensions.cs    # 3. Event wiring
├── FusionDatePickerBuilder.cs               # 4. HTML rendering (optional — SF has its own)
└── Events/
    ├── FusionDatePickerOnChanged.cs          # 5. Event args (one per JS event)
    └── FusionDatePickerEvents.cs             # 6. Events singleton registry
```

Plus:
- Gather extensions (shared per vendor, already exist)
- Tests at 3 layers (C# snapshot, TS vitest, Playwright)

---

## Step 1: Study the JS API

Before writing C#, catalog the JS component's API surface. Map every interaction
into exactly one of these patterns:

| JS API pattern | C# construct | Mutation kind | Example |
|----------------|-------------|--------------|---------|
| `comp.value = x` | `SetPropMutation("value")` | `set-prop` | `SetValue("x")` |
| `comp.value = Number(x)` | `SetPropMutation("value", coerce: "number")` | `set-prop` | `SetValue(42m)` |
| `comp.focus()` | `CallMutation("focus")` | `call` (no args) | `Focus()` |
| `comp.addItem(item)` | `CallMutation("addItem", args: [SourceArg])` | `call` (1 arg) | `AddItem(payload, x => x.Item)` |
| `comp.addItem(item, idx)` | `CallMutation("addItem", args: [SourceArg, SourceArg])` | `call` (N args) | `AddItem(src1, src2)` |
| `el.classList.add(cls)` | `CallMutation("add", chain: "classList", args: [LiteralArg])` | `call` (chained) | `AddClass("active")` |
| `el.setAttribute(k, v)` | `CallMutation("setAttribute", args: [LiteralArg, LiteralArg])` | `call` (multi-literal) | `Hide()` |
| Read `comp.value` | `TypedComponentSource<T>` | (not a mutation) | `Value()` |

**Two rules:**
1. Property assignment → `SetPropMutation`. Value/source stays on `MutateElementCommand`.
2. Method call → `CallMutation`. Args go into `MethodArg[]` array.

---

## Step 2: Component Class (Phantom Type)

```csharp
// Alis.Reactive.Fusion/Components/FusionDatePicker/FusionDatePicker.cs

public sealed class FusionDatePicker : FusionComponent, IFusionInputComponent, IInputComponent
{
    public string ReadExpr => "value";  // JS property to read: ej2.value
}
```

**Rules:**
- `sealed` — always
- Zero fields, zero state — it's a phantom type for generic constraints
- `IComponent` = has `Vendor` (from `FusionComponent` base)
- `IInputComponent` = has `ReadExpr` (the property path to read the component's value)
- `ReadExpr` is the dot-path from the vendor root. For Fusion: `ej2_instances[0].{readExpr}`

**What is ReadExpr?** Open browser devtools, find the element, and check:
```js
// Fusion (vendor: "fusion"): root = el.ej2_instances[0]
el.ej2_instances[0].value     → readExpr: "value"
el.ej2_instances[0].checked   → readExpr: "checked"

// Native (vendor: "native"): root = el
el.value                      → readExpr: "value"
el.checked                    → readExpr: "checked"
```

---

## Step 3: Event Args + Events Registry

### 3a. Event args — one class per JS event

```csharp
// Events/FusionDatePickerOnChanged.cs

public class FusionDatePickerChangeArgs
{
    public DateTime? Value { get; set; }
    public DateTime? PreviousValue { get; set; }
    public bool IsInteracted { get; set; }

    public FusionDatePickerChangeArgs() { }
}
```

**How to find the args:** Open SF docs, find the event's callback signature.
Map each callback parameter to a C# property. Use the strongest type
(`DateTime?`, `decimal`, `bool` — not `object`). These types flow into
the condition pipeline: `p.When(args, x => x.Value).Gte(someDate)`.

### 3b. Events singleton — maps friendly names to JS event strings

```csharp
// Events/FusionDatePickerEvents.cs

public sealed class FusionDatePickerEvents
{
    public static readonly FusionDatePickerEvents Instance = new();
    private FusionDatePickerEvents() { }

    public TypedEventDescriptor<FusionDatePickerChangeArgs> Changed =>
        new("change", new FusionDatePickerChangeArgs());

    public TypedEventDescriptor<FusionDatePickerFocusArgs> Focus =>
        new("focus", new FusionDatePickerFocusArgs());

    public TypedEventDescriptor<FusionDatePickerBlurArgs> Blur =>
        new("blur", new FusionDatePickerBlurArgs());
}
```

**How to find the JS event name:** Open SF docs → Events section.
The string `"change"` is what SF fires via `addEventListener("change", ...)`.

---

## Step 4: Extensions (Mutations + Reads)

This is the C# DSL surface — the methods users call in `.cshtml` views.

```csharp
// FusionDatePickerExtensions.cs

public static class FusionDatePickerExtensions
{
    private static readonly FusionDatePicker _component = new();

    // ── PATTERN A: Property write (static literal) ──
    //    JS: ej2.value = "2024-01-15"
    public static ComponentRef<FusionDatePicker, TModel> SetValue<TModel>(
        this ComponentRef<FusionDatePicker, TModel> self, DateTime value)
        where TModel : class
        => self.Emit(new SetPropMutation("value"), value: value.ToString("yyyy-MM-dd"));

    // ── PATTERN B: Property write (from event payload) ──
    //    JS: ej2.value = evt.detail.selectedDate
    public static ComponentRef<FusionDatePicker, TModel> SetValue<TModel, TSource>(
        this ComponentRef<FusionDatePicker, TModel> self,
        TSource source, Expression<Func<TSource, object?>> path)
        where TModel : class
    {
        var sourcePath = ExpressionPathHelper.ToEventPath(path);
        return self.Emit(new SetPropMutation("value"), source: new EventSource(sourcePath));
    }

    // ── PATTERN C: Property write (from HTTP response body) ──
    //    JS: ej2.value = responseBody.record.date
    public static ComponentRef<FusionDatePicker, TModel> SetValue<TModel, TResponse>(
        this ComponentRef<FusionDatePicker, TModel> self,
        ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
        where TModel : class where TResponse : class
    {
        var sourcePath = ExpressionPathHelper.ToResponsePath(path);
        return self.Emit(new SetPropMutation("value"), source: new EventSource(sourcePath));
    }

    // ── PATTERN D: Property write (from another component's value) ──
    //    JS: ej2.value = otherComponent.value
    public static ComponentRef<FusionDatePicker, TModel> SetValue<TModel, TProp>(
        this ComponentRef<FusionDatePicker, TModel> self, TypedSource<TProp> source)
        where TModel : class
        => self.Emit(new SetPropMutation("value"), source: source.ToBindSource());

    // ── PATTERN E: Void method (no args) ──
    //    JS: ej2.show()
    public static ComponentRef<FusionDatePicker, TModel> Show<TModel>(
        this ComponentRef<FusionDatePicker, TModel> self)
        where TModel : class => self.Emit(new CallMutation("show"));

    // ── PATTERN F: Method with source arg ──
    //    JS: ej2.navigateTo(month)   where month comes from event payload
    public static ComponentRef<FusionDatePicker, TModel> NavigateTo<TModel, TSource>(
        this ComponentRef<FusionDatePicker, TModel> self,
        TSource source, Expression<Func<TSource, object?>> path)
        where TModel : class
    {
        var sourcePath = ExpressionPathHelper.ToEventPath(path);
        return self.Emit(new CallMutation("navigateTo",
            args: new MethodArg[] { new SourceArg(new EventSource(sourcePath)) }));
    }

    // ── PATTERN G: Method with literal args ──
    //    JS: ej2.setAttribute("readonly", "")
    public static ComponentRef<FusionDatePicker, TModel> SetReadonly<TModel>(
        this ComponentRef<FusionDatePicker, TModel> self)
        where TModel : class
        => self.Emit(new CallMutation("setAttribute",
            args: new MethodArg[] { new LiteralArg("readonly"), new LiteralArg("") }));

    // ── PATTERN H: Method with mixed args (literal + source) ──
    //    JS: ej2.setProperty("min", resolvedValue)
    public static ComponentRef<FusionDatePicker, TModel> SetNamedProperty<TModel, TSource>(
        this ComponentRef<FusionDatePicker, TModel> self,
        string name, TSource source, Expression<Func<TSource, object?>> path)
        where TModel : class
    {
        var sourcePath = ExpressionPathHelper.ToEventPath(path);
        return self.Emit(new CallMutation("setProperty",
            args: new MethodArg[] {
                new LiteralArg(name),
                new SourceArg(new EventSource(sourcePath))
            }));
    }

    // ── PROPERTY READ (for conditions + component source) ──
    //    JS: ej2.value   (read, not write)
    public static TypedComponentSource<DateTime?> Value<TModel>(
        this ComponentRef<FusionDatePicker, TModel> self)
        where TModel : class
        => new TypedComponentSource<DateTime?>(
            self.TargetId, _component.Vendor, _component.ReadExpr);
}
```

### Decision tree: which pattern do I use?

```
Is it a property assignment?  (ej2.prop = val)
├── YES → SetPropMutation("prop")
│   ├── Static value?    → value: "literal"
│   ├── Event payload?   → source: new EventSource(path)
│   ├── Response body?   → source: new EventSource(responsePath)
│   └── Other component? → source: typedSource.ToBindSource()
│
└── NO → it's a method call  (ej2.method(...))
    └── CallMutation("method")
        ├── No args?     → args: null (omit)
        ├── Literal?     → args: [new LiteralArg(x)]
        ├── Source?      → args: [new SourceArg(new EventSource(path))]
        ├── Multi-lit?   → args: [new LiteralArg(a), new LiteralArg(b)]
        └── Mixed?       → args: [new LiteralArg(name), new SourceArg(src)]
```

---

## Step 5: Reactive Extensions (Event Wiring)

```csharp
// FusionDatePickerReactiveExtensions.cs

public static class FusionDatePickerReactiveExtensions
{
    private static readonly FusionDatePicker _component = new();

    public static DatePickerBuilder Reactive<TModel, TArgs>(
        this DatePickerBuilder builder,
        IReactivePlan<TModel> plan,
        Func<FusionDatePickerEvents, TypedEventDescriptor<TArgs>> eventSelector,
        Action<TArgs, PipelineBuilder<TModel>> pipeline)
        where TModel : class
    {
        var descriptor = eventSelector(FusionDatePickerEvents.Instance);
        var pb = new PipelineBuilder<TModel>();
        pipeline(descriptor.Args, pb);

        // Read component ID from SF builder's HtmlAttributes
        var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
        var componentId = (string)attrs["id"];
        var bindingPath = (string)attrs["name"];

        var trigger = new ComponentEventTrigger(
            componentId, descriptor.JsEvent, _component.Vendor,
            bindingPath, _component.ReadExpr);

        plan.AddEntry(new Entry(trigger, pb.BuildReaction()));
        return builder;
    }
}
```

**For standalone components** (non-SF, like TestWidget), the builder owns the
element ID directly — no need to dig into `HtmlAttributes`:

```csharp
var trigger = new ComponentEventTrigger(
    builder.ElementId, descriptor.JsEvent, _component.Vendor,
    readExpr: _component.ReadExpr);
```

---

## Step 6: Builder (HTML Rendering + ComponentsMap Registration)

### Option A: Syncfusion component (wraps existing SF builder)

```csharp
// In FusionDatePickerExtensions.cs (builder method)

public static DatePickerBuilder DatePickerFor<TModel, TProp>(
    this IHtmlHelper<TModel> html,
    IReactivePlan<TModel> plan,
    Expression<Func<TModel, TProp>> expression)
    where TModel : class
{
    var uniqueId = IdGenerator.For<TModel, TProp>(expression);
    var name = html.NameFor(expression).ToString();

    plan.AddToComponentsMap(name, new ComponentRegistration(
        uniqueId, _component.Vendor, name, _component.ReadExpr));

    return html.EJS().DatePickerFor(expression)
        .HtmlAttributes(new Dictionary<string, object> {
            ["id"] = uniqueId, ["name"] = name
        });
}
```

### Option B: Standalone component (custom builder with IHtmlContent)

```csharp
public class MyWidgetBuilder<TModel> : IHtmlContent where TModel : class
{
    private readonly string _elementId;
    internal string ElementId => _elementId;

    public MyWidgetBuilder(string elementId) => _elementId = elementId;

    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
    {
        writer.Write($"<div id=\"{encoder.Encode(_elementId)}\" data-my-widget></div>");
    }
}
```

**Critical:** Call `plan.AddToComponentsMap()` for any input component that
participates in gather or validation. The map connects binding paths to
component IDs, vendors, and readExprs.

---

## Step 7: Gather (Already Done Per-Vendor)

Gather extensions are **shared per vendor** — you don't write new ones per component.
The existing `FusionGatherExtensions.Include<TComponent>()` works for any
`TComponent : FusionComponent, IInputComponent, new()`.

Usage in views:
```csharp
p.Post("/api/save", g =>
    g.Include<FusionDatePicker, MyModel>(m => m.StartDate)
     .Include<FusionNumericTextBox, MyModel>(m => m.Amount));
```

The `new()` constraint instantiates the component to read `Vendor` and `ReadExpr`.
No new gather code needed.

---

## Step 8: Tests (3 Layers)

### Layer 1: C# unit tests (snapshot + schema)

```csharp
[TestFixture]
public class WhenMutatingAFusionDatePicker : FusionTestBase
{
    [Test]
    public Task SetValue_produces_mutation()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionDatePicker>(m => m.StartDate)
             .SetValue(new DateTime(2024, 1, 15)));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Show_produces_call()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionDatePicker>(m => m.StartDate).Show());
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public void Value_returns_typed_source()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var source = p.Component<FusionDatePicker>(m => m.StartDate).Value();
            Assert.That(source, Is.TypeOf<TypedComponentSource<DateTime?>>());
        });
    }
}
```

### Layer 2: TS unit tests (vitest + jsdom)

Only needed if the component has unusual runtime behavior. Standard set-prop/call
mutations are covered by existing tests. Add tests for:
- New multi-arg patterns
- Component source reads
- Coercion edge cases

### Layer 3: Playwright browser tests

```csharp
[Test]
public async Task DomReadySetsDateValue()
{
    await NavigateTo("/Sandbox/DatePicker");
    await WaitForTraceMessage("booted", 10000);

    await Page.WaitForFunctionAsync(
        $"() => document.getElementById('{DateId}')?.ej2_instances?.[0]?.value != null",
        null, new() { Timeout = 5000 });

    AssertNoConsoleErrors();
}
```

---

## Checklist

```
□ 1. Component class — sealed, IComponent + IInputComponent, declares ReadExpr
□ 2. Event args — one class per JS event, typed properties
□ 3. Events singleton — TypedEventDescriptor registry
□ 4. Extensions — one method per JS API surface (SetPropMutation or CallMutation)
□ 5. Reactive extensions — wires events into plan
□ 6. Builder — renders HTML, registers in ComponentsMap
□ 7. Gather — already works (shared per vendor)
□ 8. C# unit tests — snapshot + schema validation
□ 9. Playwright tests — browser behavior verification
□ 10. Sandbox view — demonstrate in sandbox app
```

**Zero runtime changes. Zero TS changes. The plan carries everything.**

---

## Common Mistakes

| Mistake | Why it fails | Fix |
|---------|-------------|-----|
| Using `CallMutation` for property set | Runtime uses `root[method].apply()` which crashes on assignment | Use `SetPropMutation` for `prop = val` |
| Passing `source:` to `Emit()` for call mutations | Source lives in MethodArg now, not on MutateElementCommand | Wrap in `new SourceArg(source)` inside args array |
| Forgetting `coerce:` on SetPropMutation | JS receives string "42" instead of number 42 | Add `coerce: "number"` for numeric props |
| Wrong ReadExpr | Gather/validation reads wrong property | Check devtools: `el.ej2_instances[0].{readExpr}` |
| Missing `AddToComponentsMap` | Gather can't find the component at render time | Call in builder method |
| Using reflection for metadata | Violates architecture — must use instance properties | Implement `IInputComponent.ReadExpr` |
| Adding `if (vendor === "x")` in runtime | Runtime is dumb executor — plan carries vendor | Component extensions set vendor via `Emit()` |

---

## Reference: How Emit() Works

```
User DSL:       p.Component<FusionDatePicker>("dp").SetValue(date)
                                    │
Extension:      self.Emit(new SetPropMutation("value"), value: "2024-01-15")
                                    │
ComponentRef:   Pipeline.AddCommand(new MutateElementCommand(
                    target: "dp",
                    mutation: { kind: "set-prop", prop: "value" },
                    value: "2024-01-15",
                    vendor: "fusion"         ← from cached _instance.Vendor
                ))
                                    │
JSON Plan:      { "kind": "mutate-element", "target": "dp",
                  "mutation": { "kind": "set-prop", "prop": "value" },
                  "value": "2024-01-15", "vendor": "fusion" }
                                    │
Runtime:        root = resolveRoot(el, "fusion")  → el.ej2_instances[0]
                root["value"] = "2024-01-15"
```

For call mutations with args:

```
User DSL:       p.Component<TestWidget>("tw").SetItems(payload, x => x.Items)
                                    │
Extension:      self.Emit(new CallMutation("setItems",
                    args: [new SourceArg(new EventSource("evt.items"))]))
                                    │
JSON Plan:      { "kind": "mutate-element", "target": "tw",
                  "mutation": { "kind": "call", "method": "setItems",
                    "args": [{ "kind": "source",
                      "source": { "kind": "event", "path": "evt.items" } }] },
                  "vendor": "fusion" }
                                    │
Runtime:        root = resolveRoot(el, "fusion")  → el.ej2_instances[0]
                val = resolveArg(args[0], ctx)    → walk(ctx, "evt.items")
                root["setItems"].apply(root, [val])
```
