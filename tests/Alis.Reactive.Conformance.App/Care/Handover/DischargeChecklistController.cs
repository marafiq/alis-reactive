using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DischargeChecklistController : Controller
{
    private const string Checklist = "/Care/Handover/DischargeChecklist.cshtml";

    [HttpGet(CarePages.DischargeChecklist)]
    public IActionResult Closed() => View(Checklist, new DischargeChecklist { ShownAtOpen = false });

    [HttpGet(CarePages.DischargeChecklistShown)]
    public IActionResult Shown() => View(Checklist, new DischargeChecklist { ShownAtOpen = true });
}

public sealed class DischargeChecklist
{
    public required bool ShownAtOpen { get; init; }
}

public static class DischargeChecklists
{
    public static readonly AppEvent Open = AppEvent.Declare();
}
