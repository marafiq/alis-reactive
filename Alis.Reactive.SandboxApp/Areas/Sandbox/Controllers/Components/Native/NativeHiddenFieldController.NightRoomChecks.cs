using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.HiddenField;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Native
{
    /// <summary>
    /// Night room checks on Maple wing: the server stamps the start of the round, names the resident in
    /// each room, and records each check with the wing, the round and the resident the page sends.
    /// </summary>
    public partial class NativeHiddenFieldController
    {
        private const string RoundStartTime = "02:00";

        private static readonly IReadOnlyDictionary<string, RoomOccupant> OccupantByRoom = new Dictionary<string, RoomOccupant>
        {
            ["Maple 110"] = new RoomOccupant { ResidentId = "3104", ResidentName = "Arthur Pell" },
            ["Maple 112"] = new RoomOccupant { ResidentId = "3107", ResidentName = "Lillian Ford" },
            ["Maple 114"] = new RoomOccupant { ResidentId = "3111", ResidentName = "Samuel Ortiz" },
        };

        [HttpGet(""), ActionName("Index")]
        public IActionResult NightRoomChecks()
        {
            return View("~/Areas/Sandbox/Views/Components/Native/NativeHiddenField/Index.cshtml", new NightRoomCheck { Wing = "Maple" });
        }

        [HttpPost("NightRound/Start")]
        public IActionResult StartNightRound([FromBody] NightRoundStart? start)
        {
            if (start == null || start.Wing.Length == 0) return BadRequest();

            return Ok(new NightRoundStarted
            {
                StartedAt = RoundStartTime,
                Announcement = $"{start.Wing} wing round started at {RoundStartTime}.",
            });
        }

        [HttpGet("NightRound/Occupant")]
        public IActionResult Occupant([FromQuery] RoomOccupantQuery query)
        {
            return OccupantByRoom.TryGetValue(query.Room, out var occupant) ? Ok(occupant) : NotFound();
        }

        [HttpPost("NightRound/Record")]
        public IActionResult RecordRoomCheck([FromBody] NightRoomCheck? check)
        {
            if (check == null) return BadRequest();

            var result = new NightRoomCheckValidator().Validate(check);
            if (!result.IsValid) return BadRequest(new { errors = result.ErrorsByField });

            var resident = OccupantByRoom.Values.FirstOrDefault(occupant => occupant.ResidentId == check.ResidentId);
            if (resident == null || string.IsNullOrEmpty(check.RoundStartedAt) || check.Wing.Length == 0) return BadRequest();

            return Ok(new RoomCheckRecorded
            {
                Summary = $"{check.Wing} wing, {check.RoundStartedAt} round: {resident.ResidentName} "
                    + $"(resident {resident.ResidentId}), {check.Finding!.ToLowerInvariant()}.",
            });
        }
    }
}
