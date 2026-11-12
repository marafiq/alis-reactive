using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class MethodCellsController : Controller
{
    [HttpGet(BenchPages.MethodVoid)]
    public IActionResult MethodVoid() => View("/Bench/Methods/MethodVoid.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.MethodObjectMemberRead)]
    public IActionResult MethodObjectMemberRead() => View("/Bench/Methods/MethodObjectMemberRead.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.MethodObjectMethodCall)]
    public IActionResult MethodObjectMethodCall() => View("/Bench/Methods/MethodObjectMethodCall.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.MethodArgScalar)]
    public IActionResult MethodArgScalar() => View("/Bench/Methods/MethodArgScalar.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.MethodArgObject)]
    public IActionResult MethodArgObject() => View("/Bench/Methods/MethodArgObject.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.MethodArgList)]
    public IActionResult MethodArgList() => View("/Bench/Methods/MethodArgList.cshtml", TheWorldsBoard());

    private MethodBoard TheWorldsBoard()
    {
        var world = HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;
        return new MethodBoard(world.ComponentText, "a-1", world.ComponentCurrentItem);
    }
}

public sealed record MethodBoard(string Text, string Value, string CurrentItem);

public static class MethodCells
{
    public static readonly AppEvent Ping = AppEvent.Declare();

    public static readonly AppEvent Show = AppEvent.Declare();

    public static readonly AppEvent Act = AppEvent.Declare();

    public static readonly AppEvent Log = AppEvent.Declare();

    public static readonly AppEvent Take = AppEvent.Declare();

    public static readonly AppEvent TakeBoth = AppEvent.Declare();
}
