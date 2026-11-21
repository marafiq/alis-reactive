using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class SavedIntakeController : Controller
{
    [HttpGet(CarePages.SavedIntake)]
    public IActionResult Open() => View("/Care/Admission/SavedIntake.cshtml", SavedIntake.ForReview());

    [HttpPost(CarePages.SavedIntake)]
    public IActionResult Save([FromForm] SavedIntake intake) => Content(intake.Receipt(), "text/plain");
}
