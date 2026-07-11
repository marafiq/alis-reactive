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

        // The monthly-charges section detail is fetched on demand the first time the
        // resident opens that section, modelling a real care-plan page that does not
        // ship every resident's billing detail in the initial document.
        [HttpGet("MonthlyChargesDetail")]
        public IActionResult MonthlyChargesDetail()
        {
            return PartialView(
                "~/Areas/Sandbox/Views/Components/Fusion/Accordion/_MonthlyChargesPartial.cshtml");
        }
    }
}
