using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class IntakeCheckController : Controller
{
    [HttpGet(CarePages.IntakeCheck)]
    public IActionResult Index() => View("/Care/AdmissionClinical/IntakeCheck.cshtml", new IntakeForm());

    [HttpPost(IntakeCheckEndpoints.Intake)]
    public IActionResult Intake([FromForm] IntakeForm form) => Content($"Saved {string.Join(", ", form.AllergyCodes ?? [])}");
}

public sealed class IntakeForm
{
    public int[]? AllergyCodes { get; set; }
}

public sealed class IntakeValidator : ReactiveValidator<IntakeForm>
{
    public IntakeValidator() => RuleFor(m => m.AllergyCodes).NotEmpty().WithMessage("Record at least one allergy");
}

public static class IntakeCheckEndpoints
{
    public const string Intake = CarePages.IntakeCheck + "/intake";
}
