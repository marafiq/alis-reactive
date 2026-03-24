using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.CoreBehaviors
{
    [Area("Sandbox")]
    [Route("Sandbox/CoreBehaviors/Events")]
    public class EventsController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/CoreBehaviors/Events/Index.cshtml");
        }
    }
}
