using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.AllModulesTogether.CrossVendor
{
    [Area("Sandbox")]
    [Route("Sandbox/AllModulesTogether/Architecture")]
    public class ArchitectureController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/AllModulesTogether/Architecture/Index.cshtml");
        }

        [HttpPost("Echo")]
        public IActionResult Echo([FromBody] Dictionary<string, object> data)
        {
            return Ok(data);
        }

        [HttpPost("ValidateClient")]
        public IActionResult ValidateClient() => Ok(new { message = "Client validation passed!" });
    }
}
