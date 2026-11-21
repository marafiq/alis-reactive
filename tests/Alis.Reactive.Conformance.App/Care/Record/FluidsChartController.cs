using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class FluidsChartController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.FluidsChart)]
    public IActionResult ForAResident() => View("/Care/Record/FluidsChart.cshtml", new ChartEntry<FluidsReading>());

    [HttpGet(CarePages.FluidsChartLabelledByArity)]
    public IActionResult LabelledByArity() => View("/Care/Record/FluidsChartLabelledByArity.cshtml", new ChartEntry<FluidsReading>());

    [HttpPost(FluidsChartEndpoints.Entries)]
    public IActionResult Chart([FromForm] ChartEntry<FluidsReading> entry) => Content($"initials {entry.Initials ?? "none"}", PlainText);
}

public sealed class ChartEntry<TReading>
    where TReading : new()
{
    public TReading Reading { get; set; } = new();

    public string? Initials { get; set; }
}

public sealed class FluidsReading
{
    public int? Millilitres { get; set; }
}

public static class FluidsChartEndpoints
{
    public const string Entries = CarePages.FluidsChart + "/entries";
}

public static class FluidsCharts
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
