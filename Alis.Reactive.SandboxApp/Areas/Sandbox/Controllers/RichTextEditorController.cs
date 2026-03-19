using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class RichTextEditorController : Controller
    {
        public IActionResult Index()
        {
            return View(new RichTextEditorModel
            {
                CarePlan = "<p>Resident requires daily medication review and weekly physical therapy sessions.</p>"
            });
        }
    }
}
