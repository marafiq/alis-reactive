using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components
{
    [Area("Sandbox")]
    [Route("Sandbox/Components")]
    public class ComponentsController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() => View("~/Areas/Sandbox/Views/Components/Index.cshtml");
    }
}
