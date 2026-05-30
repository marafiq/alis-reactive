using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionProgressButton")]
    public sealed class ProgressButtonController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/ProgressButton/Index.cshtml", new FusionProgressButtonModel());
        }

        [HttpPost("Echo")]
        public IActionResult Echo([FromBody] FusionProgressButtonEchoRequest request)
        {
            return Ok(new FusionProgressButtonEchoResponse
            {
                Content = request.Content,
                Disabled = request.Disabled,
                CssClass = request.CssClass,
                ProgressEnabled = request.ProgressEnabled,
                Summary = request.Content + ":" + request.Disabled + ":" + request.CssClass + ":" + request.ProgressEnabled
            });
        }
    }
}
