using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class NoLongerDueDosesController : Controller
{
    [HttpGet(RefusalPages.DosesNoLongerDue)]
    public IActionResult NoLongerDue() => View("/Refusals/ArrayLambdas/DosesNoLongerDue.cshtml", new NoLongerDueRound("Morning"));

    [HttpGet(NoLongerDueDoseEndpoints.Doses)]
    public IActionResult Doses() => Json(new List<NoLongerDueDose>
    {
        new("Aspirin", DoseStatus.Due),
        new("Digoxin", DoseStatus.Given),
        new("Insulin", DoseStatus.Held),
        new("Warfarin", DoseStatus.Due),
    });
}

public sealed record NoLongerDueRound(string Name);

public sealed record NoLongerDueDose(string Medicine, DoseStatus Status);

public static class NoLongerDueDoseEndpoints
{
    public const string Doses = RefusalPages.DosesNoLongerDue + "/doses";
}
