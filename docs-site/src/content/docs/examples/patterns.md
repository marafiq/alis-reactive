---
title: Patterns of Reactivity
description: Small, complete, copy-paste-ready code snippets for every common reactive pattern.
---

Each pattern below is a self-contained snippet you can drop into a `.cshtml` view. All examples are taken from the Sandbox app — they compile and run.

---

## Run code when the page loads

```csharp
Html.On(plan, t => t.DomReady(p =>
{
    p.Element("step-1").AddClass("text-green-600");
    p.Element("step-1").SetText("dom-ready fired");
    p.Dispatch("init");
}));
```

---

## Listen for a named event

```csharp
Html.On(plan, t => t.CustomEvent("init", p =>
{
    p.Element("step-2").AddClass("text-green-600");
    p.Element("step-2").SetText("\"init\" received");
}));
```

---

## Dispatch and consume a typed payload

```csharp
// Dispatch
p.Dispatch("resident-loaded", new ResidentPayload
{
    Name = "Jane Doe",
    RoomNumber = 204
});

// Listen
Html.On(plan, t => t.CustomEvent<ResidentPayload>("resident-loaded", (payload, p) =>
{
    p.Element("name-display").SetText(payload, x => x.Name);
    p.Element("room-display").SetText(payload, x => x.RoomNumber);
}));
```

---

## React to a checkbox change

```csharp
Html.InputField(plan, m => m.HasDietaryRestrictions, o => o.Label("Has Dietary Restrictions"))
    .NativeCheckBox(b => b
        .Reactive(plan, evt => evt.Changed, (args, p) =>
        {
            p.When(args, a => a.Checked).Truthy()
                .Then(t => t.Element("restrictions-panel").Show())
                .Else(e => e.Element("restrictions-panel").Hide());
        }));
```

---

## React to a dropdown change

```csharp
Html.InputField(plan, m => m.Category, o => o.Label("Category"))
    .DropDownList(b => b
        .DataSource(categories)
        .Placeholder("Select a category")
        .Reactive(plan, evt => evt.Changed, (args, p) =>
        {
            p.Element("selected-value").SetText(args, x => x.Value);
        }));
```

---

## Read a component's value from a button click

```csharp
@(Html.NativeButton("check-btn", "Check Value")
    .Reactive(plan, evt => evt.Click, (args, p) =>
    {
        var comp = p.Component<NativeTextBox>(m => m.ResidentName);
        p.When(comp.Value()).IsEmpty()
            .Then(t => t.Element("warning").SetText("name is required"))
            .Else(e => e.Element("warning").SetText("name set"));
    }))
```

---

## Branch with When / Then / ElseIf / Else

```csharp
Html.On(plan, t => t.CustomEvent<ScorePayload>("scored", (args, p) =>
{
    p.When(args, x => x.Score).Gt(100)
        .Then(t => t.Element("tier").SetText("gold"))
        .ElseIf(args, x => x.Score).Gt(50)
        .Then(t => t.Element("tier").SetText("silver"))
        .ElseIf(args, x => x.Score).Gt(10)
        .Then(t => t.Element("tier").SetText("bronze"))
        .Else(e => e.Element("tier").SetText("none"));
}));
```

---

## Combine conditions with And

```csharp
p.When(args, x => x.Active).Truthy()
    .And(args, x => x.Count).Gt(5)
    .Then(t => t.Element("qualified").Show())
    .Else(e => e.Element("qualified").Hide());
```

---

## GET data on page load

```csharp
Html.On(plan, t => t.DomReady(p =>
{
    p.Get("/api/residents")
     .WhileLoading(l => l.Element("spinner").Show())
     .Response(r => r
        .OnSuccess<ResidentsResponse>((json, s) =>
        {
            s.Element("spinner").Hide();
            s.Element("first-name").SetText(json, x => x.First);
            s.Element("second-name").SetText(json, x => x.Second);
        }));
}));
```

---

## POST form data with IncludeAll

```csharp
@(Html.NativeButton("save-btn", "Save")
    .CssClass("rounded-md bg-accent px-4 py-2 text-sm font-medium text-white")
    .Reactive(plan, evt => evt.Click, (args, p) =>
    {
        p.Post("/api/residents", g => g.IncludeAll())
         .WhileLoading(l => l.Element("spinner").Show())
         .Response(r => r
            .OnSuccess(s =>
            {
                s.Element("spinner").Hide();
                s.Element("status").SetText("Saved");
            })
            .OnError(400, e =>
            {
                e.Element("spinner").Hide();
                e.Element("error").SetText("Validation failed");
            }));
    }))
```

---

## POST with toast notification on success

```csharp
p.Post("/api/residents", g => g.IncludeAll())
 .Validate<ResidentValidator>("resident-form")
 .Response(r => r
    .OnSuccess(s =>
    {
        s.Component<FusionToast>()
            .SetTitle("Resident Saved")
            .SetContent("Intake submitted successfully")
            .Success()
            .Show();
    })
    .OnError(400, e => e.ValidationErrors("resident-form")));
```

---

## Validate before submitting

```csharp
p.Post("/api/residents", g => g.IncludeAll())
 .Validate<ResidentValidator>("resident-form")
 .Response(r => r
    .OnSuccess(s => s.Element("result").SetText("Saved"))
    .OnError(400, e => e.ValidationErrors("resident-form")));
```

---

## Confirm before a destructive action

```csharp
@(Html.NativeButton("delete-btn", "Delete")
    .CssClass("rounded-md bg-red-600 px-4 py-2 text-sm text-white")
    .Reactive(plan, evt => evt.Click, (args, p) =>
    {
        p.Confirm("Are you sure you want to delete resident #42?")
         .Then(t =>
         {
            t.Delete("/api/residents/42")
             .WhileLoading(l => l.Element("spinner").Show())
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("spinner").Hide();
                 s.Element("status").SetText("Deleted");
             }));
         });
    }))
```

---

## Cascade dropdowns — parent reloads child

```csharp
Html.InputField(plan, m => m.Country, o => o.Label("Country"))
    .DropDownList(b => b
        .DataSource(countries)
        .Reactive(plan, evt => evt.Changed, (args, p) =>
        {
            p.Get("/api/cities")
             .Gather(g => g.Include<FusionDropDownList, LocationModel>(m => m.Country))
             .Response(r => r.OnSuccess<CityLookupResponse>((json, s) =>
             {
                 s.Component<FusionDropDownList>(m => m.City)
                     .SetDataSource(json, j => j.Cities)
                     .DataBind();
             }));
        }));
```

---

## Chain sequential requests

```csharp
p.Get("/api/residents")
 .Response(r => r
    .OnSuccess<ResidentsResponse>((json, s) =>
    {
        s.Element("resident-name").SetText(json, x => x.First);
    })
    .Chained(c => c
        .Get("/api/facilities")
        .Response(r2 => r2.OnSuccess<FacilitiesResponse>((json2, s2) =>
        {
            s2.Element("facility-name").SetText(json2, x => x.First);
        }))));
```

---

## Fire multiple requests in parallel

```csharp
Html.On(plan, t => t.DomReady(p =>
{
    p.Element("spinner").Show();
    p.Parallel(
        a => a.Get("/api/residents")
              .Response(r => r.OnSuccess<ResidentsResponse>((json, s) =>
              {
                  s.Element("resident-name").SetText(json, x => x.First);
              })),
        b => b.Get("/api/facilities")
              .Response(r => r.OnSuccess<FacilitiesResponse>((json, s) =>
              {
                  s.Element("facility-name").SetText(json, x => x.First);
              }))
    ).OnAllSettled(s =>
    {
        s.Element("spinner").Hide();
        s.Element("status").SetText("All loaded");
    });
}));
```

---

## Listen to Server-Sent Events (SSE)

```csharp
Html.On(plan, t => t.ServerPush<FacilityAlert>("/api/facility-alerts", "facility-alert",
    (alert, p) =>
{
    p.Element("alert-message").SetText(alert, x => x.Message);
    p.Element("alert-level").SetText(alert, x => x.Level);
}));
```

---

## Listen to SignalR hub methods

```csharp
Html.On(plan, t => t.SignalR<NotificationPayload>("/hubs/notifications", "ReceiveNotification",
    (payload, p) =>
{
    p.Element("notif-count").SetText(payload, x => x.Count);
    p.Element("notif-message").SetText(payload, x => x.Message);
}));
```

---

## Inject a partial view into the page

```csharp
p.Get("/api/residents/details")
 .Response(r => r.OnSuccess(s => s.Into("details-container")));
```

---

## Open a drawer with loaded content

```csharp
p.Element("alis-drawer-title").SetText("Resident Details");
p.Component<NativeDrawer>().SetSize(DrawerSize.Sm);
p.Get("/api/residents/details")
 .Response(r => r.OnSuccess(s => s.Into("alis-drawer-content")));
p.Component<NativeDrawer>().Open();
```

---

## Show a loading overlay

```csharp
p.Component<NativeLoader>()
    .SetTarget("form-section")
    .SetTimeout(10000)
    .Show();

// After request completes:
s.Component<NativeLoader>().Hide();
```

---

## Set a component's value at runtime

```csharp
// Text input
p.Component<NativeTextBox>(m => m.ResidentName).SetValue("Jane Doe");

// Dropdown
p.Component<FusionDropDownList>(m => m.Category).SetValue("Electronics");

// Numeric
p.Component<FusionNumericTextBox>(m => m.Amount).SetValue(42m);

// Date
p.Component<FusionDatePicker>(m => m.AdmissionDate).SetValue(DateTime.Today);

// Checkbox
p.Component<NativeCheckBox>(m => m.IsActive).SetChecked(true);
```

---

## Echo a component's live value into a display element

```csharp
var comp = p.Component<FusionDropDownList>(m => m.Category);
p.Element("value-echo").SetText(comp.Value());
```
