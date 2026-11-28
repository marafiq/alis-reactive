using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class BedsideDoseController : Controller
{
    [HttpGet(RefusalPages.BedsideDoseFallingBackToTheSelect)]
    public IActionResult FallingBackToTheSelect() => View("/Refusals/ValuesAndSources/BedsideDoseFallingBackToTheSelect.cshtml", OfAResidentWithNoChart());

    [HttpGet(RefusalPages.BedsideDoseFallingBackToGiven)]
    public IActionResult FallingBackToGiven() => View("/Refusals/ValuesAndSources/BedsideDoseFallingBackToGiven.cshtml", OfAResidentWithNoChart());

    [HttpGet(RefusalPages.BedsideDoseChosenFromTheSelect)]
    public IActionResult ChosenFromTheSelect() => View("/Refusals/ValuesAndSources/BedsideDoseChosenFromTheSelect.cshtml", OfAResidentWithAMissedDose());

    [HttpGet(RefusalPages.BedsideDoseChosenAsGiven)]
    public IActionResult ChosenAsGiven() => View("/Refusals/ValuesAndSources/BedsideDoseChosenAsGiven.cshtml", OfAResidentWithAMissedDose());

    [HttpGet(BedsideDoses.ChartOfAResidentWithNoChart)]
    public IActionResult ChartOfAResidentWithNoChart() => Json(new BedsideChartSearch { Resident = "Ben Okafor" });

    [HttpGet(BedsideDoses.ChartOfAResidentWithAMissedDose)]
    public IActionResult ChartOfAResidentWithAMissedDose() => Json(new BedsideChartSearch { Resident = "Ada Hart", Chart = new BedsideChart { Status = BedsideDoseStatus.Missed } });

    private static BedsideDose OfAResidentWithNoChart() => new() { Resident = "Ben Okafor", Chart = BedsideDoses.ChartOfAResidentWithNoChart, Statuses = TheStatuses() };

    private static BedsideDoseCheck OfAResidentWithAMissedDose() => new() { Resident = "Ada Hart", Chart = BedsideDoses.ChartOfAResidentWithAMissedDose, Statuses = TheStatuses() };

    private static List<SelectListItem> TheStatuses() => new List<SelectListItem>
    {
        new("Due", nameof(BedsideDoseStatus.Due)),
        new("Given", nameof(BedsideDoseStatus.Given)),
        new("Missed", nameof(BedsideDoseStatus.Missed)),
    };
}

public sealed class BedsideDose
{
    public string Resident { get; init; } = "";

    public string Chart { get; init; } = "";

    public BedsideDoseStatus Status { get; set; }

    public IReadOnlyList<SelectListItem> Statuses { get; init; } = [];
}

public sealed class BedsideDoseCheck
{
    public string Resident { get; init; } = "";

    public string Chart { get; init; } = "";

    public BedsideDoseStatus Status { get; set; }

    public bool SeenAtTheBedside { get; set; }

    public IReadOnlyList<SelectListItem> Statuses { get; init; } = [];
}

public sealed class BedsideChartSearch
{
    public string Resident { get; init; } = "";

    public BedsideChart? Chart { get; init; }
}

public sealed class BedsideChart
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BedsideDoseStatus Status { get; init; }
}

public enum BedsideDoseStatus
{
    Due,

    Given,

    Missed,
}

public static class BedsideDoses
{
    private const string Root = RefusalPages.Area + "values-and-sources/bedside-dose/";

    public const string ChartOfAResidentWithNoChart = Root + "chart/none";

    public const string ChartOfAResidentWithAMissedDose = Root + "chart/missed";

    public static readonly AppEvent CheckTheDose = AppEvent.Declare();
}
