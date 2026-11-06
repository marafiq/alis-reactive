using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Rosters;

public sealed class RostersController : Controller
{
    public IActionResult Shift() => View(new ShiftRosterPage());

    public IActionResult Handover() => View(new HandoverPage());

    public IActionResult ReadOfACall() => View(new HandoverPage());

    public IActionResult ReadOfAnUnwrittenMember() => View(new HandoverPage());

    public IActionResult ReadThroughAnOverride() => View(new HandoverPage());

    public IActionResult ReadOfAnIgnoredOverride() => View(new HandoverPage());
}

public sealed record ShiftRosterPage;

public sealed record HandoverPage;
