using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class TeamShiftsController : Controller
{
    [HttpGet(CarePages.TeamShifts)]
    public IActionResult OnShift() => View("/Care/RecordContacts/TeamShifts.cshtml", new TeamShifts(new DateTime(2026, 11, 9), Shifts(), [new CareTeam(1, "Day team"), new CareTeam(2, "Night team")]));

    private static IReadOnlyList<TeamShift> Shifts()
    {
        var ada = new TeamShift("Ada Brooks", new DateTime(2026, 11, 9, 9, 0, 0), new DateTime(2026, 11, 9, 17, 0, 0), 1);
        var lena = new TeamShift("Lena Ortiz", new DateTime(2026, 11, 10, 9, 0, 0), new DateTime(2026, 11, 10, 17, 0, 0), 2);
        return [ada, lena];
    }
}

public sealed record TeamShifts(DateTime SelectedDate, IReadOnlyList<TeamShift> Shifts, IReadOnlyList<CareTeam> Teams);

public sealed record TeamShift(string Carer, DateTime Starts, DateTime Ends, [property: JsonPropertyName("team")] int TeamCode);

public sealed record CareTeam([property: JsonPropertyName("code")] int Code, [property: JsonPropertyName("label")] string Name);
