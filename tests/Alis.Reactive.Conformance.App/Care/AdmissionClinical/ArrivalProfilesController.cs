using System.Globalization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ArrivalProfilesController : Controller
{
    [HttpGet(CarePages.ArrivalLetterIntake)]
    public IActionResult ArrivalLetterIntake() => View("/Care/AdmissionClinical/ArrivalLetterIntake.cshtml", new ArrivalLetterIntake());

    [HttpGet(CarePages.ArrivalStationStamp)]
    public IActionResult ArrivalStationStamp() => View("/Care/AdmissionClinical/ArrivalStationStamp.cshtml", new ArrivalStationStamp());

    [HttpGet(CarePages.ArrivalRecoveryDesk)]
    public IActionResult ArrivalRecoveryDesk() => View("/Care/AdmissionClinical/ArrivalRecoveryDesk.cshtml", new ArrivalRecoveryDesk());

    [HttpGet(CarePages.ArrivalReviewHistory)]
    public IActionResult ArrivalReviewHistory() => View("/Care/AdmissionClinical/ArrivalReviewHistory.cshtml", new ArrivalReviewHistory());

    [HttpGet(CarePages.ArrivalPeriodNote)]
    public IActionResult ArrivalPeriodNote() => View("/Care/AdmissionClinical/ArrivalPeriodNote.cshtml", new ArrivalPeriodNote());

    [HttpGet(CarePages.ArrivalDispatchLanguage)]
    public IActionResult ArrivalDispatchLanguage() => View("/Care/AdmissionClinical/ArrivalDispatchLanguage.cshtml", new ArrivalDispatchLanguage());

    [HttpGet(CarePages.ArrivalChaplaincy)]
    public IActionResult ArrivalChaplaincy() => View("/Care/AdmissionClinical/ArrivalChaplaincy.cshtml", new ArrivalChaplaincy());

    [HttpGet(CarePages.ArrivalSeason)]
    public IActionResult ArrivalSeason() => View("/Care/AdmissionClinical/ArrivalSeason.cshtml", new ArrivalSeason());

    [HttpGet(CarePages.ArrivalPaperRow)]
    public IActionResult ArrivalPaperRow() => View("/Care/AdmissionClinical/ArrivalPaperRow.cshtml", new ArrivalPaperRow());

    [HttpGet(CarePages.ArrivalNursingMask)]
    public IActionResult ArrivalNursingMask() => View("/Care/AdmissionClinical/ArrivalTemplateMask.cshtml", new ArrivalTemplateMask { Nursing = true });

    [HttpGet(CarePages.ArrivalTransportMask)]
    public IActionResult ArrivalTransportMask() => View("/Care/AdmissionClinical/ArrivalTemplateMask.cshtml", new ArrivalTemplateMask { Nursing = false });

    [HttpGet(CarePages.ArrivalRecoverNext)]
    public IActionResult ArrivalRecoverNext() => View("/Care/AdmissionClinical/ArrivalRecoveryDesk.cshtml", new ArrivalRecoveryDesk { Remembered = true });

    [HttpGet(CarePages.ArrivalTransferEdition)]
    public IActionResult ArrivalTransferEdition() => View("/Care/AdmissionClinical/ArrivalIntakeEdition.cshtml", new ArrivalIntakeEdition { Transfer = true });

    [HttpGet(CarePages.ArrivalOrdinaryEdition)]
    public IActionResult ArrivalOrdinaryEdition() => View("/Care/AdmissionClinical/ArrivalIntakeEdition.cshtml", new ArrivalIntakeEdition { Transfer = false });

    [HttpPost(CarePages.FileArrivalEdition)]
    public IActionResult FileEdition([FromForm] ArrivalIntakeEdition request)
    {
        if (request.ArrivesOn is not DateTime arrival) return Content("Arrival date missing", "text/plain");
        var edition = request.Transfer ? "Transfer filed for " : "Admission filed for ";
        return Content(edition + arrival.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture), "text/plain");
    }
}

public sealed class ArrivalLetterIntake
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);
}

public sealed class ArrivalStationStamp
{
    public DateTime? ArrivesOn { get; set; }
}

public sealed class ArrivalTemplateMask
{
    public DateTime? ArrivesOn { get; set; }

    public bool Nursing { get; init; }
}

public sealed class ArrivalRecoveryDesk
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);

    public bool Remembered { get; init; }
}

public sealed class ArrivalReviewHistory
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);
}

public sealed class ArrivalPeriodNote
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);
}

public sealed class ArrivalDispatchLanguage
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);
}

public sealed class ArrivalChaplaincy
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);
}

public sealed class ArrivalSeason
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);
}

public sealed class ArrivalIntakeEdition
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);

    public bool Transfer { get; set; }
}

public sealed class ArrivalPaperRow
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);
}
