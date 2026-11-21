using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RosterRefreshController : Controller
{
    [HttpGet(CarePages.RosterRefreshUnderReplace)]
    public IActionResult UnderReplace() => View("/Care/Residents/RosterRefresh.cshtml", new RosterRefresh("North wing", HandlerPolicy.Replace, [DefaultHooks.SessionScript, ResidentsScripts.RosterRounds]));

    [HttpGet(CarePages.RosterRefreshUnderOverlap)]
    public IActionResult UnderOverlap() => View("/Care/Residents/RosterRefresh.cshtml", new RosterRefresh("North wing", HandlerPolicy.Overlap, [DefaultHooks.SessionScript, ResidentsScripts.RosterRounds]));

    [HttpGet(RosterRefreshEndpoints.Roster)]
    public IActionResult Roster() => NoContent();
}

public sealed record RosterRefresh(string Wing, HandlerPolicy Policy, IReadOnlyList<string> Scripts) : IScriptedResidentsPage;

public static class RosterRefreshes
{
    public static readonly AppEvent Refresh = AppEvent.Declare();
}

public static class RosterRefreshEndpoints
{
    public const string Roster = CarePages.Area + "residents/roster-refresh/roster";
}
