using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class Poc15ReturnedObjectsController : Controller
{
    [HttpGet(BenchPages.Poc15ReturnedObjectsPayloadBefore)]
    public IActionResult Poc15ReturnedObjectsPayloadBefore() => View("/Bench/Poc15/Poc15ReturnedObjectsPayloadBefore.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.Poc15ReturnedObjectsPayloadAfter)]
    public IActionResult Poc15ReturnedObjectsPayloadAfter() => View("/Bench/Poc15/Poc15ReturnedObjectsPayloadAfter.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.Poc15ReturnedObjectsHeldBefore)]
    public IActionResult Poc15ReturnedObjectsHeldBefore() => View("/Bench/Poc15/Poc15ReturnedObjectsHeldBefore.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.Poc15ReturnedObjectsHeldAfter)]
    public IActionResult Poc15ReturnedObjectsHeldAfter() => View("/Bench/Poc15/Poc15ReturnedObjectsHeldAfter.cshtml", TheWorldsBoard());

    private Poc15ReturnedObjectsBoard TheWorldsBoard()
    {
        var world = HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;
        return new Poc15ReturnedObjectsBoard(world.ComponentText, "a-1") { HeldMatchingItems = world.HeldMatchingItems };
    }
}

public sealed record Poc15ReturnedObjectsBoard(string Text, string Value)
{
    public int HeldMatchingItems { get; init; }
}
