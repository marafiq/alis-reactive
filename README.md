# Alis.Reactive

Plan-driven reactive framework for ASP.NET MVC. Write C# fluent builders that serialize to JSON plans; a tiny browser runtime executes them. Zero hand-written JavaScript in your views.

## Documentation

Full docs, architecture guides, and examples: <https://marafiq.github.io/alis-reactive/>

## Install

```shell
dotnet add package Alis.Reactive.Native --version 1.0.0-preview.1
```

Additional packages:

| Package | Purpose |
|---------|---------|
| `Alis.Reactive` | Core framework (descriptors, builders, plan, schema) |
| `Alis.Reactive.Native` | Native HTML components + JS runtime + CSS |
| `Alis.Reactive.Fusion` | Syncfusion EJ2 component integration |
| `Alis.Reactive.FluentValidator` | FluentValidation client-side rule extraction |
| `Alis.Reactive.NativeTagHelpers` | Design system tag helpers |

## Quick Start

```csharp
@{
    IReactivePlan<MyModel> plan = new ReactivePlan<MyModel>();

    Html.On(plan, t => t.DomReady(p =>
    {
        p.Element("status").AddClass("active");
        p.Element("status").SetText("loaded");
    }));
}

<div id="status">waiting...</div>
<script type="application/json" data-alis-plan data-trace="trace">
    @Html.Raw(plan.Render())
</script>
```

## Example

See the [Resident Intake example](https://marafiq.github.io/alis-reactive/examples/resident-intake/) for a full walkthrough.

## License

MIT
