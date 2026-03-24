using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/RichTextEditor")]
    public class RichTextEditorController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/RichTextEditor/Index.cshtml", new RichTextEditorModel
            {
                CarePlan = "<p>Resident requires daily medication review and weekly physical therapy sessions.</p>"
            });
        }
    }
}
