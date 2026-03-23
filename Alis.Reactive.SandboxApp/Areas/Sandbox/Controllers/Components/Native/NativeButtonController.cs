using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.Button;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Native
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/NativeButton")]
    public class NativeButtonController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Native/NativeButton/Index.cshtml", new NativeButtonModel());
        }
    }
}
