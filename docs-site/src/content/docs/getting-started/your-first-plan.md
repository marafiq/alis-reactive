---
title: Your First Reactive Plan
description: Build one reactive Razor view with inputs, an event, validation, HTTP, and a toast.
---

This page builds one small Todo form. It uses the public DSL only.

## Install Packages

Use the same package version for each Alis.Reactive package.

```xml
<PackageReference Include="AlisReactive.Native" Version="..." />
<PackageReference Include="AlisReactive.Fusion" Version="..." />
<PackageReference Include="AlisReactive.FluentValidator" Version="..." />
```

If you use the design system directly, add:

```xml
<PackageReference Include="AlisReactive.DesignSystem" Version="..." />
```

## Register Validation

Register the FluentValidation bridge in `Program.cs`.

```csharp
using Alis.Reactive.FluentValidator;

builder.Services.AddReactiveFluentValidation(validation =>
    validation.AddFromAssemblyContaining<Program>());
```

## Load Browser Assets

Package builds copy versioned assets into `wwwroot`. Link the copied filenames
from `_Layout.cshtml`.

```html
<link rel="stylesheet" href="~/css/design-system.{version}.css" asp-append-version="true" />
<link rel="stylesheet" href="~/css/syncfusion.{version}.css" asp-append-version="true" />

<script src="https://cdn.syncfusion.com/ej2/32.2.8/dist/ej2.min.js"></script>
@Html.EJS().ScriptManager()
@Html.FusionToast()
<script src="~/scripts/alis-reactive.{version}.js" asp-append-version="true"></script>
```

Replace `{version}` with the package version you installed.

## Create the Model

```csharp
public sealed class TodoModel
{
    public string? Title { get; set; }
    public bool IsUrgent { get; set; }
    public DateTime? DueDate { get; set; }
}
```

## Create the Validator

`ClientRule(...)` records metadata for browser validation and also adds the
server-side FluentValidation rule.

```csharp
using Alis.Reactive.FluentValidator;

public sealed class TodoValidator : ReactiveValidator<TodoModel>
{
    public TodoValidator()
    {
        ClientRule(x => x.Title)
            .Required("'Title' is required.")
            .MaxLength(200, "'Title' must be at most 200 characters.");

        WhenField(x => x.IsUrgent, () =>
        {
            ClientRule(x => x.DueDate)
                .Required("Urgent todos need a due date.");
        });
    }
}
```

Plain `RuleFor(...)` is server-only. Use `ClientRule(...)` for rules that must
be in the Reactive Plan.

## Add the Endpoint

```csharp
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

public sealed class TodoController(IValidator<TodoModel> validator) : Controller
{
    public IActionResult Index() => View(new TodoModel());

    [HttpPost]
    public IActionResult Save([FromBody] TodoModel model)
    {
        var result = validator.Validate(model);

        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            return BadRequest(new { errors });
        }

        return Ok(new { message = "Todo saved." });
    }
}
```

## Add the View

The view creates the plan, renders inputs, wires events, and renders the plan at
the end.

```csharp
@model TodoModel
@using Alis.Reactive.Native.Extensions
@using Alis.Reactive.Native.Components
@using Alis.Reactive.Fusion.Components
@using Alis.Reactive.Fusion.AppLevel

@{
    var plan = Html.ReactivePlan<TodoModel>();
}

<form id="todo-form">
    @{ Html.InputField(plan, m => m.Title, o => o.Label("Title").Required())
        .NativeTextBox(b => b.Placeholder("What needs to be done?")); }

    @{ Html.InputField(plan, m => m.IsUrgent, o => o.Label("Urgent"))
        .NativeCheckBox(b => b.Reactive(plan, evt => evt.Changed, (args, p) =>
        {
            p.When(args, x => x.Checked).Truthy()
             .Then(t => t.Element("due-date").Show())
             .Else(e => e.Element("due-date").Hide());
        })); }

    <div id="due-date" hidden>
        @{ Html.InputField(plan, m => m.DueDate, o => o.Label("Due Date"))
            .FusionDatePicker(b => b.Placeholder("Select due date")); }
    </div>

    @(Html.NativeButton("save-btn", "Save")
        .Reactive(plan, evt => evt.Click, (_, p) =>
        {
            p.Post("/Todo/Save", g => g.IncludeAll())
             .Validate<TodoValidator>("todo-form")
             .Response(r => r
                .OnSuccess(s =>
                {
                    s.Component<FusionToast>()
                     .SetTitle("Todo")
                     .SetContent("Todo saved.")
                     .Success()
                     .Show();
                })
                .OnError(400, e => e.ValidationErrors("todo-form")));
        }))
</form>

@Html.RenderPlan(plan)
```

## What Happened

`Html.ReactivePlan<TModel>()` creates the plan for this view.

`Html.InputField(...)` registers each model-bound input. The runtime later uses
those registrations for gather and validation.

`.Reactive(...)` wires a typed component event. The checkbox payload exposes
`Checked`, so the condition reads `args.Checked`.

`p.Post(...).Gather(...)` sends values read from registered inputs. The
`Validate<TValidator>(...)` call runs client validation before the request.

`Response(...)` routes success and error outcomes. The success path writes to
the app-level `FusionToast`. The error path displays server validation errors.
