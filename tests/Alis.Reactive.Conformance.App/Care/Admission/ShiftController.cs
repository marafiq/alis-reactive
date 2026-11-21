using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ShiftController : Controller
{
    [HttpGet(CarePages.Shift)]
    public IActionResult OfTheDay(ShiftTimes times) => View("/Care/Admission/Shift.cshtml", times);
}

public sealed class ShiftTimes
{
    public TimeOnly? Starts { get; set; }
}
