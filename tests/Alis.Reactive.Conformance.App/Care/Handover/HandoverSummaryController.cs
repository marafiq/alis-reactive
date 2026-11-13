using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class HandoverSummaryController : Controller
{
    private const string Summary = "/Care/Handover/HandoverSummary.cshtml";

    [HttpGet(CarePages.HandoverSummaryWritten)]
    public IActionResult Written() => View(Summary, new HandoverSummary { Summary = "<p>Ate a full lunch.</p>" });

    [HttpGet(CarePages.HandoverSummaryBlank)]
    public IActionResult Blank() => View(Summary, new HandoverSummary());
}

public sealed class HandoverSummary
{
    public string? Summary { get; set; }
}
