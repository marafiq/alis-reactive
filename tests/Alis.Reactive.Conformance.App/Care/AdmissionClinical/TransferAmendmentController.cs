using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class TransferAmendmentController : Controller
{
    [HttpGet(CarePages.TransferAmendment)]
    public IActionResult Amendment() => View("/Care/AdmissionClinical/TransferAmendment.cshtml", new TransferAmendment());
}

public sealed class TransferAmendment
{
    public bool Safeguards { get; set; }
    public string? Route { get; set; }
    public int? NoticeMinutes { get; set; }
    public bool? Escort { get; set; }
}
