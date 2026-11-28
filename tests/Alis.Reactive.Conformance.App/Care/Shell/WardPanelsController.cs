using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardPanelsController : Controller
{
    [HttpGet(CarePages.WardPanels)]
    public IActionResult Panels() => View("/Care/Shell/WardPanels.cshtml", new WardPanels("North: not loaded", "South: not loaded"));

    [HttpGet(CareEndpoints.NorthWardPanel)]
    [AtTheAnswerGate]
    public IActionResult North() => Json(new WardPanelText("North: 14 residents"));

    [HttpGet(CareEndpoints.SouthWardPanel)]
    [AtTheAnswerGate]
    public IActionResult South() => Json(new WardPanelText("South: 9 residents"));
}

public sealed record WardPanels(string North, string South);

public sealed record WardPanelText(string Text);

public static class WardPanelButtons
{
    public static readonly AppEvent RefreshNorth = AppEvent.Declare();
    public static readonly AppEvent RefreshSouth = AppEvent.Declare();
}
