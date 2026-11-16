using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class MedicationSignOffController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.MedicationSignOff)]
    public IActionResult SignOff() => View("/Care/Round/MedicationSignOff.cshtml", new MedicationSignOff("Ada Hart"));

    [HttpPost(MedicationSignOffEndpoints.Reviews)]
    public async Task<IActionResult> Review()
    {
        await World.Reach(GateName.Of(LanesGate.Respond), Request.Path);
        return Content("Reviewed", PlainText);
    }

    private WorldData World => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;
}

public sealed record MedicationSignOff(string Resident);

public static class MedicationSignOffEndpoints
{
    public const string Reviews = CarePages.Area + "round/medication-sign-off/reviews";
}
