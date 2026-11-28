using System.Globalization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class OutingController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(RefusalPages.OutingWholeLocation)]
    public IActionResult WholeLocation() => View("/Refusals/InputsAndIds/OutingWholeLocation.cshtml", new Outing());

    [HttpGet(RefusalPages.OutingLocationsLatitude)]
    public IActionResult LocationsLatitude() => View("/Refusals/InputsAndIds/OutingLocationsLatitude.cshtml", new Outing());

    [HttpPost(RefusalEndpoints.Outings)]
    public IActionResult Save([FromForm] Outing outing)
        => Content(outing.Location.Lat is { } lat ? $"Latitude: {lat.ToString(CultureInfo.InvariantCulture)}" : "No latitude", PlainText);
}

public sealed class Outing
{
    public Place Location { get; set; } = new();
}

public sealed class Place
{
    public decimal? Lat { get; set; }

    public decimal? Lng { get; set; }
}

public static class Outings
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
