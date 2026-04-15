using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Patterns
{
    [Area("Sandbox")]
    [Route("Sandbox/Patterns")]
    public class PatternsController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() => View("~/Areas/Sandbox/Views/Patterns/Index.cshtml");
    }
}
