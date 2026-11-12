using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class PassDeskController : Controller
{
    [HttpGet(RefusalPages.PassDeskThatSetsItsSlotsText)]
    public IActionResult SetsItsSlotsText() => View("/Refusals/PartialsAndSlots/PassDeskThatSetsItsSlotsText.cshtml", new PassDesk("Front desk"));

    [HttpGet(RefusalPages.PassDeskThatWritesBesideItsSlot)]
    public IActionResult WritesBesideItsSlot() => View("/Refusals/PartialsAndSlots/PassDeskThatWritesBesideItsSlot.cshtml", new PassDesk("Front desk"));

    [HttpGet(PassDesks.VisitorCard)]
    public IActionResult Card() => PartialView("/Refusals/PartialsAndSlots/VisitorCard.cshtml", new VisitorCard("Ada Hart"));
}

public sealed record PassDesk(string Name);

public sealed record VisitorCard(string Visitor);

public static class PassDesks
{
    public const string VisitorCard = RefusalPages.Area + "partials-and-slots/pass-desk/visitor-card";

    public static readonly AppEvent ShowTheCard = AppEvent.Declare();

    public static readonly AppEvent SignOut = AppEvent.Declare();

    public static readonly AppEvent PrintThePass = AppEvent.Declare();
}
