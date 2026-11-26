using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ResidentDetailsController : Controller
{
    [HttpGet(CarePages.ResidentDetails)]
    public IActionResult OfAResident() => View("/Care/Admission/ResidentDetails.cshtml", new NewResidentDetails());
}
