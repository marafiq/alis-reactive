using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class NightChecksController : Controller
{
    [HttpGet(CarePages.NightChecks)]
    public IActionResult OfAResident() => View("/Care/Record/NightChecks.cshtml", new NightChecks("Ada Hart", null));
}

public sealed record NightChecks(string Resident, string? WitnessCode);

public static class NightChecksButtons
{
    public static readonly AppEvent AddACheck = AppEvent.Declare();
}
