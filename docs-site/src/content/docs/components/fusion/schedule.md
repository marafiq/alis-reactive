---
title: FusionSchedule
description: Calendar and resource planning with sort, page, CRUD, and event wiring.
sidebar:
  order: 18
---

A calendar-style schedule for booking senior-living resources -- staff shift rosters, activity calendars, medical appointments. Fires a full family of action and navigation events and exposes component methods for data loading and CRUD. Non-input component: no `InputField` wrapper.

**Render as:** `@(Html.FusionSchedule(plan, "id", b => ...))` &nbsp; **Events:** `CellClicked`, `EventClicked`, `ActionBegin`, `ActionComplete`, `Navigating`, `PopupOpen`, `PopupClose`, `DataBound`, `EventRendered`

## How do I render a schedule and wire its main events?

Configure views, resources, event-field mappings, work hours, and time scale inside the builder. Chain `.Reactive(evt => evt.CellClicked, ...)` to handle empty-cell clicks (pass `StartTime`, `EndTime`, `GroupIndex` through `FromEvent` into a GET that loads a new-assignment form partial, then show a dialog). Chain `.Reactive(evt => evt.ActionComplete, ...)` guarded on `RequestType == "eventChanged"` to reload fresh data after Syncfusion finishes a CRUD operation internally.

```csharp
@(Html.FusionSchedule(plan, "shift-schedule", b =>
    {
        b.Width("100%"); b.Height("100%"); b.CurrentView(View.Week); b.SelectedDate(DateTime.Today);
        b.Views(new List<ScheduleView>
        {
            new ScheduleView { Option = View.Day },
            new ScheduleView { Option = View.Week },
            new ScheduleView { Option = View.WorkWeek },
            new ScheduleView { Option = View.Month },
            new ScheduleView { Option = View.Agenda },
        });
        b.Group(new ScheduleGroup { Resources = new string[] { "Shifts" } });
        b.Resources(new List<ScheduleResource>
        {
            new ScheduleResource
            {
                Field = "shiftId", Title = "Shift", Name = "Shifts",
                DataSource = FakeScheduleData.Shifts,
                TextField = "text", IdField = "id", ColorField = "color"
            }
        });
        b.EventSettings(new ScheduleEventSettings
        {
            EnableTooltip = true,
            Fields = new ScheduleField
            {
                Id = "id",
                Subject = new ScheduleFieldOptions { Name = "subject" },
                StartTime = new ScheduleFieldOptions { Name = "startTime" },
                EndTime = new ScheduleFieldOptions { Name = "endTime" },
                Description = new ScheduleFieldOptions { Name = "description" },
                IsAllDay = new ScheduleFieldOptions { Name = "isAllDay" },
            },
        });
        b.WorkHours(new ScheduleWorkHours { Highlight = true, Start = "06:00", End = "22:00" });
        b.StartHour("05:00"); b.EndHour("23:00");
        b.TimeScale(new ScheduleTimeScale { Enable = true, Interval = 60, SlotCount = 1 });
        b.ShowHeaderBar(true); b.ShowTimeIndicator(true); b.AllowDragAndDrop(true);
    })
    .Reactive(evt => evt.CellClicked, (args, p) =>
    {
        p.Element("event-trace").SetText(args, x => x.StartTime);
        p.Element("event-shift").SetText(args, x => x.GroupIndex);

        p.Get("/Sandbox/Components/Schedule/NewAssignmentForm")
         .Gather(g => g
             .FromEvent(args, x => x.StartTime, "startTime")
             .FromEvent(args, x => x.EndTime, "endTime")
             .FromEvent(args, x => x.GroupIndex, "shiftId")
             .Include<FusionDropDownList, PointInTimeScheduleModel>(m => m.SelectedFacilityId))
         .Response(r => r.OnSuccess(s => s.Into("new-assignment-content")));
        p.Component<FusionDialog>("new-assignment-dialog").Show();
    })
    .Reactive(evt => evt.ActionComplete, (args, p) =>
    {
        p.When(args, x => x.RequestType).Eq("eventChanged")
         .Then(t =>
         {
             t.Element("status").SetText("Saved — reloading...");
             t.Get("/api/schedule/assignments")
              .Gather(g => g
                  .Include<FusionDropDownList, PointInTimeScheduleModel>(m => m.SelectedFacilityId)
                  .Include(p.Component<FusionSchedule>("shift-schedule").CurrentView())
                  .Include(p.Component<FusionSchedule>("shift-schedule").SelectedDate(), "currentDate"))
              .Response(r => r.OnSuccess<ScheduleDataResponse>((json, s) =>
              {
                  s.Component<FusionSchedule>("shift-schedule")
                      .SetDataSource(json, j => j.Assignments);
                  s.Element("status").SetText("Refreshed after save");
              }));
         });
    }))
```

The sandbox view also wires `PopupOpen` (to cancel Syncfusion's built-in QuickInfo popup with `args.PreventDefault(t)`), `ActionBegin`, `Navigating`, `EventClicked`, `PopupClose`, `DataBound`, and `EventRendered`. See `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Schedule/Index.cshtml` for the full event surface.

## How do I load initial data after DOM ready?

The schedule never holds the full dataset. Fetch the initial slice in a `DomReady` trigger and push it in with `SetDataSource`. The server filters by facility, view, and date range on each request.

```csharp
@{ Html.On(plan, t => t.DomReady(p =>
{
    p.Get("/api/schedule/assignments?selectedFacilityId=mystery-manor")
     .Response(r => r.OnSuccess<ScheduleDataResponse>((json, s) =>
     {
         s.Component<FusionSchedule>("shift-schedule")
             .SetDataSource(json, j => j.Assignments);
         s.Element("status").SetText("Schedule loaded");
     }));
})); }
```

## Reference

| Extension | Description |
|---|---|
| `CurrentView()` | Reads the active view for conditions or gather |
| `SelectedDate()` | Reads the currently selected date |
| `SetDataSource(ResponseBody<T> source, Expression<Func<T, object?>> path)` | Replaces event data from a response body with a path selector |
| `SetDataSource(ResponseBody<T> source)` | Replaces event data with the entire response body |
| `AddEvent(ValueProducer data)` | Adds one or more events |
| `SaveEvent(ValueProducer data)` | Updates an existing event |
| `DeleteEvent(ValueProducer eventId)` | Deletes an event by ID |
| `OpenEditor(ValueProducer data, string action = "Add")` | Opens the built-in event editor programmatically |
| `CloseEditor()` | Closes the event editor |
| `RefreshEvents()` | Re-renders all events |
| `Print()` | Prints the current schedule view |
| `ScrollTo(string hour)` | Scrolls to a specific time |
