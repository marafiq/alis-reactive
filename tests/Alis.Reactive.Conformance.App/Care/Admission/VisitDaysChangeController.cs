using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class VisitDaysChangeController : Controller
{
    [HttpGet(CarePages.VisitDaysChange)]
    public IActionResult ForAResident() => View("/Care/Admission/VisitDaysChange.cshtml", new VisitDaysChange { VisitDays = ["Wed"] });
}

public sealed class VisitDaysChange
{
    public List<string> VisitDays { get; set; } = [];
}

public static class VisitDaysChanges
{
    public static readonly AppEvent ChangeDays = AppEvent.Declare();
}
