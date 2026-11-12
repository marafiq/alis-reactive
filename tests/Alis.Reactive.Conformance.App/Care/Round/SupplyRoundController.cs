using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class SupplyRoundController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.SupplyRound)]
    public IActionResult OfARound() => View("/Care/Round/SupplyRound.cshtml", new SupplyRound("Morning"));

    [HttpGet(SupplyRoundEndpoints.Supplies)]
    public IActionResult Supplies() => Json(new LoadedSupplies
    {
        Records =
        [
            new Record { Name = "Zinc", Count = 7 },
            new Record { Name = "Aspirin", Count = 2 },
        ],
    });

    [HttpPost(SupplyRoundEndpoints.Sent)]
    public IActionResult Sent([FromBody] SentSupplies supplies)
        => Content($"{string.Join(", ", supplies.Records.Select(record => record.Name))}; first count {supplies.Records.Select(record => record.Count).FirstOrDefault()}", PlainText);
}

public sealed record SupplyRound(string Name);

public sealed class LoadedSupplies
{
    public List<Record> Records { get; init; } = [];
}

public sealed class SentSupplies
{
    public List<Record> Records { get; set; } = [];
}

public static class SupplyRoundEndpoints
{
    private const string Root = CarePages.Area + "round/supplies/";
    public const string Supplies = Root + "loaded";
    public const string Sent = Root + "sent";
}

public static class SupplyRounds
{
    public static readonly AppEvent Load = AppEvent.Declare();
}
