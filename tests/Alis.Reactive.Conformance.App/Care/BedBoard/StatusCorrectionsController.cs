using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class StatusCorrectionsController : Controller
{
    [HttpGet(CarePages.StatusCorrections)]
    public IActionResult OfTheWing() => View("/Care/BedBoard/StatusCorrections.cshtml", new StatusCorrections(
        [new CorrectedStatus("R-101", "Ann", "Discharged"), new CorrectedStatus("R-102", "Bo", "On leave")],
        ["Admitted", "Discharged", "On leave"]));
}

public sealed record StatusCorrections(IReadOnlyList<CorrectedStatus> Residents, IReadOnlyList<string> Statuses);

public sealed record CorrectedStatus(string ResidentId, string Name, string Status);
