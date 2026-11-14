using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AdmitResidentController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.AdmitResident)]
    public IActionResult OfAResident() => View("/Care/Admission/AdmitResident.cshtml", new ResidentAdmittance());

    [HttpPost(AdmitResidentEndpoints.Admissions)]
    public IActionResult Admit([FromForm] ResidentAdmittance admittance)
    {
        var checkedAdmittance = new ResidentAdmittanceRules().Validate(admittance);
        if (checkedAdmittance.IsValid) return Content("Admitted", PlainText);
        return checkedAdmittance.ToErrorResponse();
    }
}

public sealed class ResidentAdmittance
{
    public string? Name { get; set; }

    public AdmittanceRoom Room { get; set; } = new();
}

public sealed class AdmittanceRoom
{
    public string? Number { get; set; }
}

public sealed class ResidentAdmittanceRules : ReactiveValidator<ResidentAdmittance>
{
    private const string TakenName = "Ada Lee";
    private const string FullRoom = "12";

    public ResidentAdmittanceRules()
    {
        RuleFor(admittance => admittance.Name).Must(name => name != TakenName).WithMessage("Name is taken");
        RuleFor(admittance => admittance.Room.Number).Must(number => number != FullRoom).WithMessage("Room is full");
    }
}

public static class AdmitResidentEndpoints
{
    public const string Admissions = CarePages.AdmitResident + "/admissions";
}

public static class AdmitResidentButtons
{
    public static readonly AppEvent Admit = AppEvent.Declare();
}
