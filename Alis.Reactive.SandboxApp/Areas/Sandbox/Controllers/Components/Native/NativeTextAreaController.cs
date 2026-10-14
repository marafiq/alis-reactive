using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Native
{
    /// <summary>
    /// Shift handover: the evening charge nurse writes the handover for the night nurse, flags any
    /// incident, and sends it.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/NativeTextArea")]
    public class NativeTextAreaController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/Components/Native/NativeTextArea/Index.cshtml", new NativeTextAreaModel());

        [HttpPost("Send")]
        public IActionResult Send([FromBody] NativeTextAreaModel? handover)
        {
            if (string.IsNullOrWhiteSpace(handover?.HandoverNote)) return BadRequest();

            var lines = handover.HandoverNote.Split('\n').Count(line => !string.IsNullOrWhiteSpace(line));
            var incident = string.IsNullOrWhiteSpace(handover.Incident) ? "no incidents" : "and an incident to read first";
            return Ok(new ShiftHandoverSent { Message = $"Handover sent to the night nurse: {lines} lines, {incident}." });
        }
    }
}
