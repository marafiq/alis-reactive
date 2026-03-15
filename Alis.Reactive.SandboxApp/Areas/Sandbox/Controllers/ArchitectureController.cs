using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class ArchitectureController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Echo([FromBody] Dictionary<string, object> data)
        {
            return Ok(data);
        }

        [HttpPost]
        public IActionResult ValidateClient() => Ok(new { message = "Client validation passed!" });
    }
}
