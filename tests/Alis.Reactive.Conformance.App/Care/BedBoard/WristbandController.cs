using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WristbandController : Controller
{
    [HttpGet(CarePages.Wristband)]
    public IActionResult OfAResident() => View("/Care/BedBoard/Wristband.cshtml", new Wristband { BandColor = "#ff0000ff" });

    [HttpGet(CarePages.WristbandBlue)]
    public IActionResult OfAResidentInBlue() => View("/Care/BedBoard/Wristband.cshtml", new Wristband { BandColor = "#0000ffff" });
}

public sealed class Wristband
{
    public string? BandColor { get; set; }
}

public static class Wristbands
{
    public static readonly AppEvent ShowBand = AppEvent.Declare();
}
