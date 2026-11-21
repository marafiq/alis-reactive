using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class GuardianAllergiesController : Controller
{
    private const string Page = "/Care/AdmissionClinical/GuardianAllergies.cshtml";

    [HttpGet(CarePages.GuardianAllergiesOfLatex)]
    public IActionResult OfLatex() => View(Page, new FamilyForm { Guardian = new GuardianAllergies { AllergyCodes = [9] } });

    [HttpGet(CarePages.GuardianAllergiesUnrecorded)]
    public IActionResult Unrecorded() => View(Page, new FamilyForm());

    [HttpPost(GuardianAllergyEndpoints.Family)]
    public IActionResult Family([FromForm] FamilyForm form)
        => Content(form.Guardian.AllergyCodes is { Length: > 0 } codes ? $"Guardian allergies {string.Join(", ", codes)}" : "Guardian allergies none");
}

public sealed class FamilyForm
{
    public GuardianAllergies Guardian { get; set; } = new();
}

public sealed class GuardianAllergies
{
    public int[]? AllergyCodes { get; set; }
}

public sealed class FamilyValidator : ReactiveValidator<FamilyForm>
{
    public FamilyValidator() => RuleFor(m => m.Guardian.AllergyCodes).NotEmpty().WithMessage("Record the guardian's allergies");
}

public static class GuardianAllergyEndpoints
{
    public const string Family = CarePages.GuardianAllergiesOfLatex + "/family";
}
