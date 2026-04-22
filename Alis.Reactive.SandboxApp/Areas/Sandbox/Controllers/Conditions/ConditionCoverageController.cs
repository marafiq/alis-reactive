using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions;

/// <summary>
/// Hosts the Condition Coverage sandbox page — a vertical slice that exercises
/// every component-read condition operator across every shape kind.
/// </summary>
[Area("Sandbox")]
[Route("Sandbox/Conditions/ConditionCoverage")]
public class ConditionCoverageController : Controller
{
    /// <summary>Renders the coverage page with default values pre-populated via DomReady.</summary>
    [HttpGet("")]
    public IActionResult Index() => View(
        "~/Areas/Sandbox/Views/Conditions/ConditionCoverage/Index.cshtml",
        new ConditionCoverageModel());
}
