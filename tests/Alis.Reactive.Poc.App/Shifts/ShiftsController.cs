using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Shifts;

public sealed class ShiftsController : Controller
{
    public IActionResult Handover() => View(new ShiftsPage());

    public IActionResult VisitorBook() => View(new ShiftsPage());

    public IActionResult LogTwice() => View(new ShiftsPage());

    public IActionResult WardLantern() => View(new ShiftsPage());

    public IActionResult DottedPlugin() => View(new ShiftsPage());
}

public sealed record ShiftsPage;
