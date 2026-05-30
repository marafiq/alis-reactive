using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionDropDownButton")]
    public sealed class DropDownButtonController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/DropDownButton/Index.cshtml", new FusionDropDownButtonModel());
        }

        [HttpPost("Echo")]
        public IActionResult Echo([FromBody] FusionDropDownButtonEchoRequest request)
        {
            return Ok(new FusionDropDownButtonEchoResponse
            {
                Content = request.Content,
                Disabled = request.Disabled,
                CssClass = request.CssClass,
                Summary = request.Content + ":" + request.Disabled + ":" + request.CssClass
            });
        }
    }
}
