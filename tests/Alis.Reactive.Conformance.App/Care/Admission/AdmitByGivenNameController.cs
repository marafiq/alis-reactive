using System.Text.Json;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AdmitByGivenNameController : Controller
{
    [HttpGet(CarePages.AdmitByGivenName)]
    public IActionResult OfAResident() => View("/Care/Admission/AdmitByGivenName.cshtml", new AdmissionByName());

    [HttpPost(AdmitByGivenNameEndpoints.Admissions)]
    public IActionResult Admit([FromBody] JsonElement body)
    {
        var appsOptions = HttpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions;
        var admission = body.Deserialize<AdmissionByName>(appsOptions)!;
        return Json(AdmissionNamesSent.Of(body.EnumerateObject().Select(member => member.Name), admission));
    }
}
