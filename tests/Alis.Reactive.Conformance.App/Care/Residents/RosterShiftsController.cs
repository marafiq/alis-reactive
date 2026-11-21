using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RosterShiftsController : Controller
{
    [HttpGet(CarePages.RosterShiftsUnderReplace)]
    public IActionResult UnderReplace() => View("/Care/Residents/RosterShifts.cshtml", new RosterShifts("North wing", HandlerPolicy.Replace, [DefaultHooks.SessionScript]));

    [HttpGet(CarePages.RosterShiftsUnderOverlap)]
    public IActionResult UnderOverlap() => View("/Care/Residents/RosterShifts.cshtml", new RosterShifts("North wing", HandlerPolicy.Overlap, [DefaultHooks.SessionScript]));

    [HttpGet(RosterShiftsEndpoints.Roster)]
    public IActionResult Roster() => NoContent();

    [HttpGet(CareEndpoints.RosterShifts)]
    public async Task<IActionResult> Shifts()
    {
        await World.Reach(GateName.Of(LanesGate.Respond), Request.Path);
        return NoContent();
    }

    private WorldData World => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;
}

public sealed record RosterShifts(string Wing, HandlerPolicy Policy, IReadOnlyList<string> Scripts) : IScriptedResidentsPage;

public static class RosterShiftsEvents
{
    public static readonly AppEvent Refresh = AppEvent.Declare();
}

public static class RosterShiftsEndpoints
{
    public const string Roster = CarePages.Area + "residents/roster-shifts/roster";
}
