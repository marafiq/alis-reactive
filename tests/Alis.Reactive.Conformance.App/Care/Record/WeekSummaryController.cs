using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WeekSummaryController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.WeekSummary)]
    public IActionResult OfAda() => View("/Care/Record/WeekSummary.cshtml", new WeekSummary("Ada Hart"));

    [HttpGet(WeekSummaryEndpoints.Tab)]
    public IActionResult Tab() => PartialView("/Care/Record/WeekSummaryTab.cshtml", new WeekSummaryTab("This week"));

    [HttpGet(CareEndpoints.WeekSummaryNotes)]
    public IActionResult Notes() => Content("4 notes this week", PlainText);
}

public sealed record WeekSummary(string Resident);

public sealed record WeekSummaryTab(string Title);

public static class WeekSummaryEndpoints
{
    public const string Tab = CarePages.WeekSummary + "/tab";
}

public static class WeekSummaries
{
    public static readonly AppEvent OpenTheSummary = AppEvent.Declare();

    public static readonly AppEvent LoadTheSummary = AppEvent.Declare();
}
