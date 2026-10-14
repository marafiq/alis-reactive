using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.HttpPipeline
{
    /// <summary>
    /// The community's resident API, as the shift email's link reaches it. Every answer is scoped to the
    /// community the X-Community header names: Helen Park (1042) and Arthur Bell (1043) live at Maple
    /// Grove, and Cedar Point has neither. A chart names the dose due on the shift its query names; a
    /// dose's last administration is looked up by the medication's name in the path; the shift notes come
    /// a page at a time. The community's integration gateway forwards a partner check to the partner its
    /// X-Partner header names: Riverside Pharmacy or Northgate Lab.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/HttpPipeline/ShiftEmailLink")]
    public class ShiftEmailLinkController : Controller
    {
        private const string MapleGrove = "maple-grove";

        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/HttpPipeline/ShiftEmailLink/Index.cshtml", new ShiftEmailLinkPageModel());

        [HttpGet("Residents/{residentId:int}/Chart")]
        public IActionResult Chart(int residentId, string? shift, [FromHeader(Name = "X-Community")] string? community)
        {
            if (Refusal(residentId, community) is { } refusal) return refusal;
            if (Resident(residentId) is not { } resident)
                return NotFound(new ShiftEmailRefusal($"Resident {residentId} does not live at Maple Grove."));
            if (DueDose(residentId, shift) is not { } due)
                return BadRequest(new ShiftEmailRefusal("Name the shift: Day or Night."));

            return Ok(new ShiftEmailChart(resident.Name, resident.Room, "Maple Grove", $"{due.Medication} at {due.Time}", due.Medication));
        }

        // A medication's name may hold a slash (Carbidopa/Levodopa): the path carries it encoded, and the
        // route keeps an encoded slash as it came, so the name is decoded here.
        [HttpGet("Residents/{residentId:int}/Doses/{medication}")]
        public IActionResult LastDose(int residentId, string medication, [FromHeader(Name = "X-Community")] string? community)
        {
            if (Refusal(residentId, community) is { } refusal) return refusal;

            return (residentId, Uri.UnescapeDataString(medication)) switch
            {
                (1042, "Donepezil 10 mg") => Ok(new ShiftEmailLastDose("Last given yesterday at 9:04 PM by Nora Ellis.")),
                (1042, "Metformin 500 mg") => Ok(new ShiftEmailLastDose("Last given today at 8:10 AM by Elena Ruiz.")),
                (1043, "Carbidopa/Levodopa 25/100 mg") => Ok(new ShiftEmailLastDose("Last given today at 6:00 PM by Malik Stone.")),
                _ => NotFound(new ShiftEmailRefusal("This medication is not on the resident's orders.")),
            };
        }

        [HttpGet("Residents/{residentId:int}/Notes")]
        public IActionResult Notes(int residentId, string? shift, int page, [FromHeader(Name = "X-Community")] string? community)
        {
            if (Refusal(residentId, community) is { } refusal) return refusal;

            var entry = (residentId, shift, Math.Max(page, 1)) switch
            {
                (1042, "Night", 1) => "9:40 PM: Took her evening medication with applesauce.",
                (1042, "Night", 2) => "2:15 AM: Woke asking for water and settled after a short walk.",
                (1042, "Day", 1) => "11:30 AM: Joined the chair exercise class.",
                (1043, "Night", 1) => "10:05 PM: Tremor eased an hour after his dose.",
                _ => "No notes on this page.",
            };
            return Ok(new ShiftEmailNotes(entry));
        }

        [HttpGet("Residents/{residentId:int}/Partners")]
        public IActionResult PartnerCheck(
            int residentId,
            [FromHeader(Name = "X-Partner")] string? partner,
            [FromHeader(Name = "X-Community")] string? community)
        {
            if (Refusal(residentId, community) is { } refusal) return refusal;

            return (partner, residentId) switch
            {
                ("riverside-pharmacy", 1042) => Ok(new ShiftEmailPartnerAnswer("Riverside Pharmacy: the Donepezil refill is ready for pickup.")),
                ("riverside-pharmacy", _) => Ok(new ShiftEmailPartnerAnswer("Riverside Pharmacy: no refill is due.")),
                ("northgate-lab", 1042) => Ok(new ShiftEmailPartnerAnswer("Northgate Lab: the blood panel results are due Friday.")),
                ("northgate-lab", _) => Ok(new ShiftEmailPartnerAnswer("Northgate Lab: no tests are pending.")),
                _ => BadRequest(new ShiftEmailRefusal("Name the partner the gateway forwards to.")),
            };
        }

        // The community the X-Community header names scopes every answer; null when it is Maple Grove,
        // the one community whose residents this API serves.
        private IActionResult? Refusal(int residentId, string? community)
        {
            if (string.IsNullOrWhiteSpace(community))
                return BadRequest(new ShiftEmailRefusal("Name the community."));
            if (community != MapleGrove)
                return NotFound(new ShiftEmailRefusal($"Resident {residentId} does not live at {CommunityName(community)}."));
            return null;
        }

        // No static lookup table: a static collection in the sandbox is shared by every browser world.
        private static (string Name, string Room)? Resident(int residentId) => residentId switch
        {
            1042 => ("Helen Park", "Maple 214"),
            1043 => ("Arthur Bell", "Cedar 108"),
            _ => null,
        };

        private static string CommunityName(string community) => community == "cedar-point" ? "Cedar Point" : community;

        private static (string Medication, string Time)? DueDose(int residentId, string? shift) => (residentId, shift) switch
        {
            (1042, "Night") => ("Donepezil 10 mg", "9:00 PM"),
            (1042, "Day") => ("Metformin 500 mg", "8:00 AM"),
            (1043, "Night") => ("Carbidopa/Levodopa 25/100 mg", "10:00 PM"),
            (1043, "Day") => ("Carbidopa/Levodopa 25/100 mg", "8:00 AM"),
            _ => null,
        };
    }
}
