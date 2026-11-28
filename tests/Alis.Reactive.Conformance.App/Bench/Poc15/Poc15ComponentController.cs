using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class Poc15ComponentController : Controller
{
    [HttpGet(BenchPages.Poc15ComponentPayloadBefore)]
    public IActionResult Poc15ComponentPayloadBefore() => View("/Bench/Poc15/Poc15ComponentPayloadBefore.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.Poc15ComponentPayloadAfter)]
    public IActionResult Poc15ComponentPayloadAfter() => View("/Bench/Poc15/Poc15ComponentPayloadAfter.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.Poc15ComponentHeldBefore)]
    public IActionResult Poc15ComponentHeldBefore() => View("/Bench/Poc15/Poc15ComponentHeldBefore.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.Poc15ComponentHeldAfter)]
    public IActionResult Poc15ComponentHeldAfter() => View("/Bench/Poc15/Poc15ComponentHeldAfter.cshtml", TheWorldsBoard());

    private Poc15ComponentBoard TheWorldsBoard() => new(HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.ComponentText, "a-1");
}

public sealed record Poc15ComponentBoard(string Text, string Value);
