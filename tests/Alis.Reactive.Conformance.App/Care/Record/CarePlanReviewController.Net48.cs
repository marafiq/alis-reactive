using System.Net;
using System.Web.Mvc;
using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CarePlanReviewController : Controller
{
    private CarePlanReviewBook Book => TwinWorlds.Of(HttpContext).CarePlanReview;

    [HttpGet]
    [Address(CarePages.CarePlanReview)]
    public ActionResult OfAda() => View("/Care/Record/CarePlanReview.cshtml", new CarePlanReview("Ada Hart", Book.Reviews));

    [HttpPut]
    [Address(CarePlanReviewEndpoints.Reviewed)]
    public ActionResult Reviewed()
    {
        Book.Review();
        return new HttpStatusCodeResult(HttpStatusCode.NoContent);
    }
}
