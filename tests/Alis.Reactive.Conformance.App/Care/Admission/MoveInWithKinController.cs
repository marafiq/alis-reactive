using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class MoveInWithKinController : Controller
{
    private const string PlainText = "text/plain";
    private const string LivesHere = "Ada Lee";

    [HttpGet(CarePages.MoveInWithKin)]
    public IActionResult OfAResident() => View("/Care/Admission/MoveInWithKin.cshtml", new MoveInWithKin());

    [HttpPost(MoveInWithKinEndpoints.MoveIns)]
    public IActionResult Save([FromForm] MoveInWithKin moveIn)
    {
        if (moveIn.Kin.Name != LivesHere) return Content("Saved", PlainText);
        ModelState.AddModelError("Kin.Name", "Ada Lee lives here; name another next of kin.");
        return ValidationProblem(ModelState);
    }
}

public sealed class MoveInWithKin
{
    public string? Name { get; set; }

    public MoveInKin Kin { get; set; } = new();
}

public sealed class MoveInKin
{
    public string? Name { get; set; }
}

public static class MoveInWithKinEndpoints
{
    public const string MoveIns = CarePages.Area + "admission/move-in-with-kin/move-ins";
}

public static class MoveInsWithKin
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
