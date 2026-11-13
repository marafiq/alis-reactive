using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class StatusCorrectionDialogController : Controller
{
    [HttpGet(CarePages.StatusCorrectionDialog)]
    public IActionResult OfTheWing() => View("/Care/BedBoard/StatusCorrectionDialog.cshtml", new StatusCorrectionDialog(
        [new StatusUnderCorrection("R-101", "Ann", "Discharged"), new StatusUnderCorrection("R-102", "Bo", "On leave")],
        ["Admitted", "Discharged", "On leave"]));
}

public sealed record StatusCorrectionDialog(IReadOnlyList<StatusUnderCorrection> Residents, IReadOnlyList<string> Statuses);

public sealed record StatusUnderCorrection(string ResidentId, string Name, string Status);
