using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class TransferSummaryController : Controller
{
    private const string Localhost = "localhost";

    private const string Loopback = "127.0.0.1";

    [HttpGet(CarePages.TransferSummaryMovedToAnotherOrigin)]
    public IActionResult MovedToAnotherOrigin() => View("/Care/Residents/TransferSummary.cshtml", new TransferSummaryPage("Ada Hart", TransferSummaryEndpoints.MovedToAnotherOrigin));

    [HttpGet(CarePages.TransferSummaryMovedWithinThisOrigin)]
    public IActionResult MovedWithinThisOrigin() => View("/Care/Residents/TransferSummary.cshtml", new TransferSummaryPage("Ada Hart", TransferSummaryEndpoints.MovedWithinThisOrigin));

    [HttpGet(TransferSummaryEndpoints.MovedToAnotherOrigin)]
    public IActionResult SummaryMovedToAnotherOrigin() => Redirect($"{Request.Scheme}://{TheOtherNameOfThisHost()}{TransferSummaryEndpoints.Summary}");

    [HttpGet(TransferSummaryEndpoints.MovedWithinThisOrigin)]
    public IActionResult SummaryMovedWithinThisOrigin() => Redirect(TransferSummaryEndpoints.Summary);

    [HttpGet(TransferSummaryEndpoints.Summary)]
    public IActionResult Summary() => Json(new TransferSummary("Ada Hart", "North wing"));

    private string TheOtherNameOfThisHost()
    {
        var otherName = Request.Host.Host == Localhost ? Loopback : Localhost;
        return Request.Host.Port is { } port ? $"{otherName}:{port}" : otherName;
    }
}

public sealed record TransferSummaryPage(string Resident, string Summary);

public sealed record TransferSummary(string Resident, string Wing);

public static class TransferSummaries
{
    public static readonly AppEvent Load = AppEvent.Declare();
}

public static class TransferSummaryEndpoints
{
    private const string Root = CarePages.Area + "residents/transfer-summary/";

    public const string MovedToAnotherOrigin = Root + "summary/moved-to-another-origin";

    public const string MovedWithinThisOrigin = Root + "summary/moved-within-this-origin";

    public const string Summary = Root + "summary";
}
