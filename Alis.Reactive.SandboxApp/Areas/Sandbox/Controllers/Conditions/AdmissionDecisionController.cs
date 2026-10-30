using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.AdmissionDecision;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions;

[Area("Sandbox")]
[Route("Sandbox/Conditions/AdmissionDecision")]
public class AdmissionDecisionController : Controller
{
    [HttpGet("")]
    public IActionResult Index() =>
        View("~/Areas/Sandbox/Views/Conditions/AdmissionDecision/Index.cshtml", new AdmissionDecisionModel());
}
