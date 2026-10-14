using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.DropDownList;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Admissions at Maple Grove places Eleanor Whitfield: the care levels the community offers, the wings
    /// that offer each one with the wing that has a room open this week, and the placement itself, which
    /// admissions refuses for a wing whose waitlist is full.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/DropDownList")]
    public sealed class DropDownListController : Controller
    {
        private const string Resident = "Eleanor Whitfield";

        private static readonly IReadOnlyList<CareLevelOption> CareLevels =
        [
            new() { Code = "independent", Name = "Independent Living" },
            new() { Code = "assisted", Name = "Assisted Living" },
            new() { Code = "memory", Name = "Memory Care" }
        ];

        // Each wing offers one care level; a wing with a room open this week takes a resident now, the
        // others take her on their waitlist, unless it is full.
        private static readonly IReadOnlyList<CommunityWing> CommunityWings =
        [
            new("birch", "Birch", "Main building", "independent", OpenRoom: true),
            new("aspen", "Aspen", "Garden house", "independent", OpenRoom: false),
            new("maple", "Maple", "Main building", "assisted", OpenRoom: false),
            new("cedar", "Cedar", "Main building", "assisted", OpenRoom: true),
            new("laurel", "Laurel", "Garden house", "assisted", OpenRoom: false),
            new("willow", "Willow", "Garden house", "memory", OpenRoom: false),
            new("juniper", "Juniper", "Garden house", "memory", OpenRoom: false, WaitlistFull: true)
        ];

        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/Components/Fusion/DropDownList/Index.cshtml",
                new FusionDropDownListModel { CareLevels = CareLevels });

        [HttpGet("Wings")]
        public IActionResult Wings([FromQuery] WingListQuery query)
        {
            var offering = CommunityWings.Where(wing => wing.CareLevel == query.CareLevel).ToList();
            if (offering.Count == 0)
                return NotFound();

            return Ok(new WingList
            {
                Wings = offering.Select(wing => new PlacementWing { Code = wing.Code, Name = wing.Name, Building = wing.Building }).ToList(),
                OpenRoomWing = offering.FirstOrDefault(wing => wing.OpenRoom)?.Code ?? ""
            });
        }

        [HttpPost("Admit")]
        public IActionResult Admit([FromBody] FusionDropDownListModel placement)
        {
            var level = CareLevels.FirstOrDefault(careLevel => careLevel.Code == placement.CareLevel);
            var wing = CommunityWings.FirstOrDefault(candidate => candidate.Code == placement.Wing);
            if (level is null || wing is null || wing.CareLevel != level.Code)
                return BadRequest(new PlacementConfirmation { Message = "Choose a care level and one of the wings that offer it." });
            if (wing.WaitlistFull)
                return BadRequest(new PlacementConfirmation { Message = $"{wing.Name}'s waitlist is full until spring; choose another wing for {Resident}." });

            return Ok(new PlacementConfirmation
            {
                Message = wing.OpenRoom
                    ? $"{Resident} moves into {wing.Name} ({level.Name}) on Monday, October 20."
                    : $"{Resident} is on the waitlist for {wing.Name} ({level.Name})."
            });
        }

        private sealed record CommunityWing(string Code, string Name, string Building, string CareLevel, bool OpenRoom, bool WaitlistFull = false);
    }
}
