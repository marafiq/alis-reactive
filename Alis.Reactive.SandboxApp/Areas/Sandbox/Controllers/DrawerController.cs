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
    }
}
