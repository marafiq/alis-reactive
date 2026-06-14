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

        [HttpPost("RecordCheckIn")]
        public IActionResult RecordCheckIn([FromBody] ButtonCheckInRequest request)
        {
            var priority = request.Recommended ? "as the recommended next step" : "for routine review";
            var followUp = request.FollowUp ? "with a follow-up flagged" : "with no follow-up needed";

            return Ok(new ButtonCheckInResponse
            {
                Confirmation =
                    $"Recorded \"{request.Action}\" {priority} {followUp}. Priority style: {request.Priority}."
            });
        }
    }
}
