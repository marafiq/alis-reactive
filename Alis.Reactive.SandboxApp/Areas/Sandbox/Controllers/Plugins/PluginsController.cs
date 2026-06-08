using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Plugins
{
    [Area("Sandbox")]
    [Route("Sandbox/Plugins")]
    public class PluginsController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() => View("~/Areas/Sandbox/Views/Plugins/Index.cshtml");
    }
}
