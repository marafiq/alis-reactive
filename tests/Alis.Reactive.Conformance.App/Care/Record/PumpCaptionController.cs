using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class PumpCaptionController : Controller
{
    [HttpGet(CarePages.PumpCaptionOfANamedRound)]
    public IActionResult OfANamedRound() => View("/Care/Record/PumpCaption.cshtml", new PumpCaption("Ada Hart", "Morning round"));

    [HttpGet(CarePages.PumpCaptionOfAnUnnamedRound)]
    public IActionResult OfAnUnnamedRound() => View("/Care/Record/PumpCaption.cshtml", new PumpCaption("Ada Hart", null));
}

public sealed record PumpCaption(string Resident, string? Round);

public static class PumpCaptions
{
    public static readonly AppEvent TakeText = AppEvent.Declare();
}
