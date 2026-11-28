using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardPharmacyAdmissionController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.WardPharmacyAdmission)]
    public IActionResult OfAResident() => View("/Care/Admission/WardPharmacyAdmission.cshtml", new WardAdmission());

    [HttpGet(CarePages.WardPharmacyAdmissionInItsContract)]
    public IActionResult InItsContract() => View("/Care/Admission/WardPharmacyAdmissionInItsContract.cshtml", new WardAdmissionInItsContract());

    [HttpPost(WardPharmacyAdmissionEndpoints.Admissions)]
    public IActionResult Admit([FromBody] WardAdmission admission) => Content("admitted", PlainText);

    [HttpPost(WardPharmacyAdmissionEndpoints.AdmissionsInTheirContract)]
    public IActionResult AdmitInItsContract([FromBody] WardAdmissionInItsContract admission) => Content("admitted", PlainText);
}

public sealed class WardAdmission
{
    public string? GivenName { get; set; }

    [JsonIgnore]
    public string Ward { get; set; } = "Rehab";

    public string Pharmacy { get; set; } = "Central";
}

public sealed class WardAdmissionInItsContract
{
    public string? GivenName { get; set; }

    public string Ward { get; set; } = "Rehab";

    public string Pharmacy { get; set; } = "Central";
}

public sealed class WardPharmacyAdmissionRules : ReactiveValidator<WardAdmission>
{
    public WardPharmacyAdmissionRules()
    {
        RuleFor(admission => admission.Ward).Equal("Rehab").WithMessage("New admissions go to Rehab.");
        RuleFor(admission => admission.Pharmacy).Equal("Central").WithMessage("New admissions use the central pharmacy.");
    }
}

public sealed class WardPharmacyAdmissionInItsContractRules : ReactiveValidator<WardAdmissionInItsContract>
{
    public WardPharmacyAdmissionInItsContractRules()
    {
        RuleFor(admission => admission.Ward).Equal("Rehab").WithMessage("New admissions go to Rehab.");
        RuleFor(admission => admission.Pharmacy).Equal("Central").WithMessage("New admissions use the central pharmacy.");
    }
}

public static class WardPharmacyAdmissionEndpoints
{
    public const string Admissions = CarePages.WardPharmacyAdmission + "/admissions";

    public const string AdmissionsInTheirContract = CarePages.WardPharmacyAdmissionInItsContract + "/admissions";
}

public static class WardPharmacyAdmissions
{
    public static readonly AppEvent Admit = AppEvent.Declare();
}
