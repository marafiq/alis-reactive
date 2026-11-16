using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
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
