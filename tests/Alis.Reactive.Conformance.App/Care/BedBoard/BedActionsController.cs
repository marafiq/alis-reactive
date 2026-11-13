using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class BedActionsController : Controller
{
    [HttpGet(CarePages.BedActions)]
    public IActionResult OfTheWing() => View("/Care/BedBoard/BedActions.cshtml", new BedActions("Bed 4, North wing"));
}

public sealed record BedActions(string Bed);
