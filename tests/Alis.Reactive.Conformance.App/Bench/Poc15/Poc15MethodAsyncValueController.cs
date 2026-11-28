using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class Poc15MethodAsyncValueController : Controller
{
    [HttpGet(BenchPages.Poc15MethodAsyncValuePayloadBefore)]
    public IActionResult Poc15MethodAsyncValuePayloadBefore() => View("/Bench/Poc15/Poc15MethodAsyncValuePayloadBefore.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.Poc15MethodAsyncValueHeldBefore)]
    public IActionResult Poc15MethodAsyncValueHeldBefore() => View("/Bench/Poc15/Poc15MethodAsyncValueHeldBefore.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.Poc15MethodAsyncValuePayloadAfter)]
    public IActionResult Poc15MethodAsyncValuePayloadAfter() => View("/Bench/Poc15/Poc15MethodAsyncValuePayloadAfter.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.Poc15MethodAsyncValueHeldAfter)]
    public IActionResult Poc15MethodAsyncValueHeldAfter() => View("/Bench/Poc15/Poc15MethodAsyncValueHeldAfter.cshtml", TheWorldsBoard());

    private Poc15MethodAsyncValueBoard TheWorldsBoard() => new(HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.ComponentText, "a-1");
}

public sealed record Poc15MethodAsyncValueBoard(string Text, string Value);
