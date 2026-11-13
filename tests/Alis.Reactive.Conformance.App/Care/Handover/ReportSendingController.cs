using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ReportSendingController : Controller
{
    private const string Sending = "/Care/Handover/ReportSending.cshtml";

    [HttpGet(CarePages.ReportSendingReport)]
    public IActionResult Report() => View(Sending, new ReportSending { Content = "Send the report" });

    [HttpGet(CarePages.ReportSendingNightReport)]
    public IActionResult NightReport() => View(Sending, new ReportSending { Content = "Send the night report" });
}

public sealed class ReportSending
{
    public required string Content { get; init; }
}

public static class ReportSendingReads
{
    public static readonly AppEvent ShowLabel = AppEvent.Declare();
}
