using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.CoreBehaviors
{
    [Area("Sandbox")]
    [Route("Sandbox/CoreBehaviors/Payload")]
    public class PayloadController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/CoreBehaviors/Payload/Index.cshtml");
        }
    }
}
