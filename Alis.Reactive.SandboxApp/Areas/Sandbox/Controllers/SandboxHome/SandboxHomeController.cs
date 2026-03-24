using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.SandboxHome
{
    [Area("Sandbox")]
    [Route("Sandbox")]
    public class SandboxHomeController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() => View("~/Areas/Sandbox/Views/SandboxHome/Index.cshtml");
    }
}
