using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionCheckBox")]
    public sealed class FusionCheckBoxController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/CheckBox/Index.cshtml", new FusionCheckBoxModel
            {
                ConsentAccepted = false,
                ReviewNeeded = false
            });
        }

        [HttpPost("Echo")]
        public IActionResult Echo([FromBody] FusionCheckBoxEchoRequest request)
        {
            return Ok(new FusionCheckBoxEchoResponse
            {
                Checked = request.Checked,
                Indeterminate = request.Indeterminate,
                Disabled = request.Disabled,
                Summary = request.Checked + ":" + request.Indeterminate + ":" + request.Disabled
            });
        }
    }
}
