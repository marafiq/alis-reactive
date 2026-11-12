using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoundVialsController : Controller
{
    [HttpGet(CarePages.RoundVialsWithASupplyMissing)]
    public IActionResult ASupplyMissing() => View("/Care/Residents/RoundVials.cshtml", new VialRound(RoundVialEndpoints.DosesWithASupplyMissing));

    [HttpGet(CarePages.RoundVialsWithEverySupply)]
    public IActionResult EverySupply() => View("/Care/Residents/RoundVials.cshtml", new VialRound(RoundVialEndpoints.Doses));

    [HttpGet(RoundVialEndpoints.DosesWithASupplyMissing)]
    public IActionResult DosesWithASupplyMissing() => Json(new List<VialDose>
    {
        new("Aspirin", new VialSupply(2)),
        new("Digoxin", null),
        new("Insulin", new VialSupply(5)),
    });

    [HttpGet(RoundVialEndpoints.Doses)]
    public IActionResult Doses() => Json(new List<VialDose>
    {
        new("Aspirin", new VialSupply(2)),
        new("Digoxin", new VialSupply(10)),
        new("Insulin", new VialSupply(5)),
    });
}

public sealed record VialRound(string Doses);

public sealed record VialDose(string Medicine, VialSupply? Supply);

public sealed record VialSupply(int Vials);

public static class RoundVialEndpoints
{
    private const string Root = CarePages.Area + "residents/round-vials/";

    public const string Doses = Root + "doses";

    public const string DosesWithASupplyMissing = Root + "doses-with-a-supply-missing";
}
