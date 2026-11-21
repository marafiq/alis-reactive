using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class PanelDaysController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.PanelDaysOfMondayAndTheWeekend)]
    public IActionResult OfMondayAndTheWeekend() => View("/Care/Record/PanelDays.cshtml", new PanelDays("Ada", Components.CareDays.Mon | Components.CareDays.Sat | Components.CareDays.Sun));

    [HttpGet(CarePages.PanelDaysOfMonday)]
    public IActionResult OfMonday() => View("/Care/Record/PanelDays.cshtml", new PanelDays("Ada", Components.CareDays.Mon));

    [HttpGet(CarePages.PanelDaysHold)]
    public IActionResult Hold() => View("/Care/Record/PanelDaysHold.cshtml", new PanelDays("Ada", Components.CareDays.Mon | Components.CareDays.Sat | Components.CareDays.Sun));

    [HttpGet(PanelDaysEndpoints.Care)]
    public IActionResult Care(PanelDaysQuery query) => Content(query.Care.ToString(), PlainText);
}

public sealed record PanelDays(string Resident, Components.CareDays Care);

public sealed class PanelDaysQuery
{
    public Components.CareDays Care { get; set; }
}

public static class PanelDaysEvents
{
    public static readonly AppEvent Send = AppEvent.Declare();
}

public static class PanelDaysEndpoints
{
    public const string Care = CarePages.Area + "record/panel-days/care";
}
