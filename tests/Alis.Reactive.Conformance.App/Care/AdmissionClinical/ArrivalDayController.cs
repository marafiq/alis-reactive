using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ArrivalDayController : Controller
{
    [HttpGet(CarePages.ArrivalDayKnown)]
    public IActionResult Known() => View("/Care/AdmissionClinical/ArrivalDay.cshtml", new ArrivalDay { ArrivesOn = new DateTime(2026, 7, 14) });

    [HttpGet(CarePages.ArrivalDayOpen)]
    public IActionResult Open() => View("/Care/AdmissionClinical/ArrivalDay.cshtml", new ArrivalDay());
}

public sealed class ArrivalDay
{
    public DateTime? ArrivesOn { get; set; }
}
