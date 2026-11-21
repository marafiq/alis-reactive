using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class VisitActionsController : Controller
{
    private const string Actions = "/Care/RecordContacts/VisitActions.cshtml";

    [HttpGet(CarePages.VisitActionsToBook)]
    public IActionResult ToBook() => View(Actions, new VisitActions { Action = "Book a visit" });

    [HttpGet(CarePages.VisitActionsToCancel)]
    public IActionResult ToCancel() => View(Actions, new VisitActions { Action = "Cancel a visit" });
}

public sealed class VisitActions
{
    public required string Action { get; init; }
}

public static class VisitActionReads
{
    public static readonly AppEvent Show = AppEvent.Declare();
}
