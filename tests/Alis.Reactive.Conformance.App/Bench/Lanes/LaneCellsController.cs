using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class LaneCellsController : Controller
{
    [HttpGet(BenchPages.LaneSlotLoad)]
    public IActionResult LaneSlotLoad() => View("/Bench/Lanes/LaneSlotLoad.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.LaneSlotUnloadListener)]
    public IActionResult LaneSlotUnloadListener() => View("/Bench/Lanes/LaneSlotUnloadListener.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.LaneSlotHostDestroyed)]
    public IActionResult LaneSlotHostDestroyed() => View("/Bench/Lanes/LaneSlotHostDestroyed.cshtml", TheWorldsBoard());

    [HttpGet(LaneCards.ComponentCard)]
    public IActionResult ComponentCard() => PartialView("/Bench/Lanes/LaneComponentCard.cshtml", new LaneComponentCard("The card listens to its own component.", "a-1", ["a-1", "a-2"]));

    [HttpGet(BenchPages.LaneConfirm)]
    public IActionResult LaneConfirm() => View("/Bench/Lanes/LaneConfirm.cshtml", TheWorldsBoard());

    [HttpGet(BenchEndpoints.ListeningCard)]
    public IActionResult ListeningCard() => PartialView("/Bench/Lanes/LaneListeningCard.cshtml", new LaneCard("The card listens to the page's component."));

    private LaneBoard TheWorldsBoard() => new(HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.ComponentText, "a-1", ["a-1", "a-2"]);
}

public sealed record LaneBoard(string Text, string Value, IReadOnlyList<string> Codes);

public sealed record LaneCard(string Note);

public sealed record LaneComponentCard(string Note, string Value, IReadOnlyList<string> Codes);

public static class LaneCards
{
    public const string ComponentCard = BenchPages.Area + "lanes/cards/component";
}

public static class LaneCells
{
    public static readonly AppEvent CloseCard = AppEvent.Declare();

    public static readonly AppEvent OpenCard = AppEvent.Declare();

    public static readonly AppEvent Move = AppEvent.Declare();
}
