using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class TransportProvidersController : Controller
{
    [HttpGet(CarePages.TransportProviders)]
    public IActionResult BookTransport()
        => View("/Care/AdmissionClinical/TransportProviders.cshtml", new TransportProviders());

    [HttpPost(TransportBookings.Save)]
    public IActionResult SaveTransport([FromForm] TransportProviders booking)
    {
        if (booking.ProviderCode is null) return Content("Family will arrange transport.", "text/plain");

        var provider = booking.WeekdayProviders.Concat(booking.WeekendServices)
            .FirstOrDefault(item => item.Code == booking.ProviderCode);

        if (provider is null) return BadRequest("Choose an available transport provider.");

        return Content($"Transport booked with {provider.ProviderName}.", "text/plain");
    }
}

public sealed class TransportProviders
{
    public string? ProviderCode { get; set; }

    public IReadOnlyList<TransportProvider> WeekdayProviders { get; } =
    [
        new("CC", "Cedar Community Cars", "Cedar medical car", "Day shift"),
        new("NT", "Northside Transit", "Northside clinic bus", "Day shift")
    ];

    public IReadOnlyList<TransportProvider> WeekendServices { get; } =
    [
        new("SM", "Sunrise Mobility", "Sunrise wheelchair van", "Day shift"),
        new("HR", "Harbour Rides", "Harbour evening shuttle", "Evening shift")
    ];
}

public sealed record TransportProvider(
    string Code,
    string ProviderName,
    [property: JsonPropertyName("serviceLabel")] string ServiceDescription,
    [property: JsonPropertyName("dutyPeriod")] string Shift);

public static class TransportBookings
{
    public const string Save = CarePages.TransportProviders + "/booking";
    public static readonly AppEvent BrowseWeekdays = AppEvent.Declare();
    public static readonly AppEvent BrowseWeekends = AppEvent.Declare();
    public static readonly AppEvent UseFamilyTransport = AppEvent.Declare();
    public static readonly AppEvent SaveArrangement = AppEvent.Declare();
}
