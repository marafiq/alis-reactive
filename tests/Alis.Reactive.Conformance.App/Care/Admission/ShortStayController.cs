using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ShortStayController : Controller
{
    [HttpGet(CarePages.StandardShortStay)]
    public IActionResult Standard() => View("/Care/Admission/ShortStay.cshtml", new ShortStay { AsksNights = false });

    [HttpGet(CarePages.CustomShortStay)]
    public IActionResult Custom() => View("/Care/Admission/ShortStay.cshtml", new ShortStay { AsksNights = true });
}

public sealed class ShortStay
{
    public string? Resident { get; set; }

    public int Nights { get; set; } = 14;

    public required bool AsksNights { get; init; }
}

public static class ShortStays
{
    public static readonly AppEvent Quote = AppEvent.Declare();
}
