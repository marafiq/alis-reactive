using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class LiveLinesController : Controller
{
    private const string Live = "/Care/Round/_LiveLayout.cshtml";
    private const string LiveTraced = "/Care/Round/_LiveTracedLayout.cshtml";
    private const string LiveTally = "/Care/Round/_LiveTallyLayout.cshtml";

    [HttpGet(CarePages.LiveWardLine)]
    public IActionResult WardLine() => View("/Care/Round/LiveWardLine.cshtml", new LiveRound("North", Live));

    [HttpGet(CarePages.LiveRoundFeed)]
    public IActionResult RoundFeed() => View("/Care/Round/LiveRoundFeed.cshtml", new LiveRound("North", LiveTally));

    [HttpGet(CarePages.LiveStatusBoard)]
    public IActionResult StatusBoard() => View("/Care/Round/LiveStatusBoard.cshtml", new LiveRound("North", Live));

    [HttpGet(CarePages.LiveBedLine)]
    public IActionResult BedLine() => View("/Care/Round/LiveBedLine.cshtml", new LiveLine("North", Live, PageNamesTheBedLine: false));

    [HttpGet(CarePages.LiveBedLineHeldByThePage)]
    public IActionResult BedLineHeldByThePage() => View("/Care/Round/LiveBedLine.cshtml", new LiveLine("North", Live, PageNamesTheBedLine: true));

    [HttpGet(CarePages.LiveLine)]
    public IActionResult Line() => View("/Care/Round/LiveLine.cshtml", new LiveRound("North", Live));

    [HttpGet(CarePages.LiveNurseLine)]
    public IActionResult NurseLine() => View("/Care/Round/LiveNurseLine.cshtml", new LiveRound("North", Live));

    [HttpGet(CarePages.LiveTracedNurseLine)]
    public IActionResult TracedNurseLine() => View("/Care/Round/LiveCallNurse.cshtml", new LiveRound("North", LiveTraced));

    [HttpGet(CareEndpoints.AdasStatusCard)]
    public IActionResult AdasStatusCard() => PartialView("/Care/Round/LiveStatusCard.cshtml", new StatusCard("Ada", "ada"));

    [HttpGet(CareEndpoints.BensStatusCard)]
    public IActionResult BensStatusCard() => PartialView("/Care/Round/LiveStatusCard.cshtml", new StatusCard("Ben", "ben"));

    [HttpGet(CareEndpoints.BedLineCard)]
    public IActionResult BedLineCard() => PartialView("/Care/Round/LiveBedLineCard.cshtml", new LineCard("Bed 4"));

    [HttpGet(CareEndpoints.LineCard)]
    public IActionResult LineCard() => PartialView("/Care/Round/LiveLineCard.cshtml", new LineCard("Bed 7"));
}
