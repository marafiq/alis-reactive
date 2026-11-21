using System.Globalization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoundLookupController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.RoundLookup)]
    public IActionResult OfTheNinth() => View("/Care/Round/RoundLookup.cshtml", new RoundLookup(new DateOnly(2026, 11, 9), new TimeOnly(13, 30)));

    [HttpGet(CarePages.RoundLookupOfTheFirstOfDecember)]
    public IActionResult OfTheFirstOfDecember() => View("/Care/Round/RoundLookup.cshtml", new RoundLookup(new DateOnly(2026, 12, 1), new TimeOnly(7, 5)));

    [HttpGet(RoundLookupEndpoints.Find)]
    public IActionResult Find([FromQuery] RoundLookupQuery round)
        => Content("Round on " + round.Day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " at " + round.At.ToString("HH:mm:ss", CultureInfo.InvariantCulture), PlainText);
}

public sealed record RoundLookup(DateOnly Day, TimeOnly At);

public sealed class RoundLookupQuery
{
    public DateOnly Day { get; set; }

    public TimeOnly At { get; set; }
}

public static class RoundLookupButtons
{
    public static readonly AppEvent FindTheRound = AppEvent.Declare();
}

public static class RoundLookupEndpoints
{
    public const string Find = CarePages.RoundLookup + "/find";
}
