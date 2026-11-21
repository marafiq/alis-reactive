using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class PanelWeekController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.PanelWeekOfMondayAndTheWeekend)]
    public IActionResult OfMondayAndTheWeekend() => View("/Care/Record/PanelWeek.cshtml", new PanelWeek("Ada", PanelWeekEndpoints.PlanOfMondayAndTheWeekend));

    [HttpGet(CarePages.PanelWeekOfMonday)]
    public IActionResult OfMonday() => View("/Care/Record/PanelWeek.cshtml", new PanelWeek("Ada", PanelWeekEndpoints.PlanOfMonday));

    [HttpGet(CarePages.NightPanelOfMondayAndTheWeekend)]
    public IActionResult NightOfMondayAndTheWeekend() => View("/Care/Record/NightPanel.cshtml", new NightPanel("Ada", Components.CareDays.Mon | Components.CareDays.Sat | Components.CareDays.Sun));

    [HttpGet(CarePages.NightPanelOfMonday)]
    public IActionResult NightOfMonday() => View("/Care/Record/NightPanel.cshtml", new NightPanel("Ada", Components.CareDays.Mon));

    [HttpGet(CarePages.PlannedCareOfSaturday)]
    public IActionResult PlannedOfSaturday() => View("/Care/Record/PlannedCare.cshtml", new PlannedCare("Ada", PanelWeekEndpoints.PlanOfSaturday));

    [HttpGet(PanelWeekEndpoints.PlanOfMondayAndTheWeekend)]
    public IActionResult PlanOfMondayAndTheWeekend() => Json(new PanelWeekPlan("Ada", Components.CareDays.Mon | Components.CareDays.Sat | Components.CareDays.Sun));

    [HttpGet(PanelWeekEndpoints.PlanOfMonday)]
    public IActionResult PlanOfMonday() => Json(new PanelWeekPlan("Ada", Components.CareDays.Mon));

    [HttpGet(PanelWeekEndpoints.PlanOfSaturday)]
    public IActionResult PlanOfSaturday() => Json(new PanelWeekPlan("Ada", Components.CareDays.Sat));

    [HttpGet(PanelWeekEndpoints.Care)]
    public IActionResult Care(PanelWeekQuery query) => Content(query.Care.ToString(), PlainText);
}

public sealed record PanelWeek(string Resident, string Plan);

public sealed record NightPanel(string Resident, Components.CareDays Care);

public sealed record PlannedCare(string Resident, string Plan);

public sealed record PanelWeekPlan(string Resident, Components.CareDays Care);

public sealed class PanelWeekQuery
{
    public Components.CareDays Care { get; set; }
}

public static class PanelWeeks
{
    public static readonly AppEvent Load = AppEvent.Declare();

    public static readonly AppEvent Copy = AppEvent.Declare();

    public static readonly AppEvent Send = AppEvent.Declare();
}

public static class PanelWeekEndpoints
{
    private const string Root = CarePages.Area + "record/panel-week/";

    public const string PlanOfMondayAndTheWeekend = Root + "plan/monday-and-the-weekend";

    public const string PlanOfMonday = Root + "plan/monday";

    public const string PlanOfSaturday = Root + "plan/saturday";

    public const string Care = Root + "care";
}
