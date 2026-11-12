using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class Poc15CallbackController : Controller
{
    [HttpGet(BenchPages.Poc15CallbackPayloadBefore)]
    public IActionResult Poc15CallbackPayloadBefore() => View("/Bench/Poc15/Poc15CallbackPayloadBefore.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.Poc15CallbackHeldBefore)]
    public IActionResult Poc15CallbackHeldBefore() => View("/Bench/Poc15/Poc15CallbackHeldBefore.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.Poc15CallbackHeldAfter)]
    public IActionResult Poc15CallbackHeldAfter() => View("/Bench/Poc15/Poc15CallbackHeldAfter.cshtml", TheWorldsBoard());

    private Poc15CallbackBoard TheWorldsBoard() => new(HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.ComponentText, "a-1");
}

public sealed record Poc15CallbackBoard(string Text, string Value);
