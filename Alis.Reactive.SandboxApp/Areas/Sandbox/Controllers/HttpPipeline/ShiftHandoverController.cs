using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.HttpPipeline
{
    [Area("Sandbox")]
    [Route("Sandbox/HttpPipeline/ShiftHandover")]
    public class ShiftHandoverController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/HttpPipeline/ShiftHandover/Index.cshtml", new ShiftHandoverPageModel());

        [HttpGet("MarkRead/{resident}")]
        public IActionResult OpenNote(string resident) =>
            ResidentName(resident) is { } name
                ? Content($"{name}'s handover note is open. Mark it read from the handover page.", "text/plain")
                : NotFound();

        [HttpPost("MarkRead/{resident}")]
        public IActionResult MarkRead(string resident, [FromHeader(Name = "X-Shift")] string? shift)
        {
            if (ResidentName(resident) is not { } name) return NotFound();
            if (string.IsNullOrWhiteSpace(shift))
                return BadRequest(new HandoverNoteRead($"{name}'s note was not marked read: no shift was named."));
            if (resident == "arthur")
                return Conflict(new HandoverNoteRead($"{name}'s note was already marked read by the day shift."));

            return Ok(new HandoverNoteRead($"{name}'s handover note is marked read by the {shift.ToLowerInvariant()} shift."));
        }

        private static string? ResidentName(string resident) => resident switch
        {
            "helen" => "Helen Park",
            "arthur" => "Arthur Lee",
            _ => null,
        };
    }
}
