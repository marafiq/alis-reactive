using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class PropertyCellsController : Controller
{
    [HttpGet(BenchPages.PropScalarRead)]
    public IActionResult PropScalarRead()
        => View("/Bench/Properties/PropScalarRead.cshtml", new ConformanceBoard(HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.ComponentText, "a-1"));
}

public sealed record ConformanceBoard(string Text, string Value);

public static class PropertyCells
{
    public static readonly AppEvent Show = AppEvent.Declare();
}
