using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Pumps;

public sealed class PumpsController : Controller
{
    public IActionResult Infusion() => View(new InfusionPage());

    public IActionResult PausedInALambda() => View(new InfusionPage());
}

public sealed record InfusionPage;
