using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ArrivalRegisterPositionsController : Controller
{
    [HttpGet(CarePages.ArrivalCenteredHandover)]
    public IActionResult ArrivalCenteredHandover() => View("/Care/AdmissionClinical/ArrivalHandoverColumn.cshtml", new ArrivalHandoverColumn { Alternate = false });

    [HttpGet(CarePages.ArrivalAlignedReturn)]
    public IActionResult ArrivalAlignedReturn() => View("/Care/AdmissionClinical/ArrivalHandoverColumn.cshtml", new ArrivalHandoverColumn { Alternate = true });

    [HttpGet(CarePages.ArrivalPinnedReference)]
    public IActionResult ArrivalPinnedReference() => View("/Care/AdmissionClinical/ArrivalTrackingRegister.cshtml", new ArrivalTrackingRegister { Alternate = false });

    [HttpGet(CarePages.ArrivalFollowingBooking)]
    public IActionResult ArrivalFollowingBooking() => View("/Care/AdmissionClinical/ArrivalTrackingRegister.cshtml", new ArrivalTrackingRegister { Alternate = true });
}

public sealed class ArrivalHandoverColumn
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);

    public bool Alternate { get; init; }
}

public sealed class ArrivalTrackingRegister
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);

    public bool Alternate { get; init; }
}
