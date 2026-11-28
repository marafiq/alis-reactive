using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class Poc15WriteOnlyController : Controller
{
    [HttpGet(BenchPages.Poc15WriteOnlyPayloadBefore)]
    public IActionResult Poc15WriteOnlyPayloadBefore() => View("/Bench/Poc15/Poc15WriteOnlyPayloadBefore.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.Poc15WriteOnlyHeldBefore)]
    public IActionResult Poc15WriteOnlyHeldBefore() => View("/Bench/Poc15/Poc15WriteOnlyHeldBefore.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.Poc15WriteOnlyHeldAfter)]
    public IActionResult Poc15WriteOnlyHeldAfter() => View("/Bench/Poc15/Poc15WriteOnlyHeldAfter.cshtml", TheWorldsBoard());

    private Poc15WriteOnlyBoard TheWorldsBoard() => new(HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.ComponentText, "a-1");
}

public sealed record Poc15WriteOnlyBoard(string Text, string Value);
