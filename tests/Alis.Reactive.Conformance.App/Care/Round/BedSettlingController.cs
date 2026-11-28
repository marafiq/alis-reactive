using System.Collections.Generic;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class BedSettlingController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.BedSettlingOfASettledBed)]
    public IActionResult OfASettledBed() => View("/Care/Round/BedSettling.cshtml", new BedSettlingDesk { Resident = "Ada Hart", Settled = true });

    [HttpGet(CarePages.BedSettlingOfAnUnsettledBed)]
    public IActionResult OfAnUnsettledBed() => View("/Care/Round/BedSettling.cshtml", new BedSettlingDesk { Resident = "Ben Okafor", Settled = false });

    [HttpPost(BedSettlings.Counted)]
    public IActionResult Counted([FromBody] BedSettlingSent sent)
    {
        if (sent.Settled.TryGetValue(true, out var settled)) return Content("Settled " + settled, PlainText);
        return Content(sent.Settled.TryGetValue(false, out var unsettled) ? "Unsettled " + unsettled : "Not counted", PlainText);
    }
}

public sealed class BedSettlingDesk
{
    public string Resident { get; init; } = "";

    public bool Settled { get; init; }
}

public sealed class BedSettlingSent
{
    public Dictionary<bool, int> Settled { get; init; } = [];
}

public static class BedSettlings
{
    private const string Root = CarePages.Area + "round/bed-settling/";

    public const string Counted = Root + "counted";

    public static readonly AppEvent Count = AppEvent.Declare();
}
