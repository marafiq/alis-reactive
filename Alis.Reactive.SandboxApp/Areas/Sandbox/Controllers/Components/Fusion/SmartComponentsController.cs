using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/SmartComponents")]
    public class SmartComponentsController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/SmartComponents/Index.cshtml",
                new SmartComponentsModel());
        }

        [HttpPost("~/api/smart-components/suggest")]
        public IActionResult Suggest()
        {
            return Content("OK:[ hydrating well]", "text/plain");
        }

        [HttpPost("~/api/smart-components/paste")]
        public IActionResult Paste()
        {
            return Content(
                "FIELD Resident^^^Nora\nFIELD Room^^^212B\nFIELD Notes^^^Hydration rounds complete",
                "text/plain");
        }
    }
}
