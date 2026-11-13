using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class PanelHandoverController : Controller
{
    [HttpGet(CarePages.PanelHandover)]
    public IActionResult OfAResident() => View("/Care/Record/PanelHandover.cshtml", new PanelHandover("Ada Hart"));

    [HttpGet(PanelHandoverEndpoints.Card)]
    public IActionResult Card() => PartialView("/Care/Record/PanelHandoverCard.cshtml", new PanelHandoverCard("The card counts the changes of the peer the panel hands."));
}

public sealed record PanelHandover(string Resident);

public sealed record PanelHandoverCard(string Note);

public static class PanelHandoverEndpoints
{
    public const string Card = CarePages.Area + "record/panel-handover/card";
}

public static class PanelHandoverEvents
{
    public static readonly AppEvent OpenTheCard = AppEvent.Declare();

    public static readonly AppEvent CloseTheCard = AppEvent.Declare();
}
