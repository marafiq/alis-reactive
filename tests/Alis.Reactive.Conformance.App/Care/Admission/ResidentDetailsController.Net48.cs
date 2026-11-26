using System.Web.Mvc;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ResidentDetailsController : Controller
{
    [HttpGet]
    [Address(CarePages.ResidentDetails)]
    public ActionResult OfAResident() => View("/Care/Admission/ResidentDetails.cshtml", new NewResidentDetails());
}
