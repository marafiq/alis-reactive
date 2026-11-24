using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class NightCoverController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.NightCover)]
    public IActionResult Night() => View("/Care/Admission/NightCover.cshtml", new NightCoverShift());

    [HttpGet(CarePages.QuickCover)]
    public IActionResult Quick() => View("/Care/Admission/QuickCover.cshtml", new QuickCoverShift());

    [HttpPost(NightCoverEndpoints.NightCovers)]
    public IActionResult SaveNight([FromForm] NightCoverShift shift) => Content($"{shift.Resident}: {shift.Cover}", PlainText);

    [HttpPost(NightCoverEndpoints.QuickCovers)]
    public IActionResult SaveQuick([FromForm] QuickCoverShift shift) => Content($"{shift.Resident}: {shift.Cover}", PlainText);
}

[JsonConverter(typeof(JsonStringEnumConverter<NightCover>))]
public enum NightCover
{
    [JsonStringEnumMemberName("one-carer")]
    OneCarer,

    [JsonStringEnumMemberName("two-carers")]
    TwoCarers,
}

public sealed class NightCoverShift
{
    public string? Resident { get; set; }

    public NightCover Cover { get; set; }
}

public sealed class QuickCoverShift
{
    public string? Resident { get; set; }

    public NightCover Cover { get; set; } = NightCover.TwoCarers;
}

public sealed class NightCoverRules : ReactiveValidator<NightCoverShift>
{
    public NightCoverRules() => RuleFor(shift => shift.Cover).IsInEnum();
}

public static class NightCoverLists
{
    public static IReadOnlyList<SelectListItem> Covers() => [new SelectListItem("One carer", "one-carer"), new SelectListItem("Two carers", "two-carers"), new SelectListItem("Agency cover", "agency")];
}

public static class NightCoverEndpoints
{
    public const string NightCovers = CarePages.NightCover + "/covers";

    public const string QuickCovers = CarePages.QuickCover + "/covers";
}
