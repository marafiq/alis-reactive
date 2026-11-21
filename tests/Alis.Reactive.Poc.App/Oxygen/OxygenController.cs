using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Oxygen;

public sealed class OxygenController : Controller
{
    public IActionResult Gauges() => View(new OxygenPage());
}

public sealed record OxygenPage;
