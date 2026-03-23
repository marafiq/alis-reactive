using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions
{
    [Area("Sandbox")]
    [Route("Sandbox/Conditions")]
    public class ConditionsHomeController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() => View("~/Areas/Sandbox/Views/Conditions/Index.cshtml");
    }
}
