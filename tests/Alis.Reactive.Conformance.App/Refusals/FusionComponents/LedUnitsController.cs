using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class LedUnitsController : Controller
{
    private const string Units = "/Refusals/FusionComponents/LedUnits.cshtml";

    [HttpGet(RefusalPages.LedUnitsByLead)]
    public IActionResult ByLead() => View(Units, new LedUnits(true, North()));

    [HttpGet(RefusalPages.LedUnitsByWard)]
    public IActionResult ByWard() => View(Units, new LedUnits(false, North()));

    private static IReadOnlyList<LedUnit> North() => [new LedUnit("North", 1, new UnitLead("Rose Achebe"))];
}

public sealed record LedUnits(bool ByLead, IReadOnlyList<LedUnit> Units);

public sealed record LedUnit(string Ward, int Beds, UnitLead Lead);

public sealed record UnitLead(string Name);
