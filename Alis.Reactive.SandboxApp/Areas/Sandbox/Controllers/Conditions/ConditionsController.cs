using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions
{
    [Area("Sandbox")]
    [Route("Sandbox/Conditions/Guards")]
    public class ConditionsController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Conditions/Guards/Index.cshtml");
        }
    }
}
