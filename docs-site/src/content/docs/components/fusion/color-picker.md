---
title: FusionColorPicker
description: Hex color picker for facility branding.
sidebar:
  order: 2
---

A color picker for facility branding -- theme color, accent color, palette choices. Bound to a `string?` that holds a hex value like `"#e11d48ff"`.

**Model type:** `string?` &nbsp; **ValueMember:** `"value"` &nbsp; **Events:** `Changed`

## How do I render one and react to changes?

Start the chain with `Html.InputField(plan, m => m.ThemeColor)`, then call `.FusionColorPicker(b => b.Reactive(plan, evt => evt.Changed, ...))`. The `Changed` handler receives typed event args (`args.Value` is the newly picked hex) and a pipeline builder for wiring conditions, component reads, and element updates -- all in one place.

```csharp
@{ Html.InputField(plan, m => m.ThemeColor, o => o.Label("Theme Color"))
    .FusionColorPicker(b => b
        .Reactive(plan, evt => evt.Changed, (args, p) =>
        {
            p.Element("change-value").SetText(args, x => x.Value);
            p.When(args, x => x.Value).Eq("#e11d48ff")
                .Then(t => t.Element("args-condition").SetText("rose selected"))
                .Else(e => e.Element("args-condition").SetText("other color"));
            var comp = p.Component<FusionColorPicker>(m => m.ThemeColor);
            p.When(comp.Value()).NotEmpty()
                .Then(then =>
                {
                    then.Element("selected-indicator").Show();
                    then.Element("selected-indicator").SetText("color active");
                })
                .Else(else_ =>
                {
                    else_.Element("selected-indicator").Hide();
                });
        })); }
```

## How do I read the current color in a condition?

`p.Component<FusionColorPicker>(m => m.Color).Value()` returns a typed source you can feed into `When(...)` anywhere in the plan -- outside the change handler, inside a button click, whatever.

```csharp
@(Html.NativeButton("check-accent-btn", "Check Accent Color")
    .Reactive(plan, evt => evt.Click, (args, p) =>
    {
        var comp = p.Component<FusionColorPicker>(m => m.AccentColor);
        p.When(comp.Value()).IsEmpty()
            .Then(t => t.Element("component-read-result").SetText("no accent color set"))
            .Else(e => e.Element("component-read-result").SetText("accent color is set"));
    }))
```

## Reference

| Extension | Description |
|---|---|
| `SetValue(string?)` | Sets the color value as a hex string (e.g. `"#ff0000"`), or `null` to clear |
| `Toggle()` | Toggles the ColorPicker popup open or closed |
| `Disable(bool)` | Sets the disabled state |
| `Value()` | Reads the current hex color value as a typed source for conditions and gather |
