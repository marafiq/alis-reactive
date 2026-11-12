using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class PropertyCellsController : Controller
{
    [HttpGet(BenchPages.PropScalarRead)]
    public IActionResult PropScalarRead()
        => View("/Bench/Properties/PropScalarRead.cshtml", new ConformanceBoard(World.ComponentText, "a-1"));

    [HttpGet(BenchPages.PropScalarWrite)]
    public IActionResult PropScalarWrite()
        => View("/Bench/Properties/PropScalarWrite.cshtml", new ConformanceBoard(World.ComponentText, "a-1"));

    [HttpGet(BenchPages.PropDottedWrite)]
    public IActionResult PropDottedWrite()
        => View("/Bench/Properties/PropDottedWrite.cshtml", new ConformanceBoard(World.ComponentText, "a-1") { OuterCount = 8 });

    [HttpGet(BenchPages.PropObjectMemberRead)]
    public IActionResult PropObjectMemberRead()
        => View("/Bench/Properties/PropObjectMemberRead.cshtml", new ConformanceBoard(World.ComponentText, "a-1") { OuterText = World.ComponentOuterText });

    [HttpGet(BenchPages.PropObjectMemberWrite)]
    public IActionResult PropObjectMemberWrite()
        => View("/Bench/Properties/PropObjectMemberWrite.cshtml", new ConformanceBoard(World.ComponentText, "a-1") { OuterText = World.ComponentOuterText });

    [HttpGet(BenchPages.PropReadOnly)]
    public IActionResult PropReadOnly()
        => View("/Bench/Properties/PropReadOnly.cshtml", new ConformanceBoard(World.ComponentText, "a-1") { ReadOnlyText = World.ComponentReadOnlyText });

    [HttpGet(BenchPages.PropWriteOnly)]
    public IActionResult PropWriteOnly()
        => View("/Bench/Properties/PropWriteOnly.cshtml", new ConformanceBoard(World.ComponentText, "a-1"));

    private WorldData World => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;
}

public sealed record ConformanceBoard(string Text, string Value)
{
    public string OuterText { get; init; } = "";

    public int OuterCount { get; init; }

    public string ReadOnlyText { get; init; } = "";
}

public static class PropertyCells
{
    public static readonly AppEvent Show = AppEvent.Declare();

    public static readonly AppEvent Rename = AppEvent.Declare();

    public static readonly AppEvent SetToFour = AppEvent.Declare();

    public static readonly AppEvent RenameOuter = AppEvent.Declare();

    public static readonly AppEvent Write = AppEvent.Declare();
}
