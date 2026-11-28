using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class LedTeamsController : Controller
{
    private const string Teams = "/Refusals/FusionComponents/LedTeams.cshtml";

    [HttpGet(RefusalPages.LedTeamsByLead)]
    public IActionResult ByLead() => View(Teams, new LedTeams(true, new DateTime(2026, 11, 9), Monday(), DayTeam()));

    [HttpGet(RefusalPages.LedTeamsByName)]
    public IActionResult ByName() => View(Teams, new LedTeams(false, new DateTime(2026, 11, 9), Monday(), DayTeam()));

    private static IReadOnlyList<LedTeamShift> Monday()
        => [new LedTeamShift("Ada Brooks", new DateTime(2026, 11, 9, 9, 0, 0), new DateTime(2026, 11, 9, 17, 0, 0), 1)];

    private static IReadOnlyList<LedTeam> DayTeam() => [new LedTeam(1, "Day team", new TeamLead("Rose Achebe"))];
}

public sealed record LedTeams(bool ByLead, DateTime SelectedDate, IReadOnlyList<LedTeamShift> Shifts, IReadOnlyList<LedTeam> Teams);

public sealed record LedTeamShift(string Carer, DateTime Starts, DateTime Ends, int TeamCode);

public sealed record LedTeam(int Code, string Name, TeamLead Lead);

public sealed record TeamLead(string Name);
