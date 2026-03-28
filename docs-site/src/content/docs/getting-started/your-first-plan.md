---
title: Your First Reactive Plan
description: Build a Todo app step by step — model, validator, view — with zero JavaScript.
---

Build a Todo page step by step. By the end you'll have conditional visibility, client-side validation, and an HTTP post with a toast notification — all in C#.

## Prerequisites

Add the framework packages to your ASP.NET MVC project:

```xml
<!-- YourApp.csproj -->
<PackageReference Include="Alis.Reactive.Native" />
<PackageReference Include="Alis.Reactive.Fusion" />
<PackageReference Include="Alis.Reactive.FluentValidator" />
```

In `Program.cs`, register the validation extractor:

```csharp
using Alis.Reactive;
using Alis.Reactive.FluentValidator;

ReactivePlanConfig.UseValidationExtractor(
    new FluentValidationAdapter(type => (IValidator?)Activator.CreateInstance(type)));
```

In `_Layout.cshtml`, load the runtime (once, for all pages):

```html
<script type="module" src="~/js/alis-reactive.js" asp-append-version="true"></script>
```

## Step 1: The Model

Create `Models/TodoModel.cs`:

```csharp
public class TodoModel
{
    public string? Title { get; set; }
    public bool IsUrgent { get; set; }
    public DateTime? DueDate { get; set; }
}
```

## Step 2: The Validator

Create `Validators/TodoValidator.cs`:

```csharp
using Alis.Reactive.FluentValidator;
using FluentValidation;

public class TodoValidator : ReactiveValidator<TodoModel>
{
    public TodoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);

        WhenField(x => x.IsUrgent, () =>
        {
            RuleFor(x => x.DueDate).NotEmpty().WithMessage("Urgent todos need a due date");
        });
    }
}
```

`ReactiveValidator<T>` extends `AbstractValidator<T>`. `WhenField` works both server-side (FluentValidation's `.When()`) and client-side (extracted into the JSON plan as a conditional rule). One rule, two enforcement points, zero drift.

## Step 3: The Controller

Create `Controllers/TodoController.cs`:

```csharp
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

public class TodoController(IValidator<TodoModel> validator) : Controller
{
    public IActionResult Index() => View(new TodoModel());

    [HttpPost]
    public IActionResult Save([FromBody] TodoModel? model)
    {
        if (model == null)
            return BadRequest(new { errors = new Dictionary<string, string[]>
                { ["Title"] = new[] { "Request body is required." } } });

        var result = validator.Validate(model);

        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return BadRequest(new { errors });
        }

        return Ok(new { message = "Todo saved!" });
    }
}
```

Register the validator in `Program.cs` (FluentValidation's DI is built-in):

```csharp
builder.Services.AddScoped<IValidator<TodoModel>, TodoValidator>();
```

The `Save` endpoint runs the same `TodoValidator` server-side via DI. On validation failure it returns 400 with an `errors` dictionary — the runtime's `ValidationErrors("todo-form")` routes these to the correct fields.

## Step 4: The View

Create `Views/Todo/Index.cshtml`:

```csharp
@model TodoModel
@using Alis.Reactive.Native.Extensions
@using Alis.Reactive.Native.Components
@using Alis.Reactive.Builders.Requests
@using Alis.Reactive.Fusion.Components
@using Alis.Reactive.Fusion.AppLevel
@{
    var plan = Html.ReactivePlan<TodoModel>();
}

<h1>New Todo</h1>

<form id="todo-form">
    @{ Html.InputField(plan, m => m.Title, o => o.Required().Label("Title"))
        .NativeTextBox(b => b
            .CssClass("rounded-md border border-border px-3 py-1.5 text-sm")
            .Placeholder("What needs to be done?")); }

    @{ Html.InputField(plan, m => m.IsUrgent, o => o.Label("Urgent"))
        .NativeCheckBox(b => b
            .CssClass("h-4 w-4 rounded border-border text-accent")
            .Reactive(plan, evt => evt.Changed, (args, p) =>
            {
                p.When(args, a => a.Checked).Truthy()
                    .Then(t => t.Element("due-date-section").Show())
                    .Else(e => e.Element("due-date-section").Hide());
            })); }

    <div id="due-date-section" hidden>
        @{ Html.InputField(plan, m => m.DueDate, o => o.Label("Due Date"))
            .FusionDatePicker(b => b.Placeholder("Select due date")); }
    </div>

    @(Html.NativeButton("save-btn", "Save Todo")
        .CssClass("rounded-md bg-accent px-4 py-2 text-sm font-medium text-white hover:bg-accent/90")
        .Reactive(plan, evt => evt.Click, (args, p) =>
        {
            p.Post("/Todo/Save", g => g.IncludeAll())
             .Validate<TodoValidator>("todo-form")
             .Response(r => r
                .OnSuccess(s =>
                {
                    s.Component<FusionToast>()
                        .SetTitle("Todo")
                        .SetContent("Todo saved successfully")
                        .Success()
                        .Show();
                })
                .OnError(400, e => e.ValidationErrors("todo-form")));
        }))
</form>

@Html.RenderPlan(plan)
```

## What Just Happened

Four files — `TodoModel.cs`, `TodoValidator.cs`, `TodoController.cs`, `Index.cshtml` — produce a page with three interactive behaviors:

- **Conditional visibility.** `Html.InputField` renders the checkbox bound to `m => m.IsUrgent`. `.Reactive()` wires its `change` event. `When/Then/Else` evaluates `args.Checked` (typed as `bool?`) and shows or hides the due date section.

- **Client-side validation.** `Validate<TodoValidator>` extracts the FluentValidation rules at render time and embeds them in the JSON plan. The `WhenField(x => x.IsUrgent)` conditional rule only fires when the checkbox is checked. Validation runs before the HTTP request — if it fails, the request never fires.

- **HTTP post with toast.** `IncludeAll()` gathers `Title`, `IsUrgent`, and `DueDate` from all registered components. On success, `FusionToast` shows a notification. On 400, `ValidationErrors` routes server errors to the form fields.

Native checkbox and Syncfusion DatePicker in the same form, same fluent API. Zero JavaScript.

You can see this example running in the sandbox at `/Sandbox/Todo`.

## Next Steps

- [The Contract](../../architecture/the-contract/) -- how the JSON plan works
- [Features](../../csharp-modules/plan-and-entries/) -- full reference for plans, triggers, conditions, and HTTP
