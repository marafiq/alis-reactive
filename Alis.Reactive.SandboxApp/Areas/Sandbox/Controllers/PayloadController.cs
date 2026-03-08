using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class PayloadController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
