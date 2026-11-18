using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class BedChecksController : Controller
{
    private const string Checks = "/Refusals/FusionTemplates/BedChecks.cshtml";

    [HttpGet(RefusalPages.BedChecksByStage)]
    public IActionResult ByStage() => View(Checks, new BedChecks(true, Beds()));

    [HttpGet(RefusalPages.BedChecksByCount)]
    public IActionResult ByCount() => View(Checks, new BedChecks(false, Beds()));

    private static IReadOnlyList<CheckedBed> Beds() => [new CheckedBed("Bed 1", CheckStage.Made, CheckStage.Stripped, 10, 9), new CheckedBed("Bed 2", CheckStage.Stripped, CheckStage.Made, 2, 2)];
}

public sealed record BedChecks(bool ByStage, IReadOnlyList<CheckedBed> Beds);

public sealed record CheckedBed(string Bed, CheckStage Stage, CheckStage Wanted, int Done, int Due);

[JsonConverter(typeof(JsonStringEnumConverter<CheckStage>))]
public enum CheckStage
{
    Stripped,
    Made,
}
