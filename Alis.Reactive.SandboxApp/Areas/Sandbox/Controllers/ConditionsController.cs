using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class ConditionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
