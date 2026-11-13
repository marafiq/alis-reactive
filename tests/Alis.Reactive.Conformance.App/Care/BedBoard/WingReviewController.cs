using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WingReviewController : Controller
{
    [HttpGet(CarePages.WingReview)]
    public IActionResult OfTheWing() => View("/Care/BedBoard/WingReview.cshtml", new WingReview(
        [new ReviewedResident("R-101", "Ann"), new ReviewedResident("R-102", "Bo")]));
}

public sealed record WingReview(IReadOnlyList<ReviewedResident> Residents);

public sealed record ReviewedResident(string ResidentId, string Name);

public sealed class WingReviewStart
{
    public string? ResidentId { get; init; }
}

public static class WingReviewEvents
{
    public static readonly AppEvent<WingReviewStart> StartReview = AppEvent.Declare<WingReviewStart>();
}
