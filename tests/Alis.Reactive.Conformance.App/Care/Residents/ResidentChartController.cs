using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ResidentChartController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.ResidentCharts)]
    public IActionResult Charts() => View("/Care/Residents/ResidentCharts.cshtml");

    [HttpGet(CarePages.ResidentChartOfNoResident)]
    public IActionResult OfNoResident() => View("/Care/Residents/ResidentChart.cshtml", new ResidentChartPanel(null));

    [HttpGet(CarePages.ResidentChartOfAda)]
    public IActionResult OfAda() => View("/Care/Residents/ResidentChart.cshtml", new ResidentChartPanel("Ada"));

    [HttpGet(CareEndpoints.ResidentChart)]
    public IActionResult Chart(string residentName) => Content($"Chart of {residentName}", PlainText);
}

public sealed record ResidentChartPanel(string? Resident);

public sealed class ResidentChartRequest
{
    public string? ResidentName { get; set; }
}

public static class ResidentCharts
{
    public static readonly AppEvent Show = AppEvent.Declare();
}
