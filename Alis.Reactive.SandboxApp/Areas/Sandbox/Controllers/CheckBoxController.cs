using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class CheckBoxController : Controller
    {
        public IActionResult Index()
        {
            return View(new CheckBoxModel { IsActive = true, IsOptIn = false, ShowExtras = false });
        }
    }
}
