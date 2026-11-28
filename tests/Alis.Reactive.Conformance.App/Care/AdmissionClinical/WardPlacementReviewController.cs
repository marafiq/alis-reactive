using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardPlacementReviewController : Controller
{
    [HttpGet(CarePages.WardPlacementReview)]
    public IActionResult Review() => View("/Care/AdmissionClinical/WardPlacementReview.cshtml", new WardPlacementReview());
}

public sealed class WardPlacementReview
{
    public string? Assignment { get; set; }
    public string? Stop { get; set; }
    public string? Editable { get; set; } = "M1";
    public string? Keys { get; set; }
    public string? Display { get; set; }
    public string? LayoutWard { get; set; }
    public string? Preview { get; set; } = "M1";
    public IReadOnlyList<PlacementWard> Wards { get; } = [new("M1", "Maple", "North wing"), new("C2", "Cedar", "South wing")];
}

public sealed record PlacementWard(string Code, string Label, string Wing);

public static class PlacementReviewActions
{
    public static readonly AppEvent Apply = AppEvent.Declare();
    public static readonly AppEvent Clear = AppEvent.Declare();
    public static readonly AppEvent First = AppEvent.Declare();
    public static readonly AppEvent Last = AppEvent.Declare();
    public static readonly AppEvent ByName = AppEvent.Declare();
    public static readonly AppEvent ByCode = AppEvent.Declare();
    public static readonly AppEvent Review = AppEvent.Declare();
    public static readonly AppEvent NoWard = AppEvent.Declare();
    public static readonly AppEvent Hold = AppEvent.Declare();
    public static readonly AppEvent ReadOnly = AppEvent.Declare();
    public static readonly AppEvent Release = AppEvent.Declare();
    public static readonly AppEvent Focus = AppEvent.Declare();
    public static readonly AppEvent Blur = AppEvent.Declare();
    public static readonly AppEvent Detail = AppEvent.Declare();
    public static readonly AppEvent Concise = AppEvent.Declare();
    public static readonly AppEvent Urgent = AppEvent.Declare();
    public static readonly AppEvent Routine = AppEvent.Declare();
    public static readonly AppEvent ReadHint = AppEvent.Declare();
    public static readonly AppEvent Close = AppEvent.Declare();
    public static readonly AppEvent Keep = AppEvent.Declare();
}
