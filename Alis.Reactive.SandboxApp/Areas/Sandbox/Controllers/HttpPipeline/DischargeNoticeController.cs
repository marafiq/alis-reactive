using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.HttpPipeline
{
    /// <summary>
    /// Discharge with a family notice. Both notice services are down: the text-message service answers
    /// 503 with no body, as a service behind a gateway often does, and the email service answers 503
    /// with its reason.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/HttpPipeline/DischargeNotice")]
    public class DischargeNoticeController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/HttpPipeline/DischargeNotice/Index.cshtml", new DischargeNoticePageModel());

        [HttpPost("Discharge/{resident}")]
        public IActionResult Discharge(string resident) =>
            ResidentName(resident) is { } name
                ? Ok(new DischargeResult($"{name} is discharged."))
                : NotFound();

        [HttpPost("NotifyFamily")]
        public IActionResult NotifyFamily([FromBody] FamilyNoticeRequest? notice) =>
            notice?.Channel == "email"
                ? StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new FamilyNoticeResult("The email service is down for maintenance."))
                : StatusCode(StatusCodes.Status503ServiceUnavailable);

        // No static lookup table: a static collection in the sandbox is shared by every browser world.
        private static string? ResidentName(string resident) => resident switch
        {
            "margaret" => "Margaret Hill",
            "arthur" => "Arthur Bell",
            _ => null,
        };
    }
}
