using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DrawerDischargeController : Controller
{
    [HttpGet(CarePages.DrawerDischarge)]
    public IActionResult OfAResident() => View("/Care/Shell/DrawerDischarge.cshtml", new DrawerDischargePage("Ada"));

    [HttpGet(DrawerDischargeEndpoints.Notes)]
    public IActionResult Notes() => PartialView("/Care/Shell/DrawerDischargeNotes.cshtml", new DrawerDischargeNotes("Ada", "Ada is ready to go home."));
}

public sealed record DrawerDischargePage(string Resident);

public sealed record DrawerDischargeNotes(string Resident, string Note);

public static class DrawerDischargeEndpoints
{
    public const string Notes = CarePages.Area + "shell/drawer-discharge/notes";
}

public static class DrawerDischargeButtons
{
    public static readonly AppEvent OpenNotes = AppEvent.Declare();

    public static readonly AppEvent Discharge = AppEvent.Declare();
}
