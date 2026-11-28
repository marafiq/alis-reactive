using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CarePlanReviewController : Controller
{
    private CarePlanReviewBook Book => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.Owns(CarePlanReviewBook.Open);

    [HttpGet(CarePages.CarePlanReview)]
    public IActionResult OfAda() => View("/Care/Record/CarePlanReview.cshtml", new CarePlanReview("Ada Hart", Book.Reviews));

    [HttpPut(CarePlanReviewEndpoints.Reviewed)]
    public IActionResult Reviewed()
    {
        Book.Review();
        return NoContent();
    }
}

internal sealed partial class CarePlanReviewBook : IOwnedByAWorld
{
    void IOwnedByAWorld.Take(Seed seed)
    {
    }
}
