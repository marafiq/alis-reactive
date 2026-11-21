using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AllergySeveritiesController : Controller
{
    [HttpGet(CarePages.AllergySeverities)]
    public IActionResult OfHelensAllergies() => View("/Care/AdmissionClinical/AllergySeverities.cshtml", new AllergySeverities([new("Penicillin", 7, "severe"), new("Latex", 9, "mild")]));

    [HttpGet(CarePages.AllergySeveritiesOfAModeratePenicillin)]
    public IActionResult OfAModeratePenicillin() => View("/Care/AdmissionClinical/AllergySeverities.cshtml", new AllergySeverities([new("Penicillin", 7, "moderate"), new("Latex", 9, "mild")]));
}

public sealed class AllergySeverities(IReadOnlyList<GradedAllergy> offered)
{
    public IReadOnlyList<GradedAllergy> Offered { get; } = offered;

    public int[]? AllergyCodes { get; set; }
}

public sealed record GradedAllergy(string Name, int Code, string Severity);
