using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class MedicationReviewController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.MedicationReviewUnderSkip)]
    public IActionResult UnderSkip() => View("/Care/Round/MedicationReview.cshtml", new MedicationReview("Ada Hart", HandlerPolicy.Skip));

    [HttpGet(CarePages.MedicationReviewUnderReplace)]
    public IActionResult UnderReplace() => View("/Care/Round/MedicationReview.cshtml", new MedicationReview("Ada Hart", HandlerPolicy.Replace));

    [HttpPost(MedicationReviewEndpoints.Reviews)]
    public async Task<IActionResult> Review()
    {
        await World.Reach(GateName.Of(LanesGate.Respond), Request.Path);
        return Content("Reviewed", PlainText);
    }

    private WorldData World => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;
}

public sealed record MedicationReview(string Resident, HandlerPolicy Policy);

public static class MedicationReviewEndpoints
{
    public const string Reviews = CarePages.Area + "round/medication-review/reviews";
}
