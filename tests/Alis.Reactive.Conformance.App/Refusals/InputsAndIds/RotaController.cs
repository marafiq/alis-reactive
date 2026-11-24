using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class RotaController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(RefusalPages.RotaCoverTicked)]
    public IActionResult CoverTicked() => View("/Refusals/InputsAndIds/RotaCoverTicked.cshtml", new Rota());

    [HttpGet(RefusalPages.RotaCoverChosen)]
    public IActionResult CoverChosen() => View("/Refusals/InputsAndIds/RotaCoverChosen.cshtml", new Rota());

    [HttpGet(RefusalPages.FloorChoiceNumbered)]
    public IActionResult FloorNumbered() => View("/Refusals/InputsAndIds/FloorChoiceNumbered.cshtml", new FloorChoice());

    [HttpGet(RefusalPages.WingChoiceNamed)]
    public IActionResult WingNamed() => View("/Refusals/InputsAndIds/WingChoiceNamed.cshtml", new WingChoice());

    [HttpPost(RefusalEndpoints.Rotas)]
    public IActionResult Save([FromForm] Rota rota) => Content($"Cover: {rota.Cover}", PlainText);

    [HttpPost(RefusalEndpoints.WingChoices)]
    public IActionResult Move([FromForm] WingChoice move) => Content($"Wing: {move.Wing}", PlainText);
}

public enum RotaCover
{
    OneCarer,
    TwoCarers,
}

public sealed class Rota
{
    public RotaCover Cover { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter<CareFloor>))]
public enum CareFloor
{
    [JsonStringEnumMemberName("1")]
    Ground,

    [JsonStringEnumMemberName("2")]
    First,

    [JsonStringEnumMemberName("3")]
    Second,
}

public sealed class FloorChoice
{
    public CareFloor Floor { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter<CareWing>))]
public enum CareWing
{
    [JsonStringEnumMemberName("east-wing")]
    East,

    [JsonStringEnumMemberName("west-wing")]
    West,
}

public sealed class WingChoice
{
    public CareWing Wing { get; set; }
}

public static class RotaLists
{
    public static IReadOnlyList<SelectListItem> Covers() => [new SelectListItem("One carer", "OneCarer"), new SelectListItem("Two carers", "TwoCarers")];
}
