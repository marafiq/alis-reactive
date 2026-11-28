using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class OpenDaysController : Controller
{
    [HttpGet(CarePages.OpenDays)]
    public IActionResult OfAResident() => View("/Care/Admission/OpenDays.cshtml", new OpenDays("Ada Hart"));
}

public sealed record OpenDays(string Resident);

public static class OpenDayChecks
{
    public static readonly AppEvent Check = AppEvent.Declare();
}
