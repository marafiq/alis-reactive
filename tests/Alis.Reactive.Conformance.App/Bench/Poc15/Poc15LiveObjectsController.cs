using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class Poc15LiveObjectsController : Controller
{
    [HttpGet(BenchPages.Poc15LiveObjectsPayloadBefore)]
    public IActionResult Poc15LiveObjectsPayloadBefore() => View("/Bench/Poc15/Poc15LiveObjectsPayloadBefore.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.Poc15LiveObjectsPayloadAfter)]
    public IActionResult Poc15LiveObjectsPayloadAfter() => View("/Bench/Poc15/Poc15LiveObjectsPayloadAfter.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.Poc15LiveObjectsHeldBefore)]
    public IActionResult Poc15LiveObjectsHeldBefore() => View("/Bench/Poc15/Poc15LiveObjectsHeldBefore.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.Poc15LiveObjectsHeldAfter)]
    public IActionResult Poc15LiveObjectsHeldAfter() => View("/Bench/Poc15/Poc15LiveObjectsHeldAfter.cshtml", TheWorldsBoard());

    private Poc15LiveObjectsBoard TheWorldsBoard()
    {
        var world = HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;
        return new Poc15LiveObjectsBoard(world.ComponentText, "a-1") { HeldItems = FourItemsTheFirstNamed(world.HeldNamedItems) };
    }

    private static List<ItemStart> FourItemsTheFirstNamed(int named)
    {
        string[] names = ["First", "Second", "Third", "Fourth"];
        return [.. names.Select((name, position) => new ItemStart { Name = position < named ? name : "" })];
    }
}

public sealed record Poc15LiveObjectsBoard(string Text, string Value)
{
    public IReadOnlyList<ItemStart> HeldItems { get; init; } = [];
}
