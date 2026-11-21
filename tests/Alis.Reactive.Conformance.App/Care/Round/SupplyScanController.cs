using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class SupplyScanController : Controller
{
    private const string Scan = "/Care/Round/SupplyScan.cshtml";

    [HttpGet(CarePages.SupplyScanOfFiveVials)]
    public IActionResult OfFiveVials() => View(Scan, new SupplyScan(SupplyScanEndpoints.FiveVials));

    [HttpGet(CarePages.SupplyScanOfAnEmptyBox)]
    public IActionResult OfAnEmptyBox() => View(Scan, new SupplyScan(SupplyScanEndpoints.EmptyBox));

    [HttpGet(SupplyScanEndpoints.FiveVials)]
    public IActionResult FiveVials() => Json(new FoundSupply { Name = "Saline", VialCount = 5m });

    [HttpGet(SupplyScanEndpoints.EmptyBox)]
    public IActionResult EmptyBox() => Json(new FoundSupply { Name = "Saline", VialCount = 0m });
}

public sealed record SupplyScan(string Supply);

public sealed class FoundSupply
{
    public string Name { get; init; } = "";

    public decimal VialCount { get; init; }
}

public static class SupplyScanEndpoints
{
    private const string Root = CarePages.Area + "round/supply-scan/supply/";
    public const string FiveVials = Root + "five-vials";
    public const string EmptyBox = Root + "empty-box";
}

public static class SupplyScans
{
    public static readonly AppEvent FindTheSupply = AppEvent.Declare();
    public static readonly AppEvent Add = AppEvent.Declare();
}
