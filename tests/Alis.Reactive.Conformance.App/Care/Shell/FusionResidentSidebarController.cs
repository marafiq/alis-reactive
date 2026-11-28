using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class FusionResidentSidebarController : Controller
{
    [HttpGet(CarePages.FusionResidentSidebar)]
    public IActionResult Overview() => View("/Care/Shell/FusionResidentSidebar.cshtml", new FusionResidentSidebar(true));

    [HttpGet(CarePages.FusionResidentSidebarCollapsed)]
    public IActionResult OverviewCollapsed() => View("/Care/Shell/FusionResidentSidebar.cshtml", new FusionResidentSidebar(false));
}

public sealed record FusionResidentSidebar(bool ShowResidents);

public static class FusionResidentSidebarButtons
{
    public static readonly AppEvent Show = AppEvent.Declare();

    public static readonly AppEvent Hide = AppEvent.Declare();
}
