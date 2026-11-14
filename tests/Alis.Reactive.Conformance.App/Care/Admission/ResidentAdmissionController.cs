using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ResidentAdmissionController : Controller
{
    [HttpGet(CarePages.ResidentAdmission)]
    public IActionResult OfAResident() => View("/Care/Admission/ResidentAdmission.cshtml", new ResidentAdmission());
}

public sealed class ResidentAdmission
{
    public string? Name { get; set; }
}

public sealed class ResidentAdmissionRules : ReactiveValidator<ResidentAdmission>
{
    public ResidentAdmissionRules() => RuleFor(admission => admission.Name).NotEmpty().WithMessage("Name is required.");
}
