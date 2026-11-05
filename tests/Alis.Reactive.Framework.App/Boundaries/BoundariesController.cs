using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Boundaries;

public sealed class BoundariesController : Controller
{
    public IActionResult Desk() => View(new DeskPage());

    public IActionResult Late() => View(new DeskPage());

    public IActionResult Bare() => View(new DeskPage());
}
