using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Desks;

public sealed class DesksController : Controller
{
    public IActionResult Desk() => View(new MonitorDeskShift());

    public IActionResult Check() => View(new MonitorDeskShift());

    public IActionResult Alarms() => View(new MonitorDeskShift());

    public IActionResult Station() => View(new MonitorDeskShift());

    public IActionResult Watch() => PartialView(new MonitorWatch());

    public IActionResult ListenById() => View(new MonitorDeskShift());

    public IActionResult ListenWithoutElement() => View(new MonitorDeskShift());
}

public sealed record MonitorDeskShift;

public sealed record MonitorWatch;
