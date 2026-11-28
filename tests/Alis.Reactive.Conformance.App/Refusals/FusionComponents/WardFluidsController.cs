using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class WardFluidsController : Controller
{
    [HttpGet(RefusalPages.WardFluidsByLead)]
    public IActionResult ByLead() => View("/Refusals/FusionComponents/WardFluidsByLead.cshtml", Wards());

    [HttpGet(RefusalPages.WardFluidsByWard)]
    public IActionResult ByWard() => View("/Refusals/FusionComponents/WardFluids.cshtml", Wards());

    private static WardFluids Wards() => new([new WardIntake("North", 1200, 1500, new WardLead("Rose Achebe"))]);
}

public sealed record WardFluids(IReadOnlyList<WardIntake> Wards);

public sealed record WardIntake(string Ward, int Taken, int Target, WardLead Lead);

public sealed record WardLead(string Name);
