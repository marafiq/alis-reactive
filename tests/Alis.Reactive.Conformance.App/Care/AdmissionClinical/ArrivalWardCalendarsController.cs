using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ArrivalWardCalendarsController : Controller
{
    [HttpGet(CarePages.ArrivalChaplaincyPaging)]
    public IActionResult ArrivalChaplaincyPaging() => View("/Care/AdmissionClinical/ArrivalChaplaincyPaging.cshtml", new ChaplaincyCalendarReview());

    [HttpGet(CarePages.ArrivalReportingWeek)]
    public IActionResult ArrivalReportingWeek() => View("/Care/AdmissionClinical/ArrivalReportingWeek.cshtml", new ArrivalReportingWeek());

    [HttpGet(CarePages.ArrivalLeftStation)]
    public IActionResult ArrivalLeftStation() => View("/Care/AdmissionClinical/ArrivalReadingStation.cshtml", new ArrivalReadingStation { Alternate = false });

    [HttpGet(CarePages.ArrivalRightStation)]
    public IActionResult ArrivalRightStation() => View("/Care/AdmissionClinical/ArrivalReadingStation.cshtml", new ArrivalReadingStation { Alternate = true });

    [HttpGet(CarePages.ArrivalQuickRegister)]
    public IActionResult ArrivalQuickRegister() => View("/Care/AdmissionClinical/ArrivalScrollingRegister.cshtml", new ArrivalScrollingRegister { Alternate = false });

    [HttpGet(CarePages.ArrivalRetainedRegister)]
    public IActionResult ArrivalRetainedRegister() => View("/Care/AdmissionClinical/ArrivalScrollingRegister.cshtml", new ArrivalScrollingRegister { Alternate = true });

    [HttpGet(CarePages.ArrivalAssistedEntry)]
    public IActionResult ArrivalAssistedEntry() => View("/Care/AdmissionClinical/ArrivalAssistedProfile.cshtml", new ArrivalAssistedProfile { Alternate = false });

    [HttpGet(CarePages.ArrivalSignedEntry)]
    public IActionResult ArrivalSignedEntry() => View("/Care/AdmissionClinical/ArrivalAssistedProfile.cshtml", new ArrivalAssistedProfile { Alternate = true });

    [HttpGet(CarePages.ArrivalIncomingTransfer)]
    public IActionResult ArrivalIncomingTransfer() => View("/Care/AdmissionClinical/ArrivalIncomingProfile.cshtml", new ArrivalIncomingProfile { Alternate = true });

    [HttpGet(CarePages.ArrivalIncomingOrdinary)]
    public IActionResult ArrivalIncomingOrdinary() => View("/Care/AdmissionClinical/ArrivalIncomingProfile.cshtml", new ArrivalIncomingProfile { Alternate = false });

    [HttpGet(CarePages.ArrivalSavedWest)]
    public IActionResult ArrivalSavedWest() => View("/Care/AdmissionClinical/ArrivalSavedZone.cshtml", new ArrivalSavedZone { Alternate = true });

    [HttpGet(CarePages.ArrivalSavedEast)]
    public IActionResult ArrivalSavedEast() => View("/Care/AdmissionClinical/ArrivalSavedZone.cshtml", new ArrivalSavedZone { Alternate = false });

    [HttpGet(CarePages.ArrivalFamilyLimited)]
    public IActionResult ArrivalFamilyLimited() => View("/Care/AdmissionClinical/ArrivalFamilyWindow.cshtml", new ArrivalFamilyWindow { Alternate = false });

    [HttpGet(CarePages.ArrivalFamilyReleased)]
    public IActionResult ArrivalFamilyReleased() => View("/Care/AdmissionClinical/ArrivalFamilyWindow.cshtml", new ArrivalFamilyWindow { Alternate = true });

    [HttpGet(CarePages.ArrivalPrivacyHeld)]
    public IActionResult ArrivalPrivacyHeld() => View("/Care/AdmissionClinical/ArrivalPrivacyReview.cshtml", new ArrivalPrivacyReview { Alternate = true });

    [HttpGet(CarePages.ArrivalPrivacyReleased)]
    public IActionResult ArrivalPrivacyReleased() => View("/Care/AdmissionClinical/ArrivalPrivacyReview.cshtml", new ArrivalPrivacyReview { Alternate = false });
}

public sealed class ChaplaincyCalendarReview
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);
}

public sealed class ArrivalReportingWeek
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2021, 1, 8);
}

public sealed class ArrivalReadingStation
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);

    public bool Alternate { get; init; }
}

public sealed class ArrivalScrollingRegister
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);

    public bool Alternate { get; init; }

    public string[] Records { get; } = ["East wing intake", "North wing transfer", "Cedar court arrival", "Willow wing intake", "Garden room transfer", "Brookside arrival", "West wing intake", "South wing transfer", "Family meeting"];
}

public sealed class ArrivalAssistedProfile
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);

    public bool Alternate { get; init; }
}

public sealed class ArrivalIncomingProfile
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);

    public bool Alternate { get; init; }
}

public sealed class ArrivalSavedZone
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14, 2, 0, 0);

    public bool Alternate { get; init; }
}

public sealed class ArrivalFamilyWindow
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);

    public bool Alternate { get; init; }
}

public sealed class ArrivalPrivacyReview
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);

    public bool Alternate { get; init; }
}
