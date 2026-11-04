using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.HttpPipeline.FamilyCall;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.HttpPipeline;

[Area("Sandbox")]
[Route("Sandbox/HttpPipeline/FamilyCall")]
public class FamilyCallController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
        => View("~/Areas/Sandbox/Views/HttpPipeline/FamilyCall/Index.cshtml", new FamilyCallModel());

    [HttpGet("EmergencyContact")]
    public IActionResult EmergencyContact()
        => Ok(new FamilyPortalAnswer(
            new PortalResident("Walter Cho", "118", new PortalContact("Grace Cho", "daughter", "(617) 555-0142"))));
}
