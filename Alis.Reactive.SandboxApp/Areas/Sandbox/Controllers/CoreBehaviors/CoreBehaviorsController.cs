using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.CoreBehaviors
{
    [Area("Sandbox")]
    [Route("Sandbox/CoreBehaviors")]
    public class CoreBehaviorsController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() => View("~/Areas/Sandbox/Views/CoreBehaviors/Index.cshtml");
    }
}
