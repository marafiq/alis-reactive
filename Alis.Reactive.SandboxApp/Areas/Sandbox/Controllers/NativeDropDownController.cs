using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class NativeDropDownController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.CareLevelItems = new[]
            {
                new SelectListItem("Assisted Living", "Assisted Living"),
                new SelectListItem("Memory Care", "Memory Care"),
                new SelectListItem("Independent Living", "Independent Living"),
                new SelectListItem("Skilled Nursing", "Skilled Nursing"),
            };

            ViewBag.FacilityTypeItems = new[]
            {
                new SelectListItem("Residential", "Residential"),
                new SelectListItem("Medical", "Medical"),
                new SelectListItem("Rehabilitation", "Rehabilitation"),
            };

            return View(new NativeDropDownModel());
        }
    }
}
