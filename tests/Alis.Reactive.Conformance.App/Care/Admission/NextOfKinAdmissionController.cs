using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class NextOfKinAdmissionController : Controller
{
    [HttpGet(CarePages.NextOfKinAdmission)]
    public IActionResult OfAResident() => View("/Care/Admission/NextOfKinAdmission.cshtml", new NextOfKinAdmission());
}

public sealed class NextOfKinAdmission
{
    public string? Name { get; set; }

    public string? Phone { get; set; }
}

public sealed class NextOfKinAdmissionRules : ReactiveValidator<NextOfKinAdmission>
{
    public NextOfKinAdmissionRules()
    {
        RuleFor(kin => kin.Name).MinimumLength(2).WithMessage("Write the kin's full name.");
        RuleFor(kin => kin.Phone).NotEmpty().WithMessage("Give the kin's phone number.");
    }
}
