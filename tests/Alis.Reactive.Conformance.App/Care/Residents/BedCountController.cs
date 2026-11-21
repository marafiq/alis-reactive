using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class BedCountController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.BedCountOfACensusWithNoWard)]
    public IActionResult OfACensusWithNoWard() => View("/Care/Residents/BedCount.cshtml", new BedCountForm(BedCountEndpoints.CensusWithNoWard));

    [HttpGet(CarePages.BedCountOfACensusWithAWard)]
    public IActionResult OfACensusWithAWard() => View("/Care/Residents/BedCount.cshtml", new BedCountForm(BedCountEndpoints.CensusWithAWard));

    [HttpGet(BedCountEndpoints.CensusWithNoWard)]
    public IActionResult CensusWithNoWard() => Json(new WingCensus("North wing", null));

    [HttpGet(BedCountEndpoints.CensusWithAWard)]
    public IActionResult CensusWithAWard() => Json(new WingCensus("North wing", new CensusWard(3)));

    [HttpPost(BedCountEndpoints.BedCounts)]
    public IActionResult Record([FromBody] BedCount? count)
        => Content(count is null ? "invalid" : $"Beds {count.Beds}, {(ModelState.IsValid ? "valid" : "invalid")}", PlainText);
}

public sealed record BedCountForm(string Census);

public sealed record WingCensus(string Wing, CensusWard? Ward);

public sealed record CensusWard(int Beds);

public sealed class BedCount
{
    public int Beds { get; set; } = 1;
}

public static class BedCounts
{
    public static readonly AppEvent LoadCensus = AppEvent.Declare();

    public static readonly AppEvent Send = AppEvent.Declare();
}

public static class BedCountEndpoints
{
    private const string Root = CarePages.Area + "residents/bed-count/";

    public const string CensusWithNoWard = Root + "census/no-ward";

    public const string CensusWithAWard = Root + "census/a-ward";

    public const string BedCounts = Root + "counts";
}
