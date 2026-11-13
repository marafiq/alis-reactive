using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoundDoseCountsController : Controller
{
    [HttpGet(CarePages.RoundDoseCounts)]
    public IActionResult OfTheMorningRound() => View("/Care/Residents/RoundDoseCounts.cshtml", new RoundDoseCounts("Morning"));

    [HttpGet(RoundDoseCountEndpoints.Doses)]
    public IActionResult Doses() => Json(new List<CountedDose>
    {
        new("Aspirin", CountedDoseStatus.Due, CountedDoseStatus.Due, 'I', 5, 2),
        new("Bisoprolol", CountedDoseStatus.Given, CountedDoseStatus.Due, 'O', 4, 3),
        new("Codeine", CountedDoseStatus.Due, CountedDoseStatus.Given, 'A', 4, null),
        new("Digoxin", CountedDoseStatus.Held, CountedDoseStatus.Held, 'T', 2, 1),
    });
}

public sealed record RoundDoseCounts(string Round);

public sealed record CountedDose(string Medicine, CountedDoseStatus Status, CountedDoseStatus Planned, char Route, int Milligrams, int? Priority);

public enum CountedDoseStatus
{
    Due,
    Given,
    Held,
}

public static class RoundDoseCountEndpoints
{
    public const string Doses = CarePages.RoundDoseCounts + "/doses";
}
