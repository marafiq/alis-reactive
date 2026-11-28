using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class OpenBedsController : Controller
{
    [HttpGet(CarePages.OpenBeds)]
    public IActionResult OfTheHome() => View("/Care/AdmissionClinical/OpenBeds.cshtml", new OpenBeds());

    [HttpGet(OpenBedsEndpoints.Beds)]
    public IActionResult Beds() => Json(new[] { new CareBed("12A Maple", "12A"), new CareBed("14B Cedar", "14B") });
}

public sealed class OpenBeds
{
    public string? BedCode { get; set; }
}

public static class OpenBedsEndpoints
{
    public const string Beds = CarePages.OpenBeds + "/beds";
}

public static class OpenBedLists
{
    public static readonly AppEvent Load = AppEvent.Declare();
}
