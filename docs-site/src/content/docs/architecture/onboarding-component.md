---
title: Onboarding a Syncfusion Component
description: Current-source guide for adding a typed Syncfusion EJ2 component slice without changing the runtime.
sidebar:
  order: 10
---

Adding a Syncfusion component is a vertical-slice change. The slice declares the
component type, the public component-specific methods, the HTML factory, events,
Reactive Plan wiring, and behavior tests. The TypeScript runtime should not need
new component-specific branches when existing BrowserObject members cover the
component.

## Pick The Component Shape

Ask one question first: **does this component expose a form value that
participates in validation or request gather?**

| Shape | Use when | Contract | Examples |
|-------|----------|----------|----------|
| Input component | The user edits a value such as `value`, `checked`, or `filesData`. | `FusionComponent, IInputComponent` | DropDownList, DatePicker, CheckBox, FileUpload |
| Non-input component | The component is a container, navigation surface, template host, or interaction target. | `FusionComponent` | Accordion, Tab, Dialog, Tooltip |
| App-level component | The component has a well-known layout instance. | `IAppLevelComponent` plus app-level extensions | Toast, Confirm |

Input components are rendered through `Html.InputField(...)`, register a value
member, participate in validation and `IncludeAll()` gather, and normally expose
a `Value()` typed source. Non-input components render directly through
`Html.FusionXxx(...)`, take an explicit controlled element ID, and do not expose
`Value()` unless there is a real readable component value.

## Verify Syncfusion First

Before adding API surface, verify the Syncfusion member in a sandbox or browser
console:

```javascript
const el = document.getElementById("ComponentId");
const ej2 = el.ej2_instances[0];

ej2.enabled = false;
typeof ej2.showPopup;
```

Add only members that work in the browser. If a documented Syncfusion method is
present but unusable for this framework, omit it and leave a short source comment
near the extension. Keep quirks encapsulated in the component slice; do not leak
vendor internals into public docs unless the developer must know them to avoid
misuse.

## Input Component Slice

Use the existing input components as the template. `FusionDropDownList` is a
good current example.

```
Alis.Reactive.Fusion/Components/FusionXxx/
├── FusionXxx.cs
├── FusionXxxExtensions.cs
├── FusionXxxHtmlExtensions.cs
├── FusionXxxEvents.cs
├── FusionXxxReactiveExtensions.cs
└── Events/
    └── FusionXxxOnChanged.cs
```

### Component Type

The component type is a sealed marker used by `ComponentRef<TComponent, TModel>`.
It declares the Fusion vendor through `FusionComponent` and the readable value
member through `IInputComponent`.

```csharp
public sealed class FusionXxx : FusionComponent, IInputComponent
{
    internal static InputComponentRegistrationProfile Registration { get; } =
        InputComponentRegistrationProfile.For(new FusionXxx(), "xxx");

    public string ValueMember => "value";
}
```

Use `"checked"` for boolean-style controls and the exact Syncfusion member for
special cases such as FileUpload's `"filesData"`.

### Component Reactions And Reads

Extensions on `ComponentRef<FusionXxx, TModel>` declare public component API.
Use `ComponentProperty<T>` for readable/writable members, `ComponentMethod` for
method calls, and `ValueExpression` for literal or sourced values.

```csharp
private static readonly FusionXxx Component = new FusionXxx();
private static readonly ComponentProperty<string> ValueProperty =
    ComponentProperty<string>.Named(Component.ValueMember);
private static readonly ComponentMethod DataBindMethod =
    ComponentMethod.Named("dataBind");

public static ComponentRef<FusionXxx, TModel> SetValue<TModel>(
    this ComponentRef<FusionXxx, TModel> self,
    string? value)
    where TModel : class
    => self.EmitSet(ValueProperty, ValueExpression.LiteralRaw(value, Shape.String));

public static ComponentRef<FusionXxx, TModel> DataBind<TModel>(
    this ComponentRef<FusionXxx, TModel> self)
    where TModel : class
    => self.EmitCall(DataBindMethod);

public static TypedComponentSource<string> Value<TModel>(
    this ComponentRef<FusionXxx, TModel> self)
    where TModel : class
    => self.Read(ValueProperty);
```

Use `EmitSet()` and `EmitCall()` only through component-specific extension
methods. Do not add stringly public APIs just to reach Syncfusion dynamically.
If there are many similar events or members, document two representative
examples and let IntelliSense show the full surface.

### HTML Factory

The HTML extension registers the input component before rendering. That
registration is what makes validation and gather see the component.

```csharp
public static void FusionXxx<TModel, TProp>(
    this InputBoundField<TModel, TProp> setup,
    Action<XxxBuilder> build)
    where TModel : class
{
    setup.RegisterInputComponent(FusionXxx.Registration);

    var builder = setup.Helper.EJS().XxxFor(setup.Expression)
        .HtmlAttributes(new Dictionary<string, object>
        {
            ["id"] = setup.ElementId,
            ["name"] = setup.BindingPath
        });

    build(builder);
    setup.Render(builder.Render());
}
```

Some Syncfusion components need a different factory shape. Follow the existing
component with the closest Syncfusion API rather than inventing a shared helper.

### Events And Reactive Wiring

`FusionXxxEvents` exposes typed events. `.Reactive()` selects one of those
events and delegates to `ComponentEventOnboarding.Wire(...)`.

```csharp
public sealed class FusionXxxEvents
{
    public static readonly FusionXxxEvents Instance = new FusionXxxEvents();
    private FusionXxxEvents() { }

    public TypedEvent<FusionXxxChangedArgs> Changed =>
        new TypedEvent<FusionXxxChangedArgs>("change", new FusionXxxChangedArgs());
}
```

```csharp
public static XxxBuilder Reactive<TModel, TArgs>(
    this XxxBuilder builder,
    ReactivePlan<TModel> plan,
    Func<FusionXxxEvents, TypedEvent<TArgs>> eventSelector,
    Action<TArgs, PipelineBuilder<TModel>> pipeline)
    where TModel : class
{
    var typedEvent = eventSelector(FusionXxxEvents.Instance);
    var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
    var componentId = (string)attrs["id"];

    ComponentEventOnboarding.Wire(
        plan,
        componentId,
        new FusionXxx().Vendor,
        typedEvent,
        pipeline);

    return builder;
}
```

Event args are plain typed payload classes. Put event-specific payload helper
methods next to the args only when they express real Syncfusion behavior.

## Non-Input Component Slice

Use `FusionAccordion` as the current template.

```
Alis.Reactive.Fusion/Components/FusionXxx/
├── FusionXxx.cs
├── FusionXxxExtensions.cs
├── FusionXxxHtmlExtensions.cs
├── FusionXxxBuilder.cs
├── FusionXxxEvents.cs
├── FusionXxxReactiveExtensions.cs
└── Events/
    └── FusionXxxOnSelected.cs
```

The component marker does not implement `IInputComponent`:

```csharp
public sealed class FusionXxx : FusionComponent
{
}
```

The HTML factory takes the plan and an explicit controlled element ID, renders
the Syncfusion builder, and returns a small wrapper builder that carries the plan
and element ID for `.Reactive()` chaining.

```csharp
public static FusionXxxBuilder<TModel> FusionXxx<TModel>(
    this IHtmlHelper<TModel> html,
    ReactivePlan<TModel> plan,
    string elementId,
    Action<XxxBuilder> build)
    where TModel : class
{
    var builder = html.EJS().Xxx(elementId);
    build(builder);

    return new FusionXxxBuilder<TModel>(plan, elementId, builder.Render());
}
```

Non-input component extensions still use `ComponentRef<TComponent, TModel>` and
`EmitCall()` / `EmitSet()`. They do not register validation, do not participate
in gather, and normally do not expose `Value()`.

## What Should Not Change

Do not change these layers for a normal component onboarding:

| Layer | Why |
|-------|-----|
| `Alis.Reactive.Assets/runtime/execution/reactions/execute.ts` | Existing `set`, `call`, `dispatch`, `request`, `branch`, and related reaction nodes already execute component behavior. |
| `Alis.Reactive.Assets/runtime/browser-objects/` | BrowserObject contracts centralize component/plugin property and method access. |
| `Alis.Reactive.Assets/runtime/types/plan.ts` | Generated from the C# plan domain; component slices should use existing contract nodes. |
| Core plan model | Add a plan concept only when the public DSL graph proves a new framework behavior, not because one Syncfusion component needs a convenience wrapper. |

If a component appears to need runtime changes, stop and write down the missing
DSL concept first. Most Syncfusion onboarding work should stay inside the
component slice and its behavior tests.

## Behavior Proof

Each component needs focused proof at the surfaces it changes:

| Surface | Proof |
|---------|-------|
| C# plan/API | Unit or snapshot test that the public extension writes the expected `ReactionGraph` and BrowserObject contract. |
| Sandbox view | A real Razor example that uses the public API naturally. |
| Browser behavior | Focused Playwright test through `scripts/playwright.sh --filter "..."` for the user-visible behavior. |

Run `scripts/build.sh` or the narrower command from `docs/developer-cli.md`
after C# changes. Run Playwright only when behavior, markup, runtime assets, or
the sandbox page changed. Comment/prose-only documentation changes do not need
Playwright.

## Common Mistakes

| Mistake | Correct approach |
|---------|------------------|
| Adding a generic shared component base because slices look similar. | Keep the vertical slice unless repeated code hides a real invariant. |
| Registering a non-input component for gather. | Only `IInputComponent` with a real `ValueMember` belongs in input registration. |
| Exposing every Syncfusion method. | Expose the small set that has proven framework value. |
| Leaking Syncfusion quirks into public docs. | Encapsulate quirks in the slice; public docs should name behavior, not vendor workaround mechanics. |
| Adding runtime branches for a component. | Use BrowserObject contracts and existing reaction nodes. |
| Writing a helper that makes one test shorter but hides the behavior. | Prefer direct test flow unless the helper is truly reusable and intent-revealing. |

**Previous:** [Plan Composition](../plan-composition/) — how multiple plans merge and compose on a single page.
