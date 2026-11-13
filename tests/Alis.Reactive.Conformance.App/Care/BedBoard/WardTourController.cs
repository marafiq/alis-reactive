using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardTourController : Controller
{
    [HttpGet(CarePages.WardTour)]
    public IActionResult OfTheWing() => View("/Care/BedBoard/WardTour.cshtml", new WardTour(2));

    [HttpGet(CarePages.WardTourFromWardB)]
    public IActionResult FromWardB() => View("/Care/BedBoard/WardTour.cshtml", new WardTour(1));
}

public sealed record WardTour(int StartingSlide);

public static class WardTours
{
    public static readonly AppEvent WhichWard = AppEvent.Declare();
}
