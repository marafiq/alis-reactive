using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Signals;

public sealed class SignalsController : Controller
{
    public IActionResult Ward() => View(new SignalsPage());

    public IActionResult RungTwice() => View(new SignalsPage());
}

public sealed record SignalsPage;
