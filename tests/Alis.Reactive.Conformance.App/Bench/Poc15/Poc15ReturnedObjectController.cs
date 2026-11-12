using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class Poc15ReturnedObjectController : Controller
{
    [HttpGet(BenchPages.Poc15ReturnedObjectPayloadBefore)]
    public IActionResult Poc15ReturnedObjectPayloadBefore() => View("/Bench/Poc15/Poc15ReturnedObjectPayloadBefore.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.Poc15ReturnedObjectPayloadAfter)]
    public IActionResult Poc15ReturnedObjectPayloadAfter() => View("/Bench/Poc15/Poc15ReturnedObjectPayloadAfter.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.Poc15ReturnedObjectHeldBefore)]
    public IActionResult Poc15ReturnedObjectHeldBefore() => View("/Bench/Poc15/Poc15ReturnedObjectHeldBefore.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.Poc15ReturnedObjectHeldAfter)]
    public IActionResult Poc15ReturnedObjectHeldAfter() => View("/Bench/Poc15/Poc15ReturnedObjectHeldAfter.cshtml", TheWorldsBoard());

    private Poc15ReturnedObjectBoard TheWorldsBoard() => new(HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.ComponentText, "a-1");
}

public sealed record Poc15ReturnedObjectBoard(string Text, string Value);
