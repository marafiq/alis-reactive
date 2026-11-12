using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class WardBedsController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(RefusalPages.WardBedsOfTwoWardsInOneForm)]
    public IActionResult OfTwoWardsInOneForm() => View("/Refusals/RequestsAndResponses/WardBedsOfTwoWardsInOneForm.cshtml", new BedSearch { Ward = BedWard.EastWing, Wards = TheWards() });

    [HttpGet(RefusalPages.WardBedsOfWardsInTheirOwnForms)]
    public IActionResult OfWardsInTheirOwnForms() => View("/Refusals/RequestsAndResponses/WardBedsOfWardsInTheirOwnForms.cshtml", new NamedBedSearch { Ward = NamedBedWard.EastWing, Wards = TheWards() });

    [HttpGet(RefusalEndpoints.WardBeds)]
    public IActionResult Beds(BedsOfANamedWard query) => Content($"Beds of {query.Ward}", PlainText);

    private static List<SelectListItem> TheWards() => new List<SelectListItem>
    {
        new("East Wing", nameof(NamedBedWard.EastWing)),
        new("East Annex", nameof(NamedBedWard.EastAnnex)),
        new("West Wing", nameof(NamedBedWard.WestWing)),
    };
}

public sealed class BedSearch
{
    public BedWard Ward { get; set; }

    public IReadOnlyList<SelectListItem> Wards { get; init; } = [];
}

public sealed class NamedBedSearch
{
    public NamedBedWard Ward { get; set; }

    public IReadOnlyList<SelectListItem> Wards { get; init; } = [];
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
