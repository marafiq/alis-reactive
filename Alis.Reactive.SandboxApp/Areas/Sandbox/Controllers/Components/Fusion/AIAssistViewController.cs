using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/AIAssistView")]
    public class AIAssistViewController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/AIAssistView/Index.cshtml",
                new AIAssistViewModel());
        }

        [HttpGet("Reply")]
        public IActionResult Reply([FromQuery] AIAssistReplyQuery query)
        {
            return Ok(new AIAssistReply
            {
                Answer = $"Here is what the care team recorded about: {query.Prompt}."
            });
        }
    }
}
