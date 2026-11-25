using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class SummarySendingController : Controller
{
    private const string Sending = "/Care/Handover/SummarySending.cshtml";
    private const string Sent = "/Care/Handover/SummarySent.cshtml";

    [HttpGet(CarePages.SummarySendingWritten)]
    public IActionResult Written() => View(Sending, new SummarySending { Summary = "<p>Ate a full lunch.</p>" });

    [HttpGet(CarePages.SummarySendingBlank)]
    public IActionResult Blank() => View(Sending, new SummarySending());

    [HttpPost(SummarySendingEndpoints.Summaries)]
    public IActionResult Send() => View(Sent, new SummarySent(string.Join(", ", Request.Form.Keys)));
}

public sealed class SummarySending
{
    public string? Summary { get; set; }
}

public sealed record SummarySent(string Received);

public static class SummarySendingEndpoints
{
    public const string Summaries = CarePages.SummarySendingWritten + "/summaries";
}
