using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class BedAdmissionController : Controller
{
    private const string TheResident = "Ada Hart";

    [HttpGet(CarePages.BedAdmission)]
    public IActionResult WithABedRequest() => View("/Care/Admission/BedAdmission.cshtml", new BedAdmission(TheResident));

    [HttpGet(CarePages.BedAdmissionWithNoBedRequest)]
    public IActionResult WithNoBedRequest() => View("/Care/Admission/BedAdmissionWithNoBedRequest.cshtml", new BedAdmission(TheResident));

    [HttpPost(BedAdmissionEndpoints.Admissions)]
    public IActionResult Admit() => NoContent();

    [HttpPost(CareEndpoints.BedAdmissionBeds)]
    public async Task<IActionResult> AssignABed()
    {
        await World.Reach(GateName.Of(LanesGate.SettleTheTail), Request.Path);
        return NoContent();
    }

    private WorldData World => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;
}

public sealed record BedAdmission(string Resident);

public static class BedAdmissionEndpoints
{
    public const string Admissions = CarePages.Area + "admission/bed-admission/admissions";
}

public static class BedAdmissions
{
    public static readonly AppEvent Admit = AppEvent.Declare();
}
