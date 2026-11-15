using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CareWeekController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.CareWeekOfMondayAndTheWeekend)]
    public IActionResult OfMondayAndTheWeekend() => View("/Care/Record/CareWeek.cshtml", new CareWeek("Ada", CareWeekEndpoints.PlanOfMondayAndTheWeekend));

    [HttpGet(CarePages.CareWeekOfMonday)]
    public IActionResult OfMonday() => View("/Care/Record/CareWeek.cshtml", new CareWeek("Ada", CareWeekEndpoints.PlanOfMonday));

    [HttpGet(CareWeekEndpoints.PlanOfMondayAndTheWeekend)]
    public IActionResult PlanOfMondayAndTheWeekend() => Json(new CareWeekPlan("Ada", Components.CareDays.Mon | Components.CareDays.Sat | Components.CareDays.Sun));

    [HttpGet(CareWeekEndpoints.PlanOfMonday)]
    public IActionResult PlanOfMonday() => Json(new CareWeekPlan("Ada", Components.CareDays.Mon));

    [HttpGet(CareWeekEndpoints.Care)]
    public IActionResult Care(CareWeekQuery query) => Content(query.Care.ToString(), PlainText);
}

public sealed record CareWeek(string Resident, string Plan);

public sealed record CareWeekPlan(string Resident, Components.CareDays Care);

public sealed class CareWeekQuery
{
    public Components.CareDays Care { get; set; }
}

public static class CareWeeks
{
    public static readonly AppEvent Load = AppEvent.Declare();

    public static readonly AppEvent Send = AppEvent.Declare();
}

public static class CareWeekEndpoints
{
    private const string Root = CarePages.Area + "record/care-week/";

    public const string PlanOfMondayAndTheWeekend = Root + "plan/monday-and-the-weekend";

    public const string PlanOfMonday = Root + "plan/monday";

    public const string Care = Root + "care";
}
