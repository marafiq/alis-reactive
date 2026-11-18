using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class BedLabelsController : Controller
{
    [HttpGet(CarePages.BedLabels)]
    public IActionResult OfTheHome()
        => View("/Care/AdmissionClinical/BedLabels.cshtml", new BedLabels { Beds = [new CareBed("12A Maple", "12A"), new CareBed("14B Cedar", "14B")] });
}

public sealed class BedLabels
{
    public string? BedCode { get; set; }

    public IReadOnlyList<CareBed> Beds { get; init; } = [];
}
