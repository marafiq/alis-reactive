using System.Web.Mvc;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AdmitResidentController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet]
    [Address(CarePages.AdmitResident)]
    public ActionResult OfAResident() => View("/Care/Admission/AdmitResident.cshtml", new ResidentAdmittance());

    [HttpPost]
    [Address(AdmitResidentEndpoints.Admissions)]
    public ActionResult Admit(ResidentAdmittance admittance)
    {
        var checkedAdmittance = new ResidentAdmittanceRules().Validate(admittance);
        if (checkedAdmittance.IsValid) return Content("Admitted", PlainText);
        return checkedAdmittance.ToErrorResponse();
    }
}
