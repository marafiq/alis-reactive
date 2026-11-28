using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class NursedRoundController : Controller
{
    private const string Round = "/Refusals/FusionComponents/NursedRound.cshtml";

    [HttpGet(RefusalPages.NursedRoundByNurse)]
    public IActionResult ByNurse() => View(Round, new NursedRound(true, Doses()));

    [HttpGet(RefusalPages.NursedRoundByMedicine)]
    public IActionResult ByMedicine() => View(Round, new NursedRound(false, Doses()));

    private static IReadOnlyList<NursedDose> Doses() => [new NursedDose("Aspirin", "Due", new RoundNurse("Rose Achebe"))];
}

public sealed record NursedRound(bool HeadedByNurse, IReadOnlyList<NursedDose> Doses);

public sealed record NursedDose(string Medicine, string Status, RoundNurse Nurse);

public sealed record RoundNurse(string Name);
