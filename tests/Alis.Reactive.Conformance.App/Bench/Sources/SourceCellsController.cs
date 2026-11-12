using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class SourceCellsController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(BenchPages.SourcePropertySet)]
    public IActionResult SourcePropertySet() => View("/Bench/Sources/SourcePropertySet.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.SourceWriteOnlySet)]
    public IActionResult SourceWriteOnlySet() => View("/Bench/Sources/SourceWriteOnlySet.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.SourceDispatchPayloadMember)]
    public IActionResult SourceDispatchPayloadMember() => View("/Bench/Sources/SourceDispatchPayloadMember.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.SourceMethodArgument)]
    public IActionResult SourceMethodArgument() => View("/Bench/Sources/SourceMethodArgument.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.SourcePluginArgument)]
    public IActionResult SourcePluginArgument() => View("/Bench/Sources/SourcePluginArgument.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.SourceElementText)]
    public IActionResult SourceElementText() => View("/Bench/Sources/SourceElementText.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.SourceElementHtml)]
    public IActionResult SourceElementHtml() => View("/Bench/Sources/SourceElementHtml.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.SourceElementClass)]
    public IActionResult SourceElementClass() => View("/Bench/Sources/SourceElementClass.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.SourceConditionLeft)]
    public IActionResult SourceConditionLeft() => View("/Bench/Sources/SourceConditionLeft.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.SourceRequestMember)]
    public IActionResult SourceRequestMember() => View("/Bench/Sources/SourceRequestMember.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.SourceHeader)]
    public IActionResult SourceHeader() => View("/Bench/Sources/SourceHeader.cshtml", TheWorldsBoard());

    [HttpPost(BenchEndpoints.Echo)]
    public IActionResult Echo([FromBody] EchoRequest request) => Content($"Received {request.Code}".TrimEnd(), PlainText);

    [HttpPost(BenchEndpoints.EchoHeader)]
    public IActionResult EchoHeader([FromHeader(Name = "X-Code")] string? code) => Content($"X-Code {code}".TrimEnd(), PlainText);

    private SourceBoard TheWorldsBoard() => new(HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.ComponentText, "a-1", ["a-1", "a-2"]);
}

public sealed record SourceBoard(string Text, string Value, IReadOnlyList<string> Codes);

public sealed class EchoRequest
{
    public string? Code { get; set; }
}

public sealed class PickedCode
{
    public string? Code { get; set; }
}

public static class SourceCells
{
    public static readonly AppEvent Show = AppEvent.Declare();

    public static readonly AppEvent Mark = AppEvent.Declare();

    public static readonly AppEvent Check = AppEvent.Declare();

    public static readonly AppEvent Send = AppEvent.Declare();

    public static readonly AppEvent Copy = AppEvent.Declare();

    public static readonly AppEvent Write = AppEvent.Declare();

    public static readonly AppEvent Log = AppEvent.Declare();

    public static readonly AppEvent Tell = AppEvent.Declare();

    public static readonly AppEvent<PickedCode> Picked = AppEvent.Declare<PickedCode>();
}
