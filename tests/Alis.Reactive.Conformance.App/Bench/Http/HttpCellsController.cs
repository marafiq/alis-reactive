using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class HttpCellsController : Controller
{
    [HttpGet(BenchPages.HttpGet)]
    public IActionResult HttpGet() => View("/Bench/Http/HttpGet.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.HttpPost)]
    public IActionResult HttpPost() => View("/Bench/Http/HttpPost.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.HttpPut)]
    public IActionResult HttpPut() => View("/Bench/Http/HttpPut.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.HttpDelete)]
    public IActionResult HttpDelete() => View("/Bench/Http/HttpDelete.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.HttpBesideRoute)]
    public IActionResult HttpBesideRoute() => View("/Bench/Http/HttpBesideRoute.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.HttpBesideQuery)]
    public IActionResult HttpBesideQuery() => View("/Bench/Http/HttpBesideQuery.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.HttpBesideHeader)]
    public IActionResult HttpBesideHeader() => View("/Bench/Http/HttpBesideHeader.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.HttpBesideStatic)]
    public IActionResult HttpBesideStatic() => View("/Bench/Http/HttpBesideStatic.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.HttpInto)]
    public IActionResult HttpInto() => View("/Bench/Http/HttpInto.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.HttpWhileLoading)]
    public IActionResult HttpWhileLoading() => View("/Bench/Http/HttpWhileLoading.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.HttpResponseSuccess)]
    public IActionResult HttpResponseSuccess() => View("/Bench/Http/HttpResponseSuccess.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.HttpResponseError)]
    public IActionResult HttpResponseError() => View("/Bench/Http/HttpResponseError.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.HttpChained)]
    public IActionResult HttpChained() => View("/Bench/Http/HttpChained.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.HttpParallel)]
    public IActionResult HttpParallel() => View("/Bench/Http/HttpParallel.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.HttpFinally)]
    public IActionResult HttpFinally() => View("/Bench/Http/HttpFinally.cshtml", TheWorldsBoard());

    [HttpGet(BenchEndpoints.Items)]
    public async Task<IActionResult> Items(ItemsQuery query)
    {
        await World.Reach(GateName.Of(RequestGate.Route), Request.Path);
        await World.Reach(GateName.Of(LanesGate.Respond), Request.Path);
        return Json(new ItemsSummary(query.Code is { } code ? $"3 items for {code}" : "3 items"));
    }

    [HttpPost(BenchEndpoints.Items)]
    public IActionResult RecordItem([FromBody] ItemCode item) => Json(new ItemsSummary($"Recorded {item.Code}"));

    [HttpPut(BenchEndpoints.Item)]
    public IActionResult MoveItem([FromForm] ItemCode item) => Json(new ItemsSummary($"Moved {item.Code}"));

    [HttpDelete(BenchEndpoints.Item)]
    public IActionResult CloseItem([FromBody] ItemCode item) => Json(new ItemsSummary($"Closed {item.Code}"));

    [HttpPost(BenchEndpoints.GroupNotes)]
    public IActionResult GroupNotes(string group, [FromBody] GroupNote note) => Content($"{group}: {note.Code}");

    [HttpPost(BenchEndpoints.Notes)]
    public IActionResult Notes([FromBody] Note note)
        => Content(Request.Headers.TryGetValue("X-Mode", out var mode) ? $"X-Mode {mode}" : "X-Mode none");

    [HttpPost(BenchEndpoints.Move)]
    public IActionResult Move() => World.MoveStatus switch
    {
        StatusCodes.Status200OK => Json(new MovedCode("a-2")),
        StatusCodes.Status422UnprocessableEntity => new JsonResult(new MoveRefusal("a-2 is closed")) { StatusCode = StatusCodes.Status422UnprocessableEntity },
        var status => StatusCode(status),
    };

    [HttpGet(BenchEndpoints.Code)]
    public async Task<IActionResult> Code()
    {
        await World.Reach(GateName.Of(LanesGate.Respond), Request.Path);
        return World.CodeRefuses ? StatusCode(StatusCodes.Status500InternalServerError) : Json(new MovedCode("a-2"));
    }

    [HttpGet(BenchEndpoints.Card)]
    public IActionResult Card() => PartialView("/Bench/Http/HttpIntoCard.cshtml", new HttpCard("Card component", "a-1", ["a-1", "a-2"]));

    private WorldData World => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;

    private HttpBoard TheWorldsBoard()
        => new(World.ComponentText, "a-1", ["a-1", "a-2"]) { Version = World.ComponentVersion, ReadOnlyText = World.ComponentReadOnlyText, Group = World.NotesGroup };
}

public sealed record HttpBoard(string Text, string Value, IReadOnlyList<string> Codes)
{
    public ComponentVersion Version { get; init; }

    public string ReadOnlyText { get; init; } = "";

    public string Group { get; init; } = "";
}

public sealed record HttpCard(string Text, string Value, IReadOnlyList<string> Codes);

public sealed class ItemsQuery
{
    public string? Code { get; set; }

    public DateTime? Day { get; set; }
}

public sealed record ItemsSummary(string Summary);

public sealed class ItemCode
{
    public string? Code { get; set; }
}

public sealed class GroupNote
{
    public string? Group { get; set; }

    public string? Code { get; set; }
}

public sealed class Note
{
    public string? Code { get; set; }

    public string? Source { get; set; }
}

public sealed record MovedCode(string Code);

public sealed record MoveRefusal(string Message);

public static class HttpCells
{
    public static readonly AppEvent Load = AppEvent.Declare();

    public static readonly AppEvent LoadWithoutTheHeader = AppEvent.Declare();

    public static readonly AppEvent LoadWithoutTheLiteral = AppEvent.Declare();

    public static readonly AppEvent OpenCard = AppEvent.Declare();
}
