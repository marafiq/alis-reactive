using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoundResidentsController : Controller
{
    [HttpGet(CarePages.RoundResidents)]
    public IActionResult ToSelect() => View("/Care/Handover/RoundSelection.cshtml", new RoundResidents { Residents = OfTheRound() });

    [HttpGet(CarePages.RoundResidentsWithLena)]
    public IActionResult WithLena() => View("/Care/Handover/RoundUnselection.cshtml", new RoundResidents { Residents = OfTheRound() });

    private static IReadOnlyList<RoundEntry> OfTheRound() => [new RoundEntry("ada", "Ada Brooks"), new RoundEntry("lena", "Lena Ortiz")];
}

public sealed class RoundResidents
{
    public IReadOnlyList<RoundEntry> Residents { get; init; } = [];
}

public sealed record RoundEntry(string Id, string Text);

public static class RoundSelections
{
    public static readonly AppEvent SelectLena = AppEvent.Declare();

    public static readonly AppEvent UnselectLena = AppEvent.Declare();
}
