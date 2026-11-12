using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class EventCellsController : Controller
{
    [HttpGet(BenchPages.EventScalar)]
    public IActionResult EventScalar() => View("/Bench/Events/EventScalar.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.EventObject)]
    public IActionResult EventObject() => View("/Bench/Events/EventObject.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.EventMethod)]
    public IActionResult EventMethod() => View("/Bench/Events/EventMethod.cshtml", TheWorldsBoard());

    private EventBoard TheWorldsBoard() => new(HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.ComponentText, "a-1");
}

public sealed record EventBoard(string Text, string Value);
