using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class BedCleaningController : Controller
{
    [HttpGet(CarePages.BedCleaning)]
    public IActionResult OfTheWard() => View("/Care/BedBoard/BedCleaning.cshtml", new BedCleanings([new CleanedBed("Bed 1", Cleaning.Done), new CleanedBed("Bed 2", Cleaning.Due)]));

    [HttpGet(CarePages.BedCleaningSwapped)]
    public IActionResult OfTheWardSwapped() => View("/Care/BedBoard/BedCleaning.cshtml", new BedCleanings([new CleanedBed("Bed 1", Cleaning.Due), new CleanedBed("Bed 2", Cleaning.Done)]));
}

public sealed record BedCleanings(IReadOnlyList<CleanedBed> Beds);

public sealed record CleanedBed(string Bed, Cleaning Cleaning);

[JsonConverter(typeof(JsonStringEnumConverter<Cleaning>))]
public enum Cleaning
{
    Due,
    Done,
}
