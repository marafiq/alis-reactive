using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.HttpPipeline
{
    /// <summary>
    /// Shift handover: a handover note is marked read by a shift, which the page names in the X-Shift
    /// header. The server refuses a note marked read by no shift, and Arthur Lee's note, which the day shift
    /// already marked read.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/HttpPipeline/ShiftHandover")]
    public class ShiftHandoverController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/HttpPipeline/ShiftHandover/Index.cshtml", new ShiftHandoverPageModel());

        // The link's own address, opened in a tab of its own (a Ctrl or Cmd click): the note, not a change.
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

        // No static lookup table: a static collection in the sandbox is shared by every browser world.
        private static string? ResidentName(string resident) => resident switch
        {
            "helen" => "Helen Park",
            "arthur" => "Arthur Lee",
            _ => null,
        };
    }
}
