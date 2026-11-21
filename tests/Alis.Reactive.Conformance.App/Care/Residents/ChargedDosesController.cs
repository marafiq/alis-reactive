using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ChargedDosesController : Controller
{
    [HttpGet(CarePages.ChargedDosesAboveNoCharge)]
    public IActionResult AboveNoCharge() => View("/Care/Residents/ChargedDoses.cshtml", new ChargeRound(0));

    [HttpGet(CarePages.ChargedDosesAboveThree)]
    public IActionResult AboveThree() => View("/Care/Residents/ChargedDoses.cshtml", new ChargeRound(3));

    [HttpGet(ChargedDoseEndpoints.Doses)]
    public IActionResult Doses() => Json(new List<ChargedDose>
    {
        new("Aspirin", 0m),
        new("Digoxin", 2.50m),
        new("Insulin", 4m),
    });
}

public sealed record ChargeRound(int FreeCharge);

public sealed record ChargedDose(string Medicine, decimal Charge);

public static class ChargedDoseEndpoints
{
    public const string Doses = CarePages.Area + "residents/charged-doses/doses";
}
