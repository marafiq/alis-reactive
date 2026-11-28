using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ChartStatusController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.ChartStatusOfAMissedDose)]
    public IActionResult OfAMissedDose() => View("/Care/Round/ChartStatus.cshtml", new ChartStatusDesk { Resident = "Ada Hart", Chart = ChartStatuses.MissedDoseChart });

    [HttpGet(CarePages.ChartStatusOfASkippedDose)]
    public IActionResult OfASkippedDose() => View("/Care/Round/ChartStatus.cshtml", new ChartStatusDesk { Resident = "Ben Okafor", Chart = ChartStatuses.SkippedDoseChart });

    [HttpGet(ChartStatuses.MissedDoseChart)]
    public IActionResult MissedDoseChart() => Json(new ChartStatusReply { Chart = new StatusChart { Status = ChartDoseStatus.Missed } });

    [HttpGet(ChartStatuses.SkippedDoseChart)]
    public IActionResult SkippedDoseChart() => Content("{\"chart\":{\"status\":\"Skipped\"}}", "application/json");

    [HttpPost(ChartStatuses.Bound)]
    public IActionResult Bound([FromBody] ChartStatusSent sent) => Content("Bound " + sent.Status, PlainText);
}

public sealed class ChartStatusDesk
{
    public string Resident { get; init; } = "";

    public string Chart { get; init; } = "";
}

public sealed class ChartStatusReply
{
    public StatusChart? Chart { get; init; }
}

public sealed class StatusChart
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ChartDoseStatus Status { get; init; }
}

public sealed class ChartStatusSent
{
    public ChartDoseStatus Status { get; init; }
}

public enum ChartDoseStatus
{
    Due,

    Given,

    Missed,
}

public static class ChartStatuses
{
    private const string Root = CarePages.Area + "round/chart-status/";

    public const string MissedDoseChart = Root + "chart/missed";

    public const string SkippedDoseChart = Root + "chart/skipped";

    public const string Bound = Root + "bound";

    public static readonly AppEvent Send = AppEvent.Declare();
}
