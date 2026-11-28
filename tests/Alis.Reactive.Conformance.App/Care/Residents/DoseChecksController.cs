using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DoseChecksController : Controller
{
    [HttpGet(CarePages.DoseChecksOfTheRound)]
    public IActionResult OfTheRound() => View("/Care/Residents/DoseChecks.cshtml", new DoseChecks("Morning", DoseChecksEndpoints.Doses));

    [HttpGet(CarePages.DoseChecksWithBenChecking)]
    public IActionResult WithBenChecking() => View("/Care/Residents/DoseChecks.cshtml", new DoseChecks("Evening", DoseChecksEndpoints.DosesBenChecked));

    [HttpGet(DoseChecksEndpoints.Doses)]
    public IActionResult Doses() => Json(TheDoses([new DoseCheck("Cora", false)]));

    [HttpGet(DoseChecksEndpoints.DosesBenChecked)]
    public IActionResult DosesBenChecked() => Json(TheDoses([new DoseCheck("Cora", false), new DoseCheck("Ben", true)]));

    private static List<CheckedDose> TheDoses(List<DoseCheck> checksOfB) => new List<CheckedDose>
    {
        new("A", new DoseCarer("Ada"), [new DoseCheck("Ada", true), new DoseCheck("Dev", true)]),
        new("B", new DoseCarer("Ben"), checksOfB),
        new("C", null, []),
    };
}

public sealed record DoseChecks(string Round, string Doses);

public sealed record CheckedDose(string Name, DoseCarer? Carer, List<DoseCheck> Checks);

public sealed record DoseCarer(string Name);

public sealed record DoseCheck(string Name, bool Signed);

public static class DoseChecksEndpoints
{
    private const string Root = CarePages.Area + "residents/dose-checks/";

    public const string Doses = Root + "doses";

    public const string DosesBenChecked = Root + "doses/ben-checked";
}
