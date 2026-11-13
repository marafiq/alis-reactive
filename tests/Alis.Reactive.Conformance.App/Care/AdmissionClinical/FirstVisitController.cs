using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class FirstVisitController : Controller
{
    [HttpGet(CarePages.FirstVisitBooked)]
    public IActionResult Booked() => View("/Care/AdmissionClinical/FirstVisit.cshtml", new FirstVisitBooking { FirstVisit = new DateTime(2026, 7, 14, 9, 30, 0) });

    [HttpGet(CarePages.FirstVisitOpen)]
    public IActionResult Open() => View("/Care/AdmissionClinical/FirstVisit.cshtml", new FirstVisitBooking());
}

public sealed class FirstVisitBooking
{
    public DateTime? FirstVisit { get; set; }
}
