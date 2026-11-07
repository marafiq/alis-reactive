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
}

public sealed record BalancesPage;
