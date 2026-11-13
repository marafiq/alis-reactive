using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DoseRowController : Controller
{
    [HttpGet(CarePages.DoseRow)]
    public IActionResult OfTheRound() => View("/Care/Handover/DoseRow.cshtml", new DoseRow("Ada Brooks", "Aspirin"));
}

public sealed record DoseRow(string Resident, string Medicine);
