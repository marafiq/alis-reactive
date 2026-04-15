using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Patterns.ReactiveWiring
{
    [Area("Sandbox")]
    [Route("Sandbox/Patterns/PlaygroundSyntax")]
    public class PlaygroundSyntaxController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            ViewBag.StatusItems = new List<SelectListItem>
            {
                new SelectListItem("-- Select Status --", ""),
                new SelectListItem("Active", "active"),
                new SelectListItem("Inactive", "inactive"),
                new SelectListItem("Pending", "pending"),
            };

            ViewBag.CategoryItems = new List<SelectListItem>
            {
                new SelectListItem("-- Select Category --", ""),
                new SelectListItem("Category A", "A"),
                new SelectListItem("Category B", "B"),
                new SelectListItem("Category C", "C"),
            };

            ViewBag.CityItems = new List<SelectListItem>
            {
                new SelectListItem("-- Select City --", ""),
                new SelectListItem("Seattle", "seattle"),
                new SelectListItem("Portland", "portland"),
                new SelectListItem("Denver", "denver"),
            };

            ViewBag.StateItems = new List<SelectListItem>
            {
                new SelectListItem("-- Select State --", ""),
                new SelectListItem("WA", "WA"),
                new SelectListItem("OR", "OR"),
                new SelectListItem("CO", "CO"),
            };

            return View("~/Areas/Sandbox/Views/Patterns/PlaygroundSyntax/Index.cshtml", new PlaygroundSyntaxModel
            {
                Address = new PlaygroundAddress()
            });
        }

        [HttpGet("ReactiveConditions")]
        public IActionResult ReactiveConditions()
        {
            ViewBag.StatusItems = new List<SelectListItem>
            {
                new SelectListItem("-- Select Status --", ""),
                new SelectListItem("Active", "active"),
                new SelectListItem("Inactive", "inactive"),
                new SelectListItem("Pending", "pending"),
            };

            ViewBag.CityItems = new List<SelectListItem>
            {
                new SelectListItem("-- Select City --", ""),
                new SelectListItem("Seattle", "seattle"),
                new SelectListItem("Portland", "portland"),
                new SelectListItem("Denver", "denver"),
            };

            ViewBag.StateItems = new List<SelectListItem>
            {
                new SelectListItem("-- Select State --", ""),
                new SelectListItem("WA", "WA"),
                new SelectListItem("OR", "OR"),
                new SelectListItem("CO", "CO"),
            };

            return View("~/Areas/Sandbox/Views/Patterns/PlaygroundSyntax/ReactiveConditions.cshtml", new PlaygroundSyntaxModel
            {
                Address = new PlaygroundAddress()
            });
        }
    }
}
