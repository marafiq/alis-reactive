using System.Globalization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ChartCardsController : Controller
{
    private const string PlainText = "text/plain";
    private const string NoneBound = "none";

    [HttpGet(CarePages.ChartCards)]
    public IActionResult ForAResident() => View("/Care/Record/ChartCards.cshtml", new ChartCards("Ada Hart"));

    [HttpPost(ChartCardEndpoints.Fluids)]
    public IActionResult ChartFluids([FromForm] FluidsCard card) => Content(Bound(card.Value), PlainText);

    [HttpPost(ChartCardEndpoints.Weights)]
    public IActionResult ChartWeight([FromForm] WeightCard card) => Content(Bound(card.Value), PlainText);

    private static string Bound(int? value) => value?.ToString(CultureInfo.InvariantCulture) ?? NoneBound;
}

public sealed record ChartCards(string Resident);

public sealed class FluidsCard
{
    public int? Value { get; set; }
}

public sealed class WeightCard
{
    public int? Value { get; set; }
}

public static class ChartCardEndpoints
{
    public const string Fluids = CarePages.ChartCards + "/fluids";
    public const string Weights = CarePages.ChartCards + "/weights";
}

public static class FluidsCards
{
    public static readonly AppEvent Save = AppEvent.Declare();
}

public static class WeightCards
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
