using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CarePlanDrawerController : Controller
{
    [HttpGet(CarePages.CarePlanDrawer)]
    public IActionResult OfAResident() => View("/Care/Shell/CarePlanDrawer.cshtml", new CarePlanEditor("Ada"));

    [HttpGet(CarePlanDrawerEndpoints.Form)]
    public IActionResult Form() => PartialView("/Care/Shell/CarePlanForm.cshtml", new CarePlanGoals());
}

public sealed record CarePlanEditor(string Resident);

public sealed class CarePlanGoals
{
    public string? Goals { get; set; }
}

public static class CarePlanDrawerEndpoints
{
    public const string Form = CarePages.Area + "shell/care-plan-drawer/form";
}

public static class CarePlanDrawerButtons
{
    public static readonly AppEvent EditCarePlan = AppEvent.Declare();
}
