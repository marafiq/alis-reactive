using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Balances;

public sealed class BalancesController : Controller
{
    public IActionResult Branches() => View(new BalancesPage());

    public IActionResult ThreeSteps() => View(new BalancesPage());

    public IActionResult BothHold() => View(new BalancesPage());

    public IActionResult Rechecks() => View(new BalancesPage());

    public IActionResult OrderedDates() => View(new BalancesPage());

    public IActionResult OrderedFlags() => View(new BalancesPage());

    public IActionResult ComparedObjects() => View(new BalancesPage());

    public IActionResult ElseAfterAStep() => View(new BalancesPage());

    public IActionResult ResultOutsideItsBranch() => View(new BalancesPage());

    public IActionResult Measured() => View(new BalancesPage());

    public IActionResult MeasuredElseIf() => View(new BalancesPage());

    public IActionResult RecountedElseIf() => View(new BalancesPage());

    public IActionResult MeasuredBeforeAStep() => View(new BalancesPage());

    public IActionResult MeasuredAndRestored() => View(new BalancesPage());

    public IActionResult RecountInAnOr() => View(new BalancesPage());

    public IActionResult MeasuredBetweenWhenAndElseIf() => View(new BalancesPage());

    public IActionResult ElseIfResultAfterItsWhen() => View(new BalancesPage());

    public IActionResult TwoElses() => View(new BalancesPage());

    public IActionResult LightPatients() => View(new BalancesPage());

    public IActionResult FluidTargets() => View(new BalancesPage());

    public IActionResult RecountInAnIfNull() => View(new BalancesPage());

    public IActionResult IntakeTargets() => View(new BalancesPage());
}

public sealed record BalancesPage;
