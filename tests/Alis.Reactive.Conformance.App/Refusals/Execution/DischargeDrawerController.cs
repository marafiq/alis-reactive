using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class DischargeDrawerController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(RefusalPages.DischargeDrawer)]
    public IActionResult OfAResident() => View("/Refusals/Execution/DischargeDrawer.cshtml", new DischargeDrawerPage("Ada"));

    [HttpPost(DischargeDrawerEndpoints.Discharges)]
    public IActionResult Discharge() => Content("Discharge sent", PlainText);
}

public sealed record DischargeDrawerPage(string Resident);

public static class DischargeDrawerEndpoints
{
    public const string Discharges = RefusalPages.Area + "execution/discharge-drawer/discharges";
}

public static class DischargeDrawerButtons
{
    public static readonly AppEvent OpenDischarge = AppEvent.Declare();
}
