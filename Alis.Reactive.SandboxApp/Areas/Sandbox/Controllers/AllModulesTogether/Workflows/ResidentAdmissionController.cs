using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.AllModulesTogether.Workflows
{
    [Area("Sandbox")]
    [Route("Sandbox/AllModulesTogether/ResidentAdmission")]
    public class ResidentAdmissionController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() => View("~/Areas/Sandbox/Views/AllModulesTogether/ResidentAdmission/Index.cshtml");

        [HttpPost("Submit")]
        public IActionResult Submit([FromBody] object? body)
            => Ok(new ResidentAdmissionResponse { Success = true, Message = "Resident admitted" });
    }
}
