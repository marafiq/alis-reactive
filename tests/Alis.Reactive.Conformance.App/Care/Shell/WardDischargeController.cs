using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardDischargeController : Controller
{
    [HttpGet(CarePages.WardDischarge)]
    public IActionResult OfAResident() => View("/Care/Shell/WardDischarge.cshtml", new WardDischarge("Ada"));
}

public sealed record WardDischarge(string Resident);

public static class WardDischargeButtons
{
    public static readonly AppEvent Discharge = AppEvent.Declare();
}
