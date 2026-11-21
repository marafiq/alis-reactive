using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AdmissionWardController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.AdmissionWard)]
    public IActionResult OfAResident() => View("/Care/Admission/AdmissionWard.cshtml", new AdmissionWard());

    [HttpPost(CareEndpoints.AdmissionWards)]
    public IActionResult Save([FromForm] AdmissionWard admission)
    {
        var validation = this.Validate<AdmissionWard, AdmissionWardRules>(admission);
        if (!validation.IsValid) return validation.ToErrorResponse();
        return Content(admission.Ward ?? string.Empty, PlainText);
    }
}

public sealed class AdmissionWard
{
    public string? Ward { get; set; }
}

public sealed class AdmissionWardRules : ReactiveValidator<AdmissionWard>
{
    public AdmissionWardRules() => RuleFor(admission => admission.Ward).Cascade(CascadeMode.Stop)
        .MaximumLength(3).WithMessage("Keep the ward to three letters.")
        .Matches("^[A-Z]*$").WithMessage("Write the ward in capitals.");
}
