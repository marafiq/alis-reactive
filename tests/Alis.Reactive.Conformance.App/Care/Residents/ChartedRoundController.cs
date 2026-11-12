using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ChartedRoundController : Controller
{
    [HttpGet(CarePages.ChartedRoundWithTwoDosesDue)]
    public IActionResult WithTwoDosesDue() => View("/Care/Residents/ChartedRound.cshtml", new ChartedRound("Morning", ChartedRoundEndpoints.Doses));

    [HttpGet(CarePages.ChartedRoundWithTwoDosesHeld)]
    public IActionResult WithTwoDosesHeld() => View("/Care/Residents/ChartedRound.cshtml", new ChartedRound("Evening", ChartedRoundEndpoints.DosesWithTwoHeld));

    [HttpGet(CarePages.GivenChartsCounted)]
    public IActionResult GivenChartsCounted() => View("/Care/Residents/GivenCharts.cshtml", new ChartedRound("Morning", ChartedRoundEndpoints.Doses));

    [HttpGet(CarePages.VialsSummedThroughASelect)]
    public IActionResult VialsSummedThroughASelect() => View("/Care/Residents/VialsSummed.cshtml", new ChartedRound("Morning", ChartedRoundEndpoints.Doses));

    [HttpGet(ChartedRoundEndpoints.Doses)]
    public IActionResult Doses() => Json(TheDoses(RoundChartStatus.Due));

    [HttpGet(ChartedRoundEndpoints.DosesWithTwoHeld)]
    public IActionResult DosesWithTwoHeld() => Json(TheDoses(RoundChartStatus.Held));

    private static List<RoundDose> TheDoses(RoundChartStatus theLastTwo) => new List<RoundDose>
    {
        new() { Medicine = "Aspirin", Charted = RoundChartStatus.Given, Supply = new RoundSupply { Vials = 2 } },
        new() { Medicine = "Digoxin", Charted = RoundChartStatus.Given, Supply = new RoundSupply { Vials = 5 } },
        new() { Medicine = "Insulin", Charted = RoundChartStatus.Given, Supply = new RoundSupply { Vials = 10 } },
        new() { Medicine = "Warfarin", Charted = theLastTwo, Supply = new RoundSupply { Vials = 1 } },
        new() { Medicine = "Morphine", Charted = theLastTwo, Supply = new RoundSupply { Vials = 3 } },
    };
}

public sealed record ChartedRound(string Round, string Doses);

public sealed class RoundDose
{
    public string Medicine { get; init; } = "";

    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public RoundChartStatus Charted { get; init; }

    public RoundSupply Supply { get; init; } = new();
}

[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
public sealed class RoundSupply
{
    public int Vials { get; init; }
}

public enum RoundChartStatus
{
    Due,
    Given,
    Held,
}

public static class ChartedRoundEndpoints
{
    private const string Root = CarePages.Area + "residents/charted-round/";

    public const string Doses = Root + "doses";

    public const string DosesWithTwoHeld = Root + "doses/two-held";
}
