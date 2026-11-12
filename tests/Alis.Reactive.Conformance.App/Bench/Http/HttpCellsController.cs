using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class HttpCellsController : Controller
{
    [HttpGet(BenchPages.HttpGet)]
    public IActionResult HttpGet() => View("/Bench/Http/HttpGet.cshtml", TheWorldsBoard());

    [HttpGet(BenchEndpoints.Items)]
    public IActionResult Items(ItemsQuery query) => Json(new ItemsSummary($"3 items for {query.Code}"));

    private HttpBoard TheWorldsBoard() => new(HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.ComponentText, "a-1", ["a-1", "a-2"]);
}

public sealed record HttpBoard(string Text, string Value, IReadOnlyList<string> Codes);

public sealed class ItemsQuery
{
    public string? Code { get; set; }
}

public sealed record ItemsSummary(string Summary);

public static class HttpCells
{
    public static readonly AppEvent Load = AppEvent.Declare();
}
