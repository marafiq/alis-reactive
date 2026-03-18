using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class DrawerController : Controller
    {
        public IActionResult Index()
        {
            return View(new Models.DrawerModel());
        }

        [HttpGet]
        public IActionResult ResidentDetails()
        {
            return PartialView("_ResidentDetailsPartial");
        }

        [HttpGet]
        public IActionResult CarePlanNotes()
        {
            return PartialView("_CarePlanNotesPartial");
        }

        [HttpGet]
        public IActionResult AddResidentForm()
        {
            return PartialView("_AddResidentFormPartial");
        }
    }
}
