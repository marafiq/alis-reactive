using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.HttpPipeline.FamilyCall;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.HttpPipeline;

/// <summary>
/// The family portal for Walter Cho, room 118: his daughter, Grace Cho, is his emergency contact. The
/// resident and the contact are named differently, so a read of the wrong level of the answer shows the
/// wrong name.
/// </summary>
[Area("Sandbox")]
[Route("Sandbox/HttpPipeline/FamilyCall")]
public class FamilyCallController : Controller
{
    [HttpGet("")]
    public IActionResult Index() =>
        View("~/Areas/Sandbox/Views/HttpPipeline/FamilyCall/Index.cshtml", new FamilyCallModel());

    [HttpGet("EmergencyContact")]
    public IActionResult EmergencyContact() =>
        Ok(new FamilyPortalAnswer(
            new PortalResident("Walter Cho", "118", new PortalContact("Grace Cho", "daughter", "(617) 555-0142"))));
}
