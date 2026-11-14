using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class FallWatchController : Controller
{
    [HttpGet(CarePages.FallWatch)]
    public IActionResult OfTheWard() => View("/Care/Record/FallWatch.cshtml", new FallWatch("East ward"));

    [HttpGet(FallWatchEndpoints.AdasCard)]
    public IActionResult AdasCard() => PartialView("/Care/Record/FallWatchCard.cshtml", new FallWatchCard("Ada Byron", "Room 4"));

    [HttpGet(FallWatchEndpoints.BensCard)]
    public IActionResult BensCard() => PartialView("/Care/Record/FallWatchCard.cshtml", new FallWatchCard("Ben Okafor", "Room 7"));
}

public sealed record FallWatch(string Ward);

public sealed record FallWatchCard(string Name, string Room);

public static class FallWatchEndpoints
{
    private const string Root = CarePages.Area + "record/fall-watch/";

    public const string AdasCard = Root + "ada";

    public const string BensCard = Root + "ben";
}

public static class FallWatches
{
    public static readonly AppEvent OpenAdasCard = AppEvent.Declare();

    public static readonly AppEvent OpenBensCard = AppEvent.Declare();

    public static readonly AppEvent CloseTheCard = AppEvent.Declare();

    public static readonly AppEvent FallNoted = AppEvent.Declare();
}
