using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class SummarySendingController : Controller
{
    private const string PlainText = "text/plain";

    private const string Sending = "/Care/Handover/SummarySending.cshtml";

    [HttpGet(CarePages.SummarySendingWritten)]
    public IActionResult Written() => View(Sending, new SummarySending { Summary = "<p>Ate a full lunch.</p>" });

    [HttpGet(CarePages.SummarySendingBlank)]
    public IActionResult Blank() => View(Sending, new SummarySending());

    [HttpPost(SummarySendingEndpoints.Summaries)]
    public IActionResult Send([FromForm] SummarySending sending) => Content(sending.Summary ?? "No summary", PlainText);
}

public sealed class SummarySending
{
    public string? Summary { get; set; }
}

public static class SummarySendingEndpoints
{
    public const string Summaries = CarePages.SummarySendingWritten + "/summaries";
}

public static class SummarySendings
{
    public static readonly AppEvent Send = AppEvent.Declare();
}
