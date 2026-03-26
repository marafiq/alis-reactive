using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/Tab")]
    public class TabController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/Tab/Index.cshtml", new TabModel());
        }

        [HttpGet("ResidentsPartial")]
        public IActionResult ResidentsPartial()
        {
            return PartialView("~/Areas/Sandbox/Views/Components/Fusion/Tab/_ResidentsPartial.cshtml");
        }

        [HttpGet("StaffPartial")]
        public IActionResult StaffPartial()
        {
            return PartialView("~/Areas/Sandbox/Views/Components/Fusion/Tab/_StaffPartial.cshtml");
        }

        [HttpGet("FacilitiesPartial")]
        public IActionResult FacilitiesPartial()
        {
            return PartialView("~/Areas/Sandbox/Views/Components/Fusion/Tab/_FacilitiesPartial.cshtml");
        }
    }
}
