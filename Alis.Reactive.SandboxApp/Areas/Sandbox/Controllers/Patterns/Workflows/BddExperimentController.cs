using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Patterns.Workflows
{
    [Area("Sandbox")]
    [Route("Sandbox/Patterns/BddExperiment")]
    public class BddExperimentController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() => View("~/Areas/Sandbox/Views/Patterns/BddExperiment/Index.cshtml");

        [HttpPost("Submit")]
        public IActionResult Submit([FromBody] object? body)
            => Ok(new BddExperimentResponse { Success = true, Message = "Resident admitted" });
    }
}
