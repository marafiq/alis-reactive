using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.RoomEvening;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Patterns
{
    [Area("Sandbox")]
    [Route("Sandbox/Patterns/RoomEvening")]
    public class RoomEveningController : Controller
    {
        private const string ViewBase = "~/Areas/Sandbox/Views/Patterns/RoomEvening/";

        [HttpGet("")]
        public IActionResult Index() => View(ViewBase + "Index.cshtml", new RoomEveningModel());

        [HttpGet("FamilyVisit")]
        public IActionResult FamilyVisit() => PartialView(ViewBase + "_FamilyVisit.cshtml", new RoomEveningModel());

        [HttpGet("QuietNight")]
        public IActionResult QuietNight() => PartialView(ViewBase + "_QuietNight.cshtml");

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
