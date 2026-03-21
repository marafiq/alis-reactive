---
title: Component Model
description: How Reactive works with both native HTML inputs and Syncfusion components through one API.
sidebar:
  order: 3
---

You've seen `NativeCheckBox` and `FusionDatePicker` used in the same form, with the same fluent API. But they have completely different DOM structures. A native checkbox is `el.checked`. A Syncfusion date picker is `el.ej2_instances[0].value`. How does one framework handle both?

## The Problem

Every component vendor has its own way of doing things:

```javascript
// Native HTML input
const value = document.getElementById("name").value;

// Syncfusion EJ2 component
const value = document.getElementById("facility").ej2_instances[0].value;
```

If the framework had vendor-specific code paths — `if (vendor === "fusion") { ... }` scattered through the runtime — adding a new vendor or component would mean touching runtime code everywhere. That doesn't scale.

## The Solution: Three Metadata Fields

Each component declares three things about itself. That's all the runtime needs.

```csharp
public sealed class NativeCheckBox : NativeComponent, IInputComponent
{
    public string Vendor => "native";
    public string ReadExpr => "checked";
}

public sealed class FusionDatePicker : FusionComponent, IInputComponent
{
    public string Vendor => "fusion";
    public string ReadExpr => "value";
}
```

`Vendor` tells the runtime which root object to resolve. `ReadExpr` tells it which property to read. The component class is a sealed, parameterless type — no runtime state, just metadata.

When you render the component in a view, it registers this metadata in the plan:

```csharp
Html.InputField(plan, m => m.IsUrgent, o => o.Label("Urgent"))
    .NativeCheckBox(b => b.CssClass("h-4 w-4"));
```

The plan now knows: `IsUrgent` → element ID `...__IsUrgent`, vendor `"native"`, readExpr `"checked"`.

## How Does the Runtime Use This?

The runtime has one function that resolves the vendor root:

| Vendor | Root resolves to |
|--------|-----------------|
| `"native"` | The DOM element itself |
| `"fusion"` | `el.ej2_instances[0]` |

Then it walks the `readExpr` on that root. `"checked"` on a native element → `el.checked`. `"value"` on a Fusion root → `ej2.value`. Same code path, different root.

Writing a value works the same way. When you write:

```csharp
p.Component<NativeCheckBox>(m => m.IsUrgent).SetChecked(true);
```

The plan carries `{ prop: "checked", vendor: "native" }`. The runtime resolves the native root and does `el.checked = true`.

When you write:

```csharp
p.Component<FusionDropDownList>(m => m.Facility).SetValue("Sunrise");
```

The plan carries `{ prop: "value", vendor: "fusion" }`. The runtime resolves the Fusion root and does `ej2.value = "Sunrise"`.

Same pattern. Different vendor. Zero branching in the runtime.

## What About Events?

Same story. A native checkbox fires `"change"` on the DOM element. A Syncfusion dropdown fires `"change"` on its EJ2 instance. The plan carries the vendor, so the runtime knows where to attach the listener:

```csharp
.NativeCheckBox(b => b.Reactive(plan, evt => evt.Changed, (args, p) => { ... }))
// plan: { jsEvent: "change", vendor: "native" } → addEventListener on el

.DropDownList(b => b.Reactive(plan, evt => evt.Changed, (args, p) => { ... }))
// plan: { jsEvent: "change", vendor: "fusion" } → addEventListener on ej2
```

The developer doesn't think about this. `.Reactive()` reads the vendor from the component type automatically.

## What About Gathering Values?

When you write `g.IncludeAll()` in an HTTP pipeline, the runtime iterates the components registry in the plan, resolves each component's vendor root, walks its `readExpr`, and collects the value. Native and Fusion components are gathered the same way.

```csharp
p.Post("/api/save", g => g.IncludeAll())
```

The runtime reads:
- `Title` → vendor `"native"`, readExpr `"value"` → `el.value`
- `IsUrgent` → vendor `"native"`, readExpr `"checked"` → `el.checked`
- `DueDate` → vendor `"fusion"`, readExpr `"value"` → `ej2.value`

One loop, no special cases.

## Adding a New Component

What happens when you need a new component — say `FusionTimePicker`?

You write a C# class:

```csharp
public sealed class FusionTimePicker : FusionComponent, IInputComponent
{
    public string Vendor => "fusion";
    public string ReadExpr => "value";
}
```

Then a builder, extensions, and a reactive wiring — the vertical slice. The runtime doesn't change. It already knows how to resolve `"fusion"` roots and walk `"value"`. The plan carries the metadata. The runtime executes it.

That's the vendor-agnostic model: components declare metadata, the plan carries it, the runtime resolves and executes. One pattern, any vendor, zero runtime changes.
