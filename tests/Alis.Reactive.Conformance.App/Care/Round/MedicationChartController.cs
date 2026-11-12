using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class MedicationChartController : Controller
{
    [HttpGet(CarePages.MedicationChartWithAspirinGiven)]
    public IActionResult WithAspirinGiven() => View("/Care/Round/MedicationChart.cshtml", new MedicationChart("Ada Hart", MedicationChartEndpoints.DosesWithAspirinGiven));

    [HttpGet(CarePages.MedicationChartWithAspirinDue)]
    public IActionResult WithAspirinDue() => View("/Care/Round/MedicationChart.cshtml", new MedicationChart("Ben Okafor", MedicationChartEndpoints.DosesWithAspirinDue));

    [HttpGet(CarePages.MedicationChartWithNoAspirin)]
    public IActionResult WithNoAspirin() => View("/Care/Round/MedicationChart.cshtml", new MedicationChart("Cy Lund", MedicationChartEndpoints.DosesWithNoAspirin));

    [HttpGet(MedicationChartEndpoints.DosesWithAspirinGiven)]
    public IActionResult DosesWithAspirinGiven() => Json(new List<ChartedMedication>
    {
        new() { Medicine = "Digoxin", Status = DoseStatus.Due },
        new() { Medicine = "Aspirin", Status = DoseStatus.Given },
    });

    [HttpGet(MedicationChartEndpoints.DosesWithAspirinDue)]
    public IActionResult DosesWithAspirinDue() => Json(new List<ChartedMedication>
    {
        new() { Medicine = "Digoxin", Status = DoseStatus.Given },
        new() { Medicine = "Aspirin", Status = DoseStatus.Due },
    });

    [HttpGet(MedicationChartEndpoints.DosesWithNoAspirin)]
    public IActionResult DosesWithNoAspirin() => Json(new List<ChartedMedication>
    {
        new() { Medicine = "Digoxin", Status = DoseStatus.Given },
    });
}

public sealed record MedicationChart(string Resident, string Doses);

public sealed class ChartedMedication
{
    public string Medicine { get; init; } = "";

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DoseStatus Status { get; init; }
}

public enum DoseStatus
{
    Due,
    Given,
    Missed,
}

public static class MedicationChartEndpoints
{
    private const string Root = CarePages.Area + "round/medication-chart/";
    public const string DosesWithAspirinGiven = Root + "doses/aspirin-given";
    public const string DosesWithAspirinDue = Root + "doses/aspirin-due";
    public const string DosesWithNoAspirin = Root + "doses/no-aspirin";
}

public static class MedicationCharts
{
    public static readonly AppEvent Check = AppEvent.Declare();
}
