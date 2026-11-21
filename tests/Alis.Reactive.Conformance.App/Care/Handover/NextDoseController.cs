using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class NextDoseController : Controller
{
    private const string Dose = "/Care/Handover/NextDose.cshtml";

    [HttpGet(CarePages.NextDoseSet)]
    public IActionResult Set() => View(Dose, new NextDose { NextDoseAt = new DateTime(2026, 11, 10, 14, 30, 0) });

    [HttpGet(CarePages.NextDoseOpen)]
    public IActionResult Open() => View(Dose, new NextDose());
}

public sealed class NextDose
{
    public DateTime? NextDoseAt { get; set; }
}
