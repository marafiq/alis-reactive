using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class MedicationCabinetController : Controller
{
    private const string Cabinet = "/Care/Handover/MedicationCabinet.cshtml";

    [HttpGet(CarePages.MedicationCabinet)]
    public IActionResult Locked() => View(Cabinet, new CabinetLock { UnlockCode = "4821" });

    [HttpGet(CarePages.MedicationCabinetUnset)]
    public IActionResult Unset() => View(Cabinet, new CabinetLock());
}

public sealed class CabinetLock
{
    public string? UnlockCode { get; set; }
}
