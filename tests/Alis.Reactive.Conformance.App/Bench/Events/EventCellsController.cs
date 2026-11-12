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

    [HttpGet(BenchPages.EventList)]
    public IActionResult EventList() => View("/Bench/Events/EventList.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.EventMethod)]
    public IActionResult EventMethod() => View("/Bench/Events/EventMethod.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.EventPromise)]
    public IActionResult EventPromise() => View("/Bench/Events/EventPromise.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.EventDeepAsync)]
    public IActionResult EventDeepAsync() => View("/Bench/Events/EventDeepAsync.cshtml", TheWorldsBoard());

    private EventBoard TheWorldsBoard()
    {
        var world = HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;
        return new EventBoard(world.ComponentText, "a-1") { Version = world.ComponentVersion };
    }
}

public sealed record EventBoard(string Text, string Value)
{
    public ComponentVersion Version { get; init; }
}
