using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Desks;

public sealed class DesksController : Controller
{
    public IActionResult Desk() => View(new MonitorDeskShift());
}

public sealed record MonitorDeskShift;
