using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionListView")]
    public sealed class ListViewController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            ViewBag.Residents = new[] { "Alice", "Bennett", "Carey", "Dawson" };
            ViewBag.Tasks = new[] { "Hydration", "Mobility", "Dining" };
            return View("~/Areas/Sandbox/Views/Components/Fusion/ListView/Index.cshtml", new FusionListViewModel());
        }
    }
}
