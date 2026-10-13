---
title: Bring Your Own Component
description: Declare any browser component once and every Alis.Reactive feature works with it.
---

Any component that already works in the browser can join a plan. You install,
bundle and construct it as usual. You declare it once in C#: one class whose
members are the object's own properties, methods and events. Every view then
uses it the way it uses a shipped component: `.Reactive`, its members, gather,
validation and partials. No framework JavaScript changes.

This page brings the sandbox's visit planner, a widget the sandbox wrote itself
with no knowledge of Alis.Reactive. Its declaration is
`BroughtComponents/VisitPlanner/VisitPlanner.cs`.

## 1. Construct It in Your Bundle

The app owns the widget. The framework raises `alis:mount` before it wires a piece
of HTML (the page at boot, a partial when it is injected) and `alis:unmount`
before it removes one, so you construct and destroy at the right moment.

```ts
import { VisitPlanner, visitPlannerHosts } from "./visit-planner";

document.addEventListener("alis:mount", event => {
  const { root } = (event as CustomEvent<MountEventDetail>).detail;
  for (const host of visitPlannerHosts(root)) {
    if (host.visitPlanner === undefined) new VisitPlanner(host);
  }
});

document.addEventListener("alis:unmount", event => {
  const { root } = (event as CustomEvent<MountEventDetail>).detail;
  for (const host of visitPlannerHosts(root)) {
    host.visitPlanner?.destroy();
  }
});
```

## 2. Declare It

One class, deriving `Component`, says where the object lives and declares each of
its members as a C# member. The vendor tells the framework how to find the object
on its element, how to subscribe to events, and what a listener receives.

```csharp
#nullable disable annotations
public sealed class VisitPlanner : Component, IInputComponent
{
    public static ComponentVendor VisitPlannerVendor { get; } = ComponentVendor.Define(
        "visitplanner",
        objectPath: "visitPlanner",
        subscribe: "on",
        unsubscribe: "off",
        payload: VendorEventPayload.Argument);

    public override ComponentVendor Vendor => VisitPlannerVendor;

    public Prop<IReadOnlyList<string>> Value => Prop<IReadOnlyList<string>>();
    public Prop<int> Capacity => Prop<int>();
    public Prop<string> Wing => Prop<string>();
    public Prop<IReadOnlyList<VisitSlot>> Slots => Prop<IReadOnlyList<VisitSlot>>();
    public Prop<IReadOnlyList<string>> OfferedDays => Prop<IReadOnlyList<string>>("slots[].day");
    public Prop<string> EmptyMessage => Prop<string>("labels.empty");
    public ReadOnly<int> Count => ReadOnly<int>();

    public TypedSource<int> Remaining() => Invoke<int>();
    public TypedSource<bool> IsBooked(Arg<string> slot) => Invoke<bool>(slot);
    public TypedSource<string> FirstOpen() => Invoke<string>();
    public void Select(Arg<string> slot) => Call(slot);
    public void Clear() => Call();
    public void Book(Arg<string> slot, Arg<decimal> visitors, Arg<string> note) => Call(slot, visitors, note);
    public void PrepareSummary() => CallAsync();
    public TypedSource<string> Summary() => InvokeAsync<string>();

    public TypedEvent<VisitPlannerWeekChangedArgs> WeekChanged => Event<VisitPlannerWeekChangedArgs>();
    public TypedEvent<VisitPlannerBeforeBookArgs> BeforeBook => Event<VisitPlannerBeforeBookArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: WeekChanged);

    // ... the full class declares more members of each kind
}
```

The JavaScript name of a member is its C# name camel-cased, so `Capacity` is
`capacity` and `IsBooked` is `isBooked`. Give a name only when it differs.

| JavaScript | Declared as |
| --- | --- |
| a property the page reads and writes | `public Prop<int> Capacity => Prop<int>();` |
| a property the page only reads | `public ReadOnly<int> Count => ReadOnly<int>();` |
| a nested property | `Prop<string>("labels.empty")` |
| a field of each item of a list, to read | `Prop<IReadOnlyList<string>>("slots[].day")` |
| a method | `public void Clear() => Call();` |
| a method with arguments | `public void Book(Arg<string> slot, Arg<decimal> visitors, Arg<string> note) => Call(slot, visitors, note);` |
| a method that returns a value | `public TypedSource<bool> IsBooked(Arg<string> slot) => Invoke<bool>(slot);` |
| a method that returns a promise | `CallAsync()`, `InvokeAsync<string>()` |
| a method whose JavaScript name differs | `Call(fieldName, name: "autoFitColumns")` |
| an argument the method always takes | `Call(key, Fixed("field"), name: "hideColumns")` |
| an event | `Event<VisitPlannerWeekChangedArgs>()` |
| an input's value and commit event | `ComponentInput.Of(Value, committedBy: WeekChanged)` |

Each `Arg<T>` parameter takes a literal or any value the page can read, in the
method's own order. `Fixed(..)` is for an argument the page never chooses, such
as the `"field"` key the grid's column methods always take.

`IInputComponent.Input`, implemented explicitly, is what makes it an input: the
value a form posts, gather reads and validation checks, and the event that
commits it. A form checks the field again whenever that event fires. The commit
event is whatever the component raises when a value is settled:
`change` for most, `valueChanged` for a rating, `selected` for an uploader, or a
name of your own, as `weekChanged` is here. The framework never assumes one.

An awaited method (`CallAsync`) holds the steps after its call until its promise
settles, and an awaited read (`InvokeAsync<T>`) is the value it settles with. A
member that returns a promise but is not declared awaited fails where the page
reads it, with a message that names the member, instead of reading the promise as
a value.

### Event payloads

An event payload is declared the same way, as a class deriving `JsObject`: what
it carries is `ReadOnly<T>`, what the component reads back after the handler is
`Prop<T>`, and what it can do is a method.

```csharp
public class VisitPlannerFullArgs : JsObject
{
    public ReadOnly<int> Capacity => ReadOnly<int>();
    public ReadOnly<string> Attempted => ReadOnly<string>();   // the time that could not be booked
    public void Waitlist() => Call();                          // puts that time on the waitlist
}

public class VisitPlannerBeforeBookArgs : JsObject
{
    public ReadOnly<VisitSlot> Slot => ReadOnly<VisitSlot>();
    public Prop<bool> Cancel => Prop<bool>();                  // the planner reads it back
}
#nullable restore annotations
```

A handler can write a member back (`args.Cancel.Set(true)`), and the component
reads it once the handler returns, as long as nothing in the handler waited
before the write.

The declaration and its payloads sit between `#nullable disable annotations` and
`#nullable restore annotations`, so a page can pass values of either nullability
to its members.

## 3. Render It

The render helper writes the component's own markup with the ID and name the
framework gives the field, and registers it as the field's input.

```csharp
public static RenderedComponent<VisitPlanner, TModel> VisitPlanner<TModel>(
    this InputBoundField<TModel, List<string>> field,
    int capacity)
    where TModel : class
{
    var html = new StringBuilder()
        .Append("<div class=\"visit-planner\" id=\"").Append(Encode(field.ElementId))
        .Append("\" data-name=\"").Append(Encode(field.BindingPath))
        .Append("\" data-capacity=\"").Append(capacity)
        .Append("\" data-value=\"").Append(Encode(string.Join(",", Booked(field))))
        .Append("\"></div>");
    return field.Render<VisitPlanner>(new HtmlString(html.ToString()));
}
```

A component that is not a form input renders against the plan with an ID you
choose: `plan.Render<TComponent>("my-id", markup)`.

## 4. Use It

Every view now has the whole DSL for it, with no further code. From the Family
Visits page:

```csharp
@{ Html.InputField(plan, m => m.VisitSlots, o => o.Required().Label("Visit times"))
    .VisitPlanner(capacity: 1)
    .Reactive(evt => evt.BeforeBook, (args, p) =>
        p.When(args.Slot.Get(x => x.Quiet)).Truthy()
            .Then(quiet =>
            {
                args.Cancel.Set(true);
                quiet.Element("quiet-slot").Text.Set(args.Slot.Get(x => x.Label));
                quiet.Element("quiet-hours").Show();
            })
            .Else(open => open.Element("quiet-hours").Hide()))
    .Reactive(evt => evt.WeekChanged, (args, p) =>
    {
        var planner = p.Component<VisitPlanner>(m => m.VisitSlots);
        p.Element("planned-count").Text.Set(args.Count);
        p.Element("visits-left").Text.Set(planner.Remaining());
    }); }
```

`p.Component<VisitPlanner>(..)` returns the planner itself. Its properties are
values and its methods are called directly; each argument is a literal or
anything the DSL can read, and a method's result is itself a value:

```csharp
var planner = p.Component<VisitPlanner>(m => m.VisitSlots);
planner.Select(planner.FirstOpen());

p.When(p.Component<VisitPlanner>(m => m.VisitSlots).IsBooked("sun-11")).Truthy()
    .Then(t => t.Element("brunch-status").Text.Set("Sunday brunch is booked."));

p.When(args.Added).NotEmpty()
    .Then(added => added.Component<VisitPlanner>(m => m.VisitSlots).Book(args.Added,
        added.Component<FusionNumericTextBox>(m => m.Visitors).Value,
        added.Component<NativeTextBox>(m => m.Note).Value));
```

A response fills its properties like any other value:

```csharp
.OnSuccess<ResidentWeekResponse>((week, s) =>
{
    var visitSlots = s.Component<VisitPlanner>(m => m.VisitSlots);
    visitSlots.Wing.Set(week.Get(x => x.Wing));
    visitSlots.Capacity.Set(week.Get(x => x.WeeklyVisits));
    visitSlots.Slots.Set(week.Get(x => x.Slots));
})
```

The Syncfusion and native components that ship with Alis.Reactive are declared
exactly this way, against the same public surface, and the ones that need
JavaScript of their own (the native check list, the smart text area, the
confirm dialog) are constructed on `alis:mount` as in step 1. The sandbox also
brings [Tom Select](https://tom-select.js.org), an npm package, the same way
(`BroughtComponents/TomSelect/TomSelect.cs`).
