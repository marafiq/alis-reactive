using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Toolbar;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionToolbar")]
    public class ToolbarController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Toolbar/Index.cshtml",
                new FusionToolbarModel());
        }

        [HttpPost("Confirm")]
        public IActionResult Confirm([FromBody] ResidentCommandRequest request)
        {
            return Ok(new ResidentCommandResponse
            {
                Confirmation = "Your payment of $248.50 was received. Reference: "
                    + request.CommandId + "."
            });
        }
    }
}
