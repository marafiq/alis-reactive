using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/ColorPicker")]
    public class ColorPickerController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/ColorPicker/Index.cshtml", new ColorPickerModel
            {
                ThemeColor = "#3b82f6",
                AccentColor = "#10b981"
            });
        }
    }
}
