using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RespiteStayController : Controller
{
    [HttpGet(CarePages.RespiteStayBooked)]
    public IActionResult Booked() => View("/Care/AdmissionClinical/RespiteStay.cshtml", new RespiteStay { Stay = [new DateTime(2026, 7, 14), new DateTime(2026, 7, 20)] });

    [HttpGet(CarePages.RespiteStayOpen)]
    public IActionResult Open() => View("/Care/AdmissionClinical/RespiteStay.cshtml", new RespiteStay());
}

public sealed class RespiteStay
{
    public DateTime[]? Stay { get; set; }
}
