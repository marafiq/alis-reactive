using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class IntakeBedController : Controller
{
    [HttpGet(CarePages.IntakeBed)]
    public IActionResult ForAResident() => View("/Care/Admission/IntakeBed.cshtml", new IntakeBed { AssignedBed = "A-3" });
}

public sealed class IntakeBed
{
    public string? AssignedBed { get; set; }
}

public static class IntakeBeds
{
    public static readonly AppEvent AssignB12 = AppEvent.Declare();
}
