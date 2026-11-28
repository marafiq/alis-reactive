using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DoseScanController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.DoseScan)]
    public IActionResult Scan() => View("/Care/Round/DoseScan.cshtml", new DoseScan("Ada Hart"));

    [HttpPost(DoseScanEndpoints.Reviews)]
    public IActionResult Review() => Content("Reviewed", PlainText);
}

public sealed record DoseScan(string Resident);

public static class DoseScanEndpoints
{
    public const string Reviews = CarePages.Area + "round/dose-scan/reviews";
}
