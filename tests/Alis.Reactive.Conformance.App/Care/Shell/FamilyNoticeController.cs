using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class FamilyNoticeController : Controller
{
    [HttpGet(CarePages.FamilyNotice)]
    public IActionResult OfAResident() => View("/Care/Shell/FamilyNotice.cshtml", new FamilyNotice("Ada"));
}

public sealed record FamilyNotice(string Resident);

public static class FamilyNoticeButtons
{
    public static readonly AppEvent Discharge = AppEvent.Declare();
}
