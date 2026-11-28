using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class PanelReopeningController : Controller
{
    [HttpGet(CarePages.PanelReopening)]
    public IActionResult OfAResident() => View("/Care/Record/PanelReopening.cshtml", new PanelReopening("Ada Hart"));

    [HttpGet(PanelReopeningEndpoints.Panel)]
    public IActionResult Panel() => PartialView("/Care/Record/ReopenedPanel.cshtml", new ReopenedPanel("Bed alarm panel", "Bed alarm"));
}

public sealed record PanelReopening(string Resident);

public sealed record ReopenedPanel(string Heading, string Alarm);

public static class PanelReopeningEndpoints
{
    public const string Panel = CarePages.Area + "record/panel-reopening/panel";
}

public static class PanelReopeningEvents
{
    public static readonly AppEvent OpenThePanel = AppEvent.Declare();

    public static readonly AppEvent CloseThePanel = AppEvent.Declare();
}
