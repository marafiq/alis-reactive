using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class AbsenceCellsController : Controller
{
    [HttpGet(BenchPages.AbsenceMember)]
    public IActionResult AbsenceMember() => View("/Bench/Absences/AbsenceMember.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.AbsencePast)]
    public IActionResult AbsencePast() => View("/Bench/Absences/AbsencePast.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.AbsencePromise)]
    public IActionResult AbsencePromise() => View("/Bench/Absences/AbsencePromise.cshtml", TheWorldsBoard());

    private AbsenceBoard TheWorldsBoard()
    {
        var world = HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;
        return new AbsenceBoard(world.ComponentText, "a-1")
        {
            Version = world.ComponentVersion,
            ReadOnlyText = world.ComponentReadOnlyText,
            Count = world.ComponentCount,
            RecordDetail = world.ComponentRecordDetail,
        };
    }
}

public sealed record AbsenceBoard(string Text, string Value)
{
    public ComponentVersion Version { get; init; }

    public string ReadOnlyText { get; init; } = "";

    public int Count { get; init; }

    public string? RecordDetail { get; init; }
}

public static class AbsenceCells
{
    public static readonly AppEvent Show = AppEvent.Declare();
}
