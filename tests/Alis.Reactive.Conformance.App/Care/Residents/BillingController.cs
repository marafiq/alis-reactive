using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class BillingController : Controller
{
    [HttpGet(CarePages.BillingWithUnitsPastAnInt)]
    public IActionResult WithUnitsPastAnInt() => View("/Care/Residents/Billing.cshtml", new Billing("March", BillingEndpoints.FiguresPastAnInt));

    [HttpGet(CarePages.BillingWithUnitsWithinAnInt)]
    public IActionResult WithUnitsWithinAnInt() => View("/Care/Residents/Billing.cshtml", new Billing("April", BillingEndpoints.FiguresWithinAnInt));

    [HttpGet(BillingEndpoints.FiguresPastAnInt)]
    public IActionResult FiguresPastAnInt() => Json(TheFigures(2_147_483_000, 1_000, -1_000));

    [HttpGet(BillingEndpoints.FiguresWithinAnInt)]
    public IActionResult FiguresWithinAnInt() => Json(TheFigures(2_147_483_000, 600));

    private static BillingFigures TheFigures(params int[] units) => new(
        [new BillingCharge(0.1m), new BillingCharge(0.2m)],
        [new BillingRate(0.1), new BillingRate(0.2)],
        [],
        [.. units.Select(billed => new BillingUnits(billed))]);
}

public sealed record Billing(string Month, string Figures);

public sealed record BillingFigures(List<BillingCharge> Charges, List<BillingRate> Rates, List<BillingCharge> Refunds, List<BillingUnits> Units);

public sealed record BillingCharge(decimal Amount);

public sealed record BillingRate(double Hourly);

public sealed record BillingUnits(int Billed);

public static class BillingEndpoints
{
    private const string Root = CarePages.Area + "residents/billing/";

    public const string FiguresPastAnInt = Root + "figures/past-an-int";

    public const string FiguresWithinAnInt = Root + "figures/within-an-int";
}

public static class Billings
{
    public static readonly AppEvent Sum = AppEvent.Declare();
}
