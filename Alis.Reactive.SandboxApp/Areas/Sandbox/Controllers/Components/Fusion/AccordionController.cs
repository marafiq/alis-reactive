using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/Accordion")]
    public class AccordionController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Accordion/Index.cshtml",
                new AccordionModel());
        }

        [HttpGet("OverviewPartial")]
        public IActionResult OverviewPartial()
        {
            return PartialView("~/Areas/Sandbox/Views/Components/Fusion/Accordion/_OverviewPartial.cshtml");
        }

        [HttpGet("CareLevelsPartial")]
        public IActionResult CareLevelsPartial()
        {
            return PartialView("~/Areas/Sandbox/Views/Components/Fusion/Accordion/_CareLevelsPartial.cshtml");
        }

        [HttpGet("ContactPartial")]
        public IActionResult ContactPartial()
        {
            return PartialView("~/Areas/Sandbox/Views/Components/Fusion/Accordion/_ContactPartial.cshtml");
        }
    }
}
