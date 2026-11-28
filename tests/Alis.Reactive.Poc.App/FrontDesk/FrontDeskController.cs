using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.FrontDesk;

public sealed class FrontDeskController : Controller
{
    public IActionResult Badge() => View(new BadgePage("Helen Park"));
}

public sealed record BadgePage(string VisitorName);
