using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class NativeTextAreaController : Controller
    {
        public IActionResult Index()
        {
            return View(new NativeTextAreaModel { CareNotes = "Patient admitted. Initial assessment completed." });
        }
    }
}
