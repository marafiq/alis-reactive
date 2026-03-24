using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.AllModulesTogether.Workflows
{
    [Area("Sandbox")]
    [Route("Sandbox/AllModulesTogether/BddExperiment")]
    public class BddExperimentController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index() => View("~/Areas/Sandbox/Views/AllModulesTogether/BddExperiment/Index.cshtml");

        [HttpPost("Submit")]
        public IActionResult Submit([FromBody] object? body)
            => Ok(new BddExperimentResponse { Success = true, Message = "Resident admitted" });
    }
}
