using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionSplitButton")]
    public sealed class SplitButtonController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/SplitButton/Index.cshtml", new FusionSplitButtonModel());
        }

        [HttpPost("Echo")]
        public IActionResult Echo([FromBody] FusionSplitButtonEchoRequest request)
        {
            return Ok(new FusionSplitButtonEchoResponse
            {
                Content = request.Content,
                Disabled = request.Disabled,
                CssClass = request.CssClass,
                Summary = request.Content + ":" + request.Disabled + ":" + request.CssClass
            });
        }
    }
}
