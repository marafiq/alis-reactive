using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RosterRecountController : Controller
{
    [HttpGet(CarePages.RosterRecountUnderReplace)]
    public IActionResult UnderReplace() => View("/Care/Residents/RosterRecount.cshtml", new RosterRecount("North wing", HandlerPolicy.Replace, [DefaultHooks.SessionScript, ResidentsScripts.RosterRounds]));

    [HttpGet(CarePages.RosterRecountUnderOverlap)]
    public IActionResult UnderOverlap() => View("/Care/Residents/RosterRecount.cshtml", new RosterRecount("North wing", HandlerPolicy.Overlap, [DefaultHooks.SessionScript, ResidentsScripts.RosterRounds]));

    [HttpGet(RosterRecountEndpoints.Roster)]
    public IActionResult Roster() => NoContent();
}

public sealed record RosterRecount(string Wing, HandlerPolicy Policy, IReadOnlyList<string> Scripts) : IScriptedResidentsPage;

public static class RosterRecounts
{
    public static readonly AppEvent Refresh = AppEvent.Declare();
}

public static class RosterRecountEndpoints
{
    public const string Roster = CarePages.Area + "residents/roster-recount/roster";
}
