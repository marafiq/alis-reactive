using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CarePlanFilingController : Controller
{
    [HttpGet(CarePages.CarePlanFiling)]
    public IActionResult OfAResident() => View("/Care/AdmissionClinical/CarePlanFiling.cshtml", new CarePlanFiling());

    [HttpPost(CarePages.CarePlanFiling)]
    public IActionResult Save([FromForm] CarePlanFiling filing) => View("/Care/AdmissionClinical/CarePlanFiled.cshtml", new CarePlanFiled(FileBound(filing.CarePlan)));

    private static string FileBound(IFormFile? carePlan) => carePlan is null ? "No care plan filed" : $"Care plan filed: {carePlan.FileName}";
}

public sealed class CarePlanFiling
{
    public IFormFile? CarePlan { get; set; }
}

public sealed record CarePlanFiled(string FileBound);
