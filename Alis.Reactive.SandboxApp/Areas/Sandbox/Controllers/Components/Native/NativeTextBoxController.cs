using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.TextBox;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Native
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/NativeTextBox")]
    public class NativeTextBoxController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Native/NativeTextBox/Index.cshtml", new NativeTextBoxModel());
        }
    }
}
