using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.RoomEvening;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Patterns
{
    /// <summary>
    /// Room 12's evening: a visit card loaded into the log, an escort card loaded into the visit card, and
    /// the quiet-night card that replaces the visit. URL: /Sandbox/Patterns/RoomEvening.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Patterns/RoomEvening")]
    public class RoomEveningController : Controller
    {
        private const string ViewBase = "~/Areas/Sandbox/Views/Patterns/RoomEvening/";

        [HttpGet("")]
        public IActionResult Index() => View(ViewBase + "Index.cshtml", new RoomEveningModel());

        // The family visit card: the visitor's inputs, the sign-in rule, and a slot for the escort card.
        [HttpGet("FamilyVisit")]
        public IActionResult FamilyVisit() => PartialView(ViewBase + "_FamilyVisit.cshtml", new RoomEveningModel());

        // The quiet-night card that replaces the visit: no plan of its own.
        [HttpGet("QuietNight")]
        public IActionResult QuietNight() => PartialView(ViewBase + "_QuietNight.cshtml");

        // The escort card, loaded into the visit card's own slot.
        [HttpGet("EscortCard")]
        public IActionResult EscortCard() => PartialView(ViewBase + "_EscortCard.cshtml", new RoomEveningModel());

        [HttpPost("SignIn")]
        public IActionResult SignIn([FromBody] RoomEveningModel? visit)
        {
            if (visit == null || string.IsNullOrWhiteSpace(visit.Visitor)) return BadRequest();
            var relationship = string.IsNullOrWhiteSpace(visit.Relationship) ? "a visitor" : visit.Relationship.ToLowerInvariant();
            return Ok(new VisitSigned($"{visit.Visitor} signed in for room {visit.Room} as {relationship}."));
        }

        [HttpPost("SaveLog")]
        public IActionResult SaveLog([FromBody] RoomEveningModel? log)
        {
            if (log == null || string.IsNullOrWhiteSpace(log.RoomNote)) return BadRequest();
            var visitor = string.IsNullOrWhiteSpace(log.Visitor) ? "none" : log.Visitor;
            return Ok(new RoomLogSaved($"Evening log saved: {log.RoomNote} Visitor: {visitor}."));
        }
    }
}
