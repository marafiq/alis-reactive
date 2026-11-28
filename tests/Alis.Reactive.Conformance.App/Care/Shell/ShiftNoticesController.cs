using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ShiftNoticesController : Controller
{
    [HttpGet(CarePages.DoseToasts)]
    public IActionResult DoseToasts() => View("/Care/Shell/DoseToasts.cshtml", new DoseToasts());

    [HttpGet(CarePages.WetBedNote)]
    public IActionResult WetBedNote() => View("/Care/Shell/WetBedNote.cshtml", new WetBedNote());

    [HttpGet(CarePages.RoundNotices)]
    public IActionResult RoundNotices() => View("/Care/Shell/RoundNotices.cshtml", new RoundNotices());
}

public sealed record DoseToasts;

public sealed class WetBedNote
{
    public string Note { get; set; } = "";
}

public sealed record RoundNotices;

public static class ShiftNoticeButtons
{
    public static readonly AppEvent RecordADose = AppEvent.Declare();
    public static readonly AppEvent ReportAFall = AppEvent.Declare();
    public static readonly AppEvent Report = AppEvent.Declare();
}
