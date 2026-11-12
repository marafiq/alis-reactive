using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class StaffSearchController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.StaffSearch)]
    public IActionResult Search() => View("/Care/Round/StaffSearch.cshtml", new StaffSearch("Morning"));

    [HttpGet(CarePages.StaffSearchLogged)]
    public IActionResult Logged() => View("/Care/Round/StaffSearchLogged.cshtml", new StaffSearch("Morning"));

    [HttpGet(CareEndpoints.RoundStaff)]
    public IActionResult Staff(StaffQuery query) => Content(query.Choice.ToString(), PlainText);
}

public sealed record StaffSearch(string Round);

public sealed class StaffQuery
{
    public Choice Choice { get; set; }
}

public static class StaffSearches
{
    public static readonly AppEvent Find = AppEvent.Declare();
}
