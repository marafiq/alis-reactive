using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Monitors;

public sealed class MonitorsController : Controller
{
    public IActionResult Alarms() => View(new MonitorsPage());

    public IActionResult Acknowledgements() => View(new MonitorsPage());

    public IActionResult CancelledAfterAnAwait() => View(new MonitorsPage());

    public IActionResult AwaitedInOneBranch() => View(new MonitorsPage());

    public IActionResult CancelledAfterABranchThatAwaits() => View(new MonitorsPage());

    public IActionResult CancelledAfterAnElseIf() => View(new MonitorsPage());

    public IActionResult CancelledAfterAnElseIfThatAwaits() => View(new MonitorsPage());

    public IActionResult NotesShown() => View(new MonitorsPage());
}

public sealed record MonitorsPage;
