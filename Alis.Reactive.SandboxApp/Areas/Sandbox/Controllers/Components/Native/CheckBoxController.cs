using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.CheckBox;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Native
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/CheckBox")]
    public class CheckBoxController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Native/CheckBox/Index.cshtml", new CheckBoxModel
            {
                ReceivesMedication = true,
                AllowsVisitors = false,
                HasDietaryRestrictions = false
            });
        }
    }
}
