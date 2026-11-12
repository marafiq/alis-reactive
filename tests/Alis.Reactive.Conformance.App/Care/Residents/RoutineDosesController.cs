using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoutineDosesController : Controller
{
    [HttpGet(CarePages.RoutineDosesWithAnUnprioritisedDose)]
    public IActionResult WithAnUnprioritisedDose() => View("/Care/Residents/RoutineDoses.cshtml", new RoutineDoses(2, RoutineDosesEndpoints.Doses));

    [HttpGet(CarePages.RoutineDosesWithEveryDosePrioritised)]
    public IActionResult WithEveryDosePrioritised() => View("/Care/Residents/RoutineDoses.cshtml", new RoutineDoses(2, RoutineDosesEndpoints.DosesAllPrioritised));

    [HttpGet(RoutineDosesEndpoints.Doses)]
    public IActionResult Doses() => Json(new List<RoutineDose>
    {
        new("Aspirin", 1),
        new("Digoxin", 2),
        new("Insulin", 2),
        new("Warfarin", 3),
        new("Morphine", null),
    });

    [HttpGet(RoutineDosesEndpoints.DosesAllPrioritised)]
    public IActionResult DosesAllPrioritised() => Json(new List<RoutineDose>
    {
        new("Aspirin", 1),
        new("Digoxin", 2),
        new("Insulin", 2),
        new("Warfarin", 3),
    });
}

public sealed record RoutineDoses(int? Routine, string Doses);

public sealed record RoutineDose(string Medicine, int? Priority);

public static class RoutineDosesEndpoints
{
    private const string Root = CarePages.Area + "residents/routine-doses/";

    public const string Doses = Root + "doses";

    public const string DosesAllPrioritised = Root + "doses/all-prioritised";
}
