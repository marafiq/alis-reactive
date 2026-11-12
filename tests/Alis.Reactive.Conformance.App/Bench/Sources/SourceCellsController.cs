using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class SourceCellsController : Controller
{
    [HttpGet(BenchPages.SourceElementText)]
    public IActionResult SourceElementText() => View("/Bench/Sources/SourceElementText.cshtml", TheWorldsBoard());

    private SourceBoard TheWorldsBoard() => new(HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.ComponentText, "a-1", ["a-1", "a-2"]);
}

public sealed record SourceBoard(string Text, string Value, IReadOnlyList<string> Codes);

public static class SourceCells
{
    public static readonly AppEvent Show = AppEvent.Declare();
}
