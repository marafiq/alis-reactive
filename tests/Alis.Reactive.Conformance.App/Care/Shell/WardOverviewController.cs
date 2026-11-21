using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardOverviewController : Controller
{
    [HttpGet(CarePages.WardOverview)]
    public IActionResult OfTheWard() => View("/Care/Shell/WardOverview.cshtml", new WardOverview("North"));
}

public sealed record WardOverview(string Ward);

public static class WardOverviewEvents
{
    public static readonly AppEvent ShowTheWard = AppEvent.Declare();
}
