using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.AllModulesTogether
{
    [Area("Sandbox")]
    [Route("Sandbox/AllModulesTogether")]
    public class AllModulesTogetherController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() => View("~/Areas/Sandbox/Views/AllModulesTogether/Index.cshtml");
    }
}
