using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DischargeDeskController : Controller
{
    [HttpGet(CarePages.DischargeDesk)]
    public IActionResult OfAResident() => View("/Care/Shell/DischargeDesk.cshtml", new DischargeDesk("Ada"));
}

public sealed record DischargeDesk(string Resident);

public static class DischargeDeskButtons
{
    public static readonly AppEvent Discharge = AppEvent.Declare();

    public static readonly AppEvent AddNote = AppEvent.Declare();
}
