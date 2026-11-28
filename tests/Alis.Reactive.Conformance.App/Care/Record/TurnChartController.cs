using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class TurnChartController : Controller
{
    [HttpGet(CarePages.TurnChart)]
    public IActionResult OfAResident() => View("/Care/Record/TurnChart.cshtml", new TurnChart("Ada Hart"));

    [HttpGet(TurnChartEndpoints.Tools)]
    public IActionResult Tools() => PartialView("/Care/Record/TurnChartTools.cshtml", new TurnChartTools("Night shift"));

    [HttpGet(TurnChartEndpoints.TurnPlan)]
    public IActionResult TurnPlan() => PartialView("/Care/Record/TurnChartPlan.cshtml", new TurnChartPlan("Turn every two hours."));
}

public sealed record TurnChart(string Resident);

public sealed record TurnChartTools(string Shift);

public sealed record TurnChartPlan(string Instruction);

public static class TurnChartEndpoints
{
    private const string Root = CarePages.Area + "record/turn-chart/";

    public const string Tools = Root + "tools";

    public const string TurnPlan = Root + "turn-plan";
}

public static class TurnCharts
{
    public static readonly AppEvent OpenTheDesk = AppEvent.Declare();

    public static readonly AppEvent CloseTheDesk = AppEvent.Declare();

    public static readonly AppEvent LogATurn = AppEvent.Declare();

    public static readonly AppEvent StartAgain = AppEvent.Declare();

    public static readonly AppEvent LoadTheTurnPlan = AppEvent.Declare();
}
