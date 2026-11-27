using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ArrivalCalendarBoardsController : Controller
{
    [HttpGet(CarePages.ArrivalPaperMargin)]
    public IActionResult ArrivalPaperMargin() => View("/Care/AdmissionClinical/ArrivalPaperMargins.cshtml", new AdmissionRegisterMargins { UseVerticalPaperMargin = false });

    [HttpGet(CarePages.ArrivalCrossMargin)]
    public IActionResult ArrivalCrossMargin() => View("/Care/AdmissionClinical/ArrivalPaperMargins.cshtml", new AdmissionRegisterMargins { UseVerticalPaperMargin = true });

    [HttpGet(CarePages.ArrivalTurnedCalendar)]
    public IActionResult ArrivalTurnedCalendar() => View("/Care/AdmissionClinical/ArrivalBedsideEdges.cshtml", new LateTransferDateBoard { UsePrintLayout = false });

    [HttpGet(CarePages.ArrivalPrintedCalendar)]
    public IActionResult ArrivalPrintedCalendar() => View("/Care/AdmissionClinical/ArrivalBedsideEdges.cshtml", new LateTransferDateBoard { UsePrintLayout = true });

    [HttpGet(CarePages.ArrivalFittedReview)]
    public IActionResult ArrivalFittedReview() => View("/Care/AdmissionClinical/ArrivalMovedReview.cshtml", new HandoverBookingReview { KeepPaperMargins = false });

    [HttpGet(CarePages.ArrivalRetainedMargins)]
    public IActionResult ArrivalRetainedMargins() => View("/Care/AdmissionClinical/ArrivalMovedReview.cshtml", new HandoverBookingReview { KeepPaperMargins = true });

    [HttpGet(CarePages.ArrivalFullReview)]
    public IActionResult ArrivalFullReview() => View("/Care/AdmissionClinical/ArrivalClippedReview.cshtml", new SupervisorCalendarReview { ShowPrintMargins = false });

    [HttpGet(CarePages.ArrivalPrintReview)]
    public IActionResult ArrivalPrintReview() => View("/Care/AdmissionClinical/ArrivalClippedReview.cshtml", new SupervisorCalendarReview { ShowPrintMargins = true });

    [HttpGet(CarePages.ArrivalPausedTracking)]
    public IActionResult ArrivalPausedTracking() => View("/Care/AdmissionClinical/ArrivalReviewRegister.cshtml", new TransferReviewRegister { ResumeTracking = false });

    [HttpGet(CarePages.ArrivalResumedTracking)]
    public IActionResult ArrivalResumedTracking() => View("/Care/AdmissionClinical/ArrivalReviewRegister.cshtml", new TransferReviewRegister { ResumeTracking = true });

    [HttpGet(CarePages.ArrivalRegisteredReview)]
    public IActionResult ArrivalRegisteredReview() => View("/Care/AdmissionClinical/ArrivalAncestryReview.cshtml", new TransferHandoverReview { UseBookingSheet = false });

    [HttpGet(CarePages.ArrivalSheetReview)]
    public IActionResult ArrivalSheetReview() => View("/Care/AdmissionClinical/ArrivalAncestryReview.cshtml", new TransferHandoverReview { UseBookingSheet = true });
}

public sealed class AdmissionRegisterMargins
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);

    public bool UseVerticalPaperMargin { get; init; }
}

public sealed class LateTransferDateBoard
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);

    public bool UsePrintLayout { get; init; }
}

public sealed class HandoverBookingReview
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);

    public bool KeepPaperMargins { get; init; }
}

public sealed class SupervisorCalendarReview
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);

    public bool ShowPrintMargins { get; init; }
}

public sealed class TransferReviewRegister
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);

    public bool ResumeTracking { get; init; }
}

public sealed class TransferHandoverReview
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);

    public bool UseBookingSheet { get; init; }
}
