using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions;

[Area("Sandbox")]
[Route("Sandbox/Conditions/SupportedSyntax")]
public class SupportedSyntaxController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        return View("~/Areas/Sandbox/Views/Conditions/SupportedSyntax/Index.cshtml", new SupportedSyntaxModel
        {
            RiskScore = 0,
            AssessmentScore = 0,
            SupervisorOverride = false,
            CareTrack = ""
        });
    }
}
