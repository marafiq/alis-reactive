using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class MedicationCheckController : Controller
{
    [HttpGet(CarePages.MedicationCheckOfAResidentWithNoChart)]
    public IActionResult OfAResidentWithNoChart() => View("/Care/Round/MedicationCheck.cshtml", new MedicationCheck("Ben Okafor", MedicationCheckEndpoints.SearchThatFindsNoChart));

    [HttpGet(CarePages.MedicationCheckOfAResidentWhoseChartSaysMissed)]
    public IActionResult OfAResidentWhoseChartSaysMissed() => View("/Care/Round/MedicationCheck.cshtml", new MedicationCheck("Ada Hart", MedicationCheckEndpoints.SearchThatFindsAMissedDose));

    [HttpGet(CarePages.MedicationTestCheck)]
    public IActionResult WithATestBox() => View("/Care/Round/MedicationTestCheck.cshtml", new MedicationTestCheck("Ada Hart", MedicationCheckEndpoints.SearchThatFindsAMissedDose, false));

    [HttpGet(MedicationCheckEndpoints.SearchThatFindsNoChart)]
    public IActionResult SearchThatFindsNoChart() => Json(new ChartSearch { Resident = "Ben Okafor" });

    [HttpGet(MedicationCheckEndpoints.SearchThatFindsAMissedDose)]
    public IActionResult SearchThatFindsAMissedDose() => Json(new ChartSearch { Resident = "Ada Hart", Chart = new ResidentMedication { Status = DoseStatus.Missed } });
}

public sealed record MedicationCheck(string Resident, string Chart);

public sealed record MedicationTestCheck(string Resident, string Chart, bool Test);

public sealed class ChartSearch
{
    public string Resident { get; init; } = "";

    public ResidentMedication? Chart { get; init; }
}

public sealed class ResidentMedication
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DoseStatus Status { get; init; }
}

public static class MedicationCheckEndpoints
{
    private const string Root = CarePages.Area + "round/medication-check/";
    public const string SearchThatFindsNoChart = Root + "chart/none";
    public const string SearchThatFindsAMissedDose = Root + "chart/missed";
}

public static class MedicationChecks
{
    public static readonly AppEvent FindTheChart = AppEvent.Declare();
    public static readonly AppEvent Check = AppEvent.Declare();
}
