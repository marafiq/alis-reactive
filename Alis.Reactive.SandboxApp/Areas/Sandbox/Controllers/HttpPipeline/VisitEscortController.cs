using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Alis.Reactive.SandboxApp.BroughtComponents;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.HttpPipeline
{
    /// <summary>
    /// The wing's desk, which books escorts for family visits. Helen Park (Maple wing, two visits a week)
    /// and Arthur Bell (Cedar wing, one) each have a week of times; the desk the X-Desk header names
    /// answers with its own escort, Dana at the east desk and Malik at the west, and a request that names
    /// no desk is refused. Every answer repeats what the desk received, so a value lost between the page
    /// and the desk shows up on the page.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/HttpPipeline/VisitEscort")]
    public class VisitEscortController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/HttpPipeline/VisitEscort/Index.cshtml", new VisitEscortModel());

        [HttpGet("Week/{resident}")]
        public IActionResult Week(string resident) => resident switch
        {
            "helen" => Ok(new EscortWeek
            {
                Resident = "Helen Park",
                Wing = "Maple",
                Capacity = 2,
                Slots = Times("mon-10", "wed-14", "sat-11"),
            }),
            "arthur" => Ok(new EscortWeek
            {
                Resident = "Arthur Bell",
                Wing = "Cedar",
                Capacity = 1,
                Slots = Times("tue-15", "fri-10"),
            }),
            _ => NotFound(new EscortMessage("No resident matches this link.")),
        };

        [HttpGet("Escorts/{slot}")]
        public IActionResult Escort(string slot, string? wing, int visit, [FromHeader(Name = "X-Desk")] string? desk)
        {
            if (string.IsNullOrWhiteSpace(desk))
                return BadRequest(new EscortMessage("No desk is named in the link: book the escort by phone."));

            return Ok(new EscortMessage($"{EscortAt(desk)} will escort {Label(slot)}: visit {visit} of the week, {wing} wing, {desk} desk."));
        }

        [HttpDelete("Escorts/{slot}")]
        public IActionResult Release(string slot, [FromBody] EscortRelease? release) =>
            Ok(new EscortMessage($"{Label(slot)}: escort released ({release?.Reason ?? "no reason given"})."));

        [HttpPost("Share")]
        public IActionResult Share([FromBody] WeekShare? share) =>
            string.IsNullOrWhiteSpace(share?.Summary)
                ? Ok(new EscortMessage("Nothing to share yet: no visit is booked."))
                : Ok(new EscortMessage($"Sent to the family: {share.Summary}. {Visits(share.VisitsLeft)} left this week."));

        [HttpPut("Party/{slot}")]
        public IActionResult Party(string slot, [FromBody] PartyUpdate? party)
        {
            if (party?.Party is not { } details)
                return Ok(new EscortMessage($"{Label(slot)}: no party details reached the desk."));

            var companion = party.Companions.FirstOrDefault()?.Name;
            return Ok(new EscortMessage($"{Label(slot)}: {details.Visitors} visitors, companion {Named(companion)}; note: {details.Note}."));
        }

        [HttpGet("Seating")]
        public IActionResult Seating([FromQuery] PartyQuery query) =>
            Ok(new EscortMessage($"The family room seats {query.Party.Visitors} visitors and {Named(query.Companions.FirstOrDefault()?.Name)}."));

        // Built per call: a static collection in the sandbox is shared by every browser world.
        private static List<VisitSlot> Times(params string[] ids) =>
            ids.Select(id => new VisitSlot { Id = id, Label = Label(id), Day = DayOf(id) }).ToList();

        private static string Label(string slot) => slot switch
        {
            "mon-10" => "Mon 10:00 AM",
            "wed-14" => "Wed 2:00 PM",
            "sat-11" => "Sat 11:00 AM",
            "tue-15" => "Tue 3:00 PM",
            "fri-10" => "Fri 10:00 AM",
            _ => slot,
        };

        private static string DayOf(string slot) => slot switch
        {
            "mon-10" => "Monday",
            "wed-14" => "Wednesday",
            "sat-11" => "Saturday",
            "tue-15" => "Tuesday",
            "fri-10" => "Friday",
            _ => "",
        };

        private static string EscortAt(string desk) => desk switch
        {
            "east" => "Dana",
            "west" => "Malik",
            _ => "Someone from the " + desk + " desk",
        };

        private static string Visits(int count) => count == 1 ? "1 visit" : $"{count} visits";

        private static string Named(string? companion) => string.IsNullOrWhiteSpace(companion) ? "no companion" : companion;
    }
}
