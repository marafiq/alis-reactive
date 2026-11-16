using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class WardBedsController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(RefusalPages.WardBedsOfTwoWardsInOneForm)]
    public IActionResult OfTwoWardsInOneForm() => View("/Refusals/RequestsAndResponses/WardBedsOfTwoWardsInOneForm.cshtml", new BedSearch("Morning"));

    [HttpGet(RefusalPages.WardBedsOfWardsInTheirOwnForms)]
    public IActionResult OfWardsInTheirOwnForms() => View("/Refusals/RequestsAndResponses/WardBedsOfWardsInTheirOwnForms.cshtml", new BedSearch("Morning"));

    [HttpGet(RefusalEndpoints.WardOfTheRound)]
    public IActionResult WardOfTheRound() => Json(new NamedWardOfTheRound { Ward = NamedBedWard.EastAnnex });

    [HttpGet(RefusalEndpoints.WardBeds)]
    public IActionResult Beds(BedsOfANamedWard query) => Content($"Beds of {query.Ward}", PlainText);
}

public sealed record BedSearch(string Round);

public sealed class WardOfTheRound
{
    public BedWard Ward { get; init; }
}

public sealed class NamedWardOfTheRound
{
    public NamedBedWard Ward { get; init; }
}

public sealed class BedsOfAWard
{
    public BedWard Ward { get; set; }
}

public sealed class BedsOfANamedWard
{
    public NamedBedWard Ward { get; set; }
}

public enum BedWard
{
    [JsonStringEnumMemberName("east")]
    EastWing,

    [JsonStringEnumMemberName("east")]
    EastAnnex,

    WestWing,
}

public enum NamedBedWard
{
    [JsonStringEnumMemberName("east")]
    EastWing,

    [JsonStringEnumMemberName("east-annex")]
    EastAnnex,

    WestWing,
}

public static class WardBedEvents
{
    public static readonly AppEvent Find = AppEvent.Declare();
}
