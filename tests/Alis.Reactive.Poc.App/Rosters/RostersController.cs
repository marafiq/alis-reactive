using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Rosters;

public sealed class RostersController : Controller
{
    public IActionResult Shift() => View(new ShiftRosterPage());
}

public sealed record ShiftRosterPage;
