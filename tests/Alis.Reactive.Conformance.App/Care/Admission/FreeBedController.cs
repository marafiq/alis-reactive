using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class FreeBedController : Controller
{
    private const string EastFour = "east-4";

    [HttpGet(CarePages.FreeBedOnAFullWard)]
    public IActionResult OnAFullWard() => View("/Care/Admission/FreeBed.cshtml", new FreeBedDesk("North ward", FreeBedEndpoints.NorthWardsFreeBeds));

    [HttpGet(CarePages.FreeBedOnAWardWithABedFree)]
    public IActionResult OnAWardWithABedFree() => View("/Care/Admission/FreeBed.cshtml", new FreeBedDesk("East ward", FreeBedEndpoints.EastWardsFreeBeds));

    [HttpGet(CarePages.BedsOfTheEastWard)]
    public IActionResult BedsOfTheEastWard() => View("/Care/Admission/WardBeds.cshtml", new WardBeds("East ward", [EastFour]));

    [HttpGet(FreeBedEndpoints.NorthWardsFreeBeds)]
    public IActionResult NorthWardsFreeBed() => Json(new FreeBedAnswer(null));

    [HttpGet(FreeBedEndpoints.EastWardsFreeBeds)]
    public IActionResult EastWardsFreeBed() => Json(new FreeBedAnswer(new FoundBed(EastFour)));

    [HttpGet(FreeBedEndpoints.Bed)]
    public IActionResult Bed(string bedId) => bedId == EastFour ? Json(new OpenedBed("East 4")) : NotFound();
}

public sealed record FreeBedDesk(string Ward, string FreeBeds);

public sealed record WardBeds(string Ward, IReadOnlyList<string> Ids);

public sealed record FreeBedAnswer(FoundBed? Bed);

public sealed record FoundBed(string Id);

public sealed record OpenedBed(string Name);

public sealed class BedOpening
{
    public string BedId { get; set; } = "";
}

public static class FreeBedEndpoints
{
    private const string Root = CarePages.Area + "admission/free-bed/";

    private const string Beds = Root + "beds/";

    public const string NorthWardsFreeBeds = Root + "north/beds/free";

    public const string EastWardsFreeBeds = Root + "east/beds/free";

    public const string Bed = Beds + "{bedId}";

    public static string BedAt(string id) => Beds + id;
}

public static class FreeBedEvents
{
    public static readonly AppEvent Find = AppEvent.Declare();
}
