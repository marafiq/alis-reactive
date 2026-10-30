using System.Text.Json;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.HttpPipeline
{
    /// <summary>
    /// Resident register for Maple wing: Helen Park (record 1042), Arthur Bell (1043) and Rosa Diaz (1044,
    /// the newest resident) each hold a care plan; records 1047 (Helen Park) and 1048 (Arthur Bell) are
    /// duplicates entered by mistake. A record that holds a care plan is never removed. A resident moves
    /// only into a room no other resident lives in. The records this browser removed and the rooms it
    /// changed live in its own session, so no other browser sees them.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/HttpPipeline/ResidentRegister")]
    public class ResidentRegisterController : Controller
    {
        private const string ViewFolder = "~/Areas/Sandbox/Views/HttpPipeline/ResidentRegister/";
        private const string ChangesKey = "residentRegisterChanges";

        [HttpGet("")]
        public IActionResult Index() =>
            View(ViewFolder + "Index.cshtml", new ResidentRegisterModel { Records = CurrentRecords() });

        [HttpGet("Summary")]
        public IActionResult Summary()
        {
            var records = CurrentRecords();
            var residents = records.Where(record => record.HoldsCarePlan).ToList();
            var newest = residents.MaxBy(record => record.RecordId)!;
            return Ok(new RegisterSummary(records.Count, residents.Count, newest.Name));
        }

        [HttpGet("Find")]
        public IActionResult Find([FromQuery] RegisterSearch search)
        {
            var text = search.Name?.Trim() ?? "";
            var matches = CurrentRecords()
                .Where(record => record.Name.Contains(text, StringComparison.OrdinalIgnoreCase))
                .Select(record => $"{record.Name}, {record.Room} (record {record.RecordId})")
                .ToList();
            var listed = matches.Count == 0 ? $"No resident on the register matches {text}." : string.Join("; ", matches);
            return Ok(new RegisterMatches(matches.Count, listed));
        }

        [HttpPut("Room")]
        public IActionResult MoveRoom([FromBody] RegisterRoomMove? move)
        {
            var room = move?.Room?.Trim() ?? "";
            var records = CurrentRecords();
            var resident = records.FirstOrDefault(record => record.HoldsCarePlan && record.RecordId.ToString() == move?.ResidentId);
            if (resident == null || room.Length == 0) return UnprocessableEntity();

            var occupant = records.FirstOrDefault(record =>
                record.Name != resident.Name && string.Equals(record.Room, room, StringComparison.OrdinalIgnoreCase));
            if (occupant != null) return Conflict(new RegisterRoomTaken($"{occupant.Room} is {occupant.Name}'s room."));

            var changes = Changes();
            changes.Rooms[resident.RecordId] = room;
            Save(changes);
            return Ok(new RegisterRoomMoved($"{resident.Name} now lives in {room}."));
        }

        [HttpGet("Records")]
        public IActionResult Records() => PartialView(ViewFolder + "_RegisterRecords.cshtml", CurrentRecords());

        [HttpDelete("Records/{recordId:int}")]
        public IActionResult RemoveDuplicate(int recordId, [FromBody] RegisterRecordRemoval? removal)
        {
            var record = CurrentRecords().FirstOrDefault(entry => entry.RecordId == recordId);
            if (record == null) return NotFound();
            if (record.HoldsCarePlan) return Conflict();

            var changes = Changes();
            changes.Removed.Add(recordId);
            Save(changes);
            return Ok(new RegisterRecordRemoved($"Removed record {recordId} for {record.Name}: {removal?.Reason}."));
        }

        [HttpPost("Roster")]
        public IActionResult Roster([FromBody] RegisterRosterRequest? request)
        {
            if (string.IsNullOrWhiteSpace(request?.Wing)) return BadRequest();

            var wing = request.Wing;
            var onWing = CurrentRecords()
                .Where(record => record.Room.StartsWith(wing + " ", StringComparison.OrdinalIgnoreCase))
                .OrderBy(record => record.Room, StringComparer.OrdinalIgnoreCase)
                .ThenBy(record => record.RecordId)
                .ToList();
            return PartialView(ViewFolder + "_FireDrillRoster.cshtml", new RegisterRoster(wing, onWing));
        }

        private List<RegisterRecord> CurrentRecords()
        {
            var changes = Changes();
            return SeedRecords()
                .Where(record => !changes.Removed.Contains(record.RecordId))
                .Select(record => changes.Rooms.TryGetValue(record.RecordId, out var room) ? record with { Room = room } : record)
                .ToList();
        }

        // Built per call: a static collection in the sandbox is shared by every browser world.
        private static RegisterRecord[] SeedRecords() => new[]
        {
            new RegisterRecord(1042, "Helen Park", "Maple 214", HoldsCarePlan: true),
            new RegisterRecord(1043, "Arthur Bell", "Maple 216", HoldsCarePlan: true),
            new RegisterRecord(1044, "Rosa Diaz", "Maple 218", HoldsCarePlan: true),
            new RegisterRecord(1047, "Helen Park", "Maple 214", HoldsCarePlan: false),
            new RegisterRecord(1048, "Arthur Bell", "Maple 216", HoldsCarePlan: false),
        };

        private RegisterChanges Changes()
        {
            var json = HttpContext.Session.GetString(ChangesKey);
            return json == null ? new RegisterChanges() : JsonSerializer.Deserialize<RegisterChanges>(json)!;
        }

        private void Save(RegisterChanges changes) =>
            HttpContext.Session.SetString(ChangesKey, JsonSerializer.Serialize(changes));

        /// <summary>What this browser changed on the register: the records it removed and the rooms it moved residents to.</summary>
        private sealed class RegisterChanges
        {
            public List<int> Removed { get; set; } = new();
            public Dictionary<int, string> Rooms { get; set; } = new();
        }
    }
}
