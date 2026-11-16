using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class QuickAdmissionController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.QuickAdmission)]
    public IActionResult OfAResident() => View("/Care/Admission/QuickAdmission.cshtml", new QuickAdmission());

    [HttpPost(QuickAdmissionEndpoints.Admissions)]
    public IActionResult Admit([FromForm] QuickAdmission admission) => Content($"Admitted {admission.Name}", PlainText);
}

public sealed class QuickAdmission
{
    public string? Name { get; set; }
}

public sealed class QuickAdmissionRules : ReactiveValidator<QuickAdmission>
{
    public QuickAdmissionRules() => RuleFor(admission => admission.Name).NotEmpty().WithMessage("Name the resident.");
}

public static class QuickAdmissionEndpoints
{
    public const string Admissions = CarePages.QuickAdmission + "/admissions";
}

public static class QuickAdmissions
{
    public static readonly AppEvent Admit = AppEvent.Declare();
}
