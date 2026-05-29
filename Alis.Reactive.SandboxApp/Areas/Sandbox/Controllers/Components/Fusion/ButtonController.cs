using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/Button")]
    public sealed class ButtonController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/Button/Index.cshtml", new ButtonModel());
        }

        [HttpPost("Echo")]
        public IActionResult Echo([FromBody] ButtonEchoRequest request)
        {
            return Ok(new ButtonEchoResponse
            {
                Content = request.Content,
                Disabled = request.Disabled,
                CssClass = request.CssClass,
                IsPrimary = request.IsPrimary,
                IsToggle = request.IsToggle,
                Summary = request.Content + ":" + request.Disabled + ":" + request.IsPrimary + ":" + request.IsToggle
            });
        }
    }
}
