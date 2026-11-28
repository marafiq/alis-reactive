using System.Globalization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class VisitRoundController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.VisitRound)]
    public IActionResult Round() => View("/Care/Admission/VisitRound.cshtml", new VisitRound
    {
        Visits = [new RoundVisit { When = new DateTime(2026, 11, 30), Note = "Bloods" }, new RoundVisit { When = new DateTime(2026, 12, 1) }],
    });

    [HttpPost(VisitRoundEndpoints.Rounds)]
    public IActionResult Save([FromForm] VisitRound round)
        => Content($"{round.Resident}: {string.Join(", ", round.Visits.Select(visit => visit.When.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)))}", PlainText);
}

public sealed class VisitRound
{
    public string? Resident { get; set; }

    public List<RoundVisit> Visits { get; set; } = [];
}

public sealed class RoundVisit
{
    public DateTime When { get; set; }

    public string? Note { get; set; }
}

public static class VisitRoundEndpoints
{
    public const string Rounds = CarePages.VisitRound + "/rounds";
}
