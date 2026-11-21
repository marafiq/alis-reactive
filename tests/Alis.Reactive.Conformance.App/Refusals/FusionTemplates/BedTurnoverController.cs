using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class BedTurnoverController : Controller
{
    private const string Turnover = "/Refusals/FusionTemplates/BedTurnover.cshtml";

    [HttpGet(RefusalPages.BedTurnoverOrdered)]
    public IActionResult Ordered() => View(Turnover, new BedTurnover(true, Beds()));

    [HttpGet(RefusalPages.BedTurnoverCompared)]
    public IActionResult Compared() => View(Turnover, new BedTurnover(false, Beds()));

    private static IReadOnlyList<TurnoverBed> Beds() => [new TurnoverBed("Bed 1", TurnoverStage.Made), new TurnoverBed("Bed 2", TurnoverStage.Stripped)];
}

public sealed record BedTurnover(bool Ordered, IReadOnlyList<TurnoverBed> Beds);

public sealed record TurnoverBed(string Bed, TurnoverStage Stage);

[JsonConverter(typeof(JsonStringEnumConverter<TurnoverStage>))]
public enum TurnoverStage
{
    Stripped,
    Made,
}
