using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ArrivalCalendarChoicesController : Controller
{
    [HttpGet(CarePages.ArrivalFamilyTransport)]
    public IActionResult ArrivalFamilyTransport() => View("/Care/AdmissionClinical/ArrivalTransportChoice.cshtml", new TransferBookingChoice { OfficeArranged = false });

    [HttpGet(CarePages.ArrivalOfficeTransport)]
    public IActionResult ArrivalOfficeTransport() => View("/Care/AdmissionClinical/ArrivalTransportChoice.cshtml", new TransferBookingChoice { OfficeArranged = true });

    [HttpGet(CarePages.ArrivalFamilyFinalization)]
    public IActionResult ArrivalFamilyFinalization() => View("/Care/AdmissionClinical/ArrivalClosingChoice.cshtml", new ArrivalHandoverDecision { OfficeArranged = false });

    [HttpGet(CarePages.ArrivalOfficeFinalization)]
    public IActionResult ArrivalOfficeFinalization() => View("/Care/AdmissionClinical/ArrivalClosingChoice.cshtml", new ArrivalHandoverDecision { OfficeArranged = true });
}

public sealed class TransferBookingChoice
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);

    public DateTime? TransportOn { get; set; } = new DateTime(2026, 7, 17);

    public bool OfficeArranged { get; init; }
}

public sealed class ArrivalHandoverDecision
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);

    public bool OfficeArranged { get; init; }
}
