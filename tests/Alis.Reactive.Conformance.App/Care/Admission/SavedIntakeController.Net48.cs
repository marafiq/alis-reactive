using System.Web.Mvc;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class SavedIntakeController : Controller
{
    [HttpGet]
    [Address(CarePages.SavedIntake)]
    public ActionResult Open() => View("/Care/Admission/SavedIntake.cshtml", SavedIntake.ForReview());

    [HttpPost]
    [Address(CarePages.SavedIntake)]
    public ActionResult Save(SavedIntake intake) => Content(intake.Receipt(), "text/plain");
}
