---
title: Bring Your Own Component
description: Declare any browser component once and every Alis.Reactive feature works with it.
---

Any component that already works in the browser can join a plan. You install,
bundle and construct it as usual. You declare it once in C#, and the framework
gives it the same typed API every shipped component has: `.Reactive`, `Set`,
`Call`, `Get`, gather, validation and partials.

This page brings [Tom Select](https://tom-select.js.org), as the sandbox does.

## 1. Construct It in Your Bundle

The app owns the widget. The framework raises `alis:mount` before it wires a piece
of HTML (the page at boot, a partial when it is injected) and `alis:unmount`
before it removes one, so you construct and destroy at the right moment.

```ts
import TomSelect from "tom-select";

document.addEventListener("alis:mount", event => {
  const { root } = (event as CustomEvent<{ root: HTMLElement }>).detail;
  for (const host of root.querySelectorAll<HTMLSelectElement & { tomselect?: TomSelect }>("select.tom-select")) {
    if (host.tomselect === undefined) new TomSelect(host, { dropdownParent: "body" });
  }
});

document.addEventListener("alis:unmount", event => {
  const { root } = (event as CustomEvent<{ root: HTMLElement }>).detail;
  for (const host of root.querySelectorAll<HTMLSelectElement & { tomselect?: TomSelect }>("select.tom-select")) {
    host.tomselect?.destroy();
  }
});
```

## 2. Declare It

One class names the object's members by the names its JavaScript API uses, and
says where the object lives. The vendor tells the framework how to find the
object on its element, how to subscribe to events, and what a listener receives.

```csharp
public sealed class TomSelect : Component, IInputComponent
{
    public static ComponentVendor TomSelectVendor { get; } = ComponentVendor.Define(
        "tomselect",
        objectPath: "tomselect",
        subscribe: "on",
        unsubscribe: "off",
        payload: VendorEventPayload.Argument);

    public override ComponentVendor Vendor => TomSelectVendor;

    // The value a form posts, gather reads and validation checks.
    public string ValueMember => "input.value";

    public ComponentProperty<string> Value => Property<string>(ValueMember);
    public ComponentMethod<string> SetValue => Method<string>("setValue");
    public ComponentMethod Clear => Method("clear");
    public TypedEvent<TomSelectChangeArgs> Change => Event<TomSelectChangeArgs>("change");
}

public class TomSelectChangeArgs
{
}
```

| JavaScript | Declared as |
| --- | --- |
| property | `Property<T>("name")` |
| method | `Method("name")`, `Method<T1, T2>("name")` |
| method that returns a value | `Function<TResult>("name")` |
| event | `Event<TArgs>("name")` |

## 3. Render It

The render helper writes the component's own markup with the ID and name the
framework gives the field, and registers it as the field's input.

```csharp
public static RenderedComponent<TomSelect, TModel> TomSelect<TModel, TProp>(
    this InputBoundField<TModel, TProp> field,
    IEnumerable<TomSelectOption> options,
    string placeholder)
    where TModel : class
{
    var html = new StringBuilder()
        .Append("<select class=\"tom-select\" id=\"").Append(Encode(field.ElementId))
        .Append("\" name=\"").Append(Encode(field.BindingPath)).Append("\">");
    // ... one <option> per suite ...
    html.Append("</select>");
    return field.Render<TomSelect>(new HtmlString(html.ToString()));
}
```

A component that is not a form input renders against the plan with an ID you
choose: `plan.Render<TComponent>("my-id", markup)`.

## 4. Use It

Every view now has the whole DSL for it, with no further code:

```csharp
@{ Html.InputField(plan, m => m.SuiteType, o => o.Required().Label("Suite type"))
    .TomSelect(suites, "Choose a suite type")
    .Reactive(evt => evt.Change, (args, p) =>
    {
        var suite = p.Component<TomSelect>(m => m.SuiteType);
        p.When(suite.Get(x => x.Value)).NotEmpty()
            .Then(quote => quote.Get(rateUrl)
                .Gather(g => g.RouteParam("suiteType", suite.Get(x => x.Value)))
                .Response(r => r.OnSuccess<SuiteRateResponse>((rate, s) =>
                    s.Component<FusionNumericTextBox>(m => m.MonthlyRate)
                        .Set(x => x.Value, rate.Read(x => x.MonthlyRate)))));
    }); }
```

```csharp
s.Component<TomSelect>(m => m.SuiteType).Call(c => c.SetValue, recommendation.Read(x => x.SuiteType));
```

The Syncfusion and native components that ship with Alis.Reactive are declared
exactly this way, against the same public surface.
