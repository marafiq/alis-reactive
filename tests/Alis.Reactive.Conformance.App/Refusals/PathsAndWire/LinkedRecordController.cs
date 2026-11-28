using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class LinkedRecordController : Controller
{
    private static readonly DateTime TheRound = new(2026, 3, 1, 8, 0, 0);

    [HttpGet(RefusalPages.LinkedRecord)]
    public IActionResult LinkedRecord() => View("/Refusals/PathsAndWire/LinkedRecord.cshtml", TheRoundAndTheOneBefore());

    [HttpGet(RefusalPages.LinkedDays)]
    public IActionResult LinkedDays() => View("/Refusals/PathsAndWire/LinkedDays.cshtml", TheRoundAndTheOneBefore());

    private static LinkedRounds TheRoundAndTheOneBefore()
    {
        var before = new Linked { Day = TheRound.AddDays(-1) };
        return new LinkedRounds(new Linked { Day = TheRound, Previous = before }, [TheRound, before.Day]);
    }
}

public sealed record LinkedRounds(Linked Round, List<DateTime> Days);

public static class LinkedRecords
{
    public static readonly AppEvent TakeTheLinked = AppEvent.Declare();
}
