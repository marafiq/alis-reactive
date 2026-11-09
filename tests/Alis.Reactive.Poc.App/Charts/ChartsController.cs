using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Charts;

public sealed class ChartsController : Controller
{
    public IActionResult Doses() => View(new ChartsPage());

    public IActionResult ChartUnit() => View(new ChartsPage());

    public IActionResult AwaitedInAnAnswer() => View(new ChartsPage());

    public IActionResult StepInAnAnswer() => View(new ChartsPage());

    public IActionResult TickKeptOutOfItsAnswer() => View(new ChartsPage());

    public IActionResult UnlabelledTick() => View(new ChartsPage());
}

public sealed record ChartsPage;
