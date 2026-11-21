using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AddressAdmissionController : Controller
{
    [HttpGet(CarePages.AddressAdmission)]
    public IActionResult OfAResident() => View("/Care/Admission/AddressAdmission.cshtml", new AddressAdmission());

    [HttpGet(AddressAdmissionEndpoints.HomeAddress)]
    public IActionResult HomeAddress() => PartialView("/Care/Admission/AddressAdmissionHome.cshtml", new AddressAdmission());

    [HttpGet(AddressAdmissionEndpoints.PostalAddress)]
    public IActionResult PostalAddress() => PartialView("/Care/Admission/AddressAdmissionPostal.cshtml", new AddressAdmission());
}

public sealed class AddressAdmission
{
    public AdmittedAddress Address { get; set; } = new();
}

public sealed class AdmittedAddress
{
    public string? Street { get; set; }
}

public sealed class AddressAdmissionRules : ReactiveValidator<AddressAdmission>
{
    public AddressAdmissionRules() => RuleFor(admission => admission.Address.Street).NotEmpty().WithMessage("Name the street.");
}

public static class AddressAdmissionEndpoints
{
    public const string HomeAddress = CarePages.AddressAdmission + "/home-address";

    public const string PostalAddress = CarePages.AddressAdmission + "/postal-address";
}

public static class AddressAdmissions
{
    public static readonly AppEvent LoadTheHomeAddress = AppEvent.Declare();

    public static readonly AppEvent LoadThePostalAddress = AppEvent.Declare();
}
