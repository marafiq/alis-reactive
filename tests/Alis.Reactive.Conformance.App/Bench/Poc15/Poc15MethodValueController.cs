using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class Poc15MethodValueController : Controller
{
    [HttpGet(BenchPages.Poc15MethodValuePayloadBefore)]
    public IActionResult Poc15MethodValuePayloadBefore() => View("/Bench/Poc15/Poc15MethodValuePayloadBefore.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.Poc15MethodValuePayloadAfter)]
    public IActionResult Poc15MethodValuePayloadAfter() => View("/Bench/Poc15/Poc15MethodValuePayloadAfter.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.Poc15MethodValueHeldBefore)]
    public IActionResult Poc15MethodValueHeldBefore() => View("/Bench/Poc15/Poc15MethodValueHeldBefore.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.Poc15MethodValueHeldAfter)]
    public IActionResult Poc15MethodValueHeldAfter() => View("/Bench/Poc15/Poc15MethodValueHeldAfter.cshtml", TheWorldsBoard());

    private Poc15MethodValueBoard TheWorldsBoard()
    {
        var world = HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;
        return new Poc15MethodValueBoard(world.ComponentText, "a-1") { HeldTotal = world.HeldTotal };
    }
}

public sealed record Poc15MethodValueBoard(string Text, string Value)
{
    public int HeldTotal { get; init; }
}
