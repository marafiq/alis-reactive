using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DoseLinesController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.DoseLinesOfAMissedDose)]
    public IActionResult OfAMissedDose() => View("/Care/Round/DoseLines.cshtml", new DoseLinesDesk { Resident = "Ada Hart", Chart = DoseLines.MissedDoseChart });

    [HttpGet(CarePages.DoseLinesOfALateMissedDose)]
    public IActionResult OfALateMissedDose() => View("/Care/Round/DoseLines.cshtml", new DoseLinesDesk { Resident = "Cleo Banks", Chart = DoseLines.LateMissedDoseChart });

    [HttpGet(CarePages.DoseLinesOfASkippedDose)]
    public IActionResult OfASkippedDose() => View("/Care/Round/DoseLines.cshtml", new DoseLinesDesk { Resident = "Ben Okafor", Chart = DoseLines.SkippedDoseChart });

    [HttpGet(DoseLines.MissedDoseChart)]
    public IActionResult MissedDoseChart() => Json(new DoseLinesReply { Chart = new LateChart { Status = ChartDoseStatus.Missed, Late = false } });

    [HttpGet(DoseLines.LateMissedDoseChart)]
    public IActionResult LateMissedDoseChart() => Json(new DoseLinesReply { Chart = new LateChart { Status = ChartDoseStatus.Missed, Late = true } });

    [HttpGet(DoseLines.SkippedDoseChart)]
    public IActionResult SkippedDoseChart() => Content("{\"chart\":{\"status\":\"Skipped\",\"late\":false}}", "application/json");

    [HttpPost(DoseLines.Bound)]
    public IActionResult Bound([FromBody] DoseLinesSent sent) => Content("Bound " + sent.Doses[0].Status + " " + sent.Doses[0].Note, PlainText);
}

public sealed class DoseLinesDesk
{
    public string Resident { get; init; } = "";

    public string Chart { get; init; } = "";
}

public sealed class DoseLinesReply
{
    public LateChart? Chart { get; init; }
}

public sealed class LateChart
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ChartDoseStatus Status { get; init; }

    public bool Late { get; init; }
}

public sealed class DoseLinesSent
{
    public List<DoseLine> Doses { get; init; } = [];
}

public sealed class DoseLine
{
    public ChartDoseStatus Status { get; init; }

    public string Note { get; init; } = "unnoted";
}

public static class DoseLines
{
    private const string Root = CarePages.Area + "round/dose-lines/";

    public const string MissedDoseChart = Root + "chart/missed";

    public const string LateMissedDoseChart = Root + "chart/late-missed";

    public const string SkippedDoseChart = Root + "chart/skipped";

    public const string Bound = Root + "bound";

    public static readonly AppEvent Send = AppEvent.Declare();
}
