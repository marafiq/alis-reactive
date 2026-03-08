using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class EventsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
