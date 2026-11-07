using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Monitors;

public sealed class MonitorsController : Controller
{
    public IActionResult Alarms() => View(new MonitorsPage());
}

public sealed record MonitorsPage;
