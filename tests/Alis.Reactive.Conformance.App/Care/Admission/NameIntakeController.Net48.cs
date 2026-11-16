using System.Web.Mvc;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class NameIntakeController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet]
    [Address(CarePages.NameIntake)]
    public ActionResult OfAResident() => View("/Care/Admission/NameIntake.Net48.cshtml", new NameIntake());

    [HttpPost]
    [Address(CareEndpoints.NameIntakeNames)]
    public ActionResult Save(NameIntake intake) => Content("Saved " + intake.Name, PlainText);
}

public sealed class NameIntake
{
    public string? Name { get; set; }
}

public static class NameIntakes
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
