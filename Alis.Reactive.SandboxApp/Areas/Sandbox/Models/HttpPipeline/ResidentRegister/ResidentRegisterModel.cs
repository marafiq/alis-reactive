namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Resident register: the front-desk coordinator finds residents by name, moves a resident to another
    /// room, removes duplicate records, and prepares the wing's fire-drill roster.
    /// </summary>
    public class ResidentRegisterModel
    {
        /// <summary>Part of a resident's name to look for.</summary>
        public string? SearchName { get; set; }

        /// <summary>The record number of the resident who moves; null until the coordinator chooses one.</summary>
        public string? MoveResidentId { get; set; }

        /// <summary>The room the resident moves to.</summary>
        public string? MoveToRoom { get; set; }

        /// <summary>The records on the register when the page opens.</summary>
        public IReadOnlyList<RegisterRecord> Records { get; set; } = Array.Empty<RegisterRecord>();
    }

    /// <summary>
    /// One record on the register. A record that holds a care plan is the resident's own; one without is a
    /// duplicate entered by mistake.
    /// </summary>
    public sealed record RegisterRecord(int RecordId, string Name, string Room, bool HoldsCarePlan);

    /// <summary>How many records the register holds, for how many residents, and who moved in last.</summary>
    public sealed record RegisterSummary(int Records, int Residents, string NewestResident);

    /// <summary>The query <c>Find?name=</c> binds: part of a resident's name.</summary>
    public sealed class RegisterSearch
    {
        public string? Name { get; set; }
    }

    /// <summary>The records whose name holds the text searched for, as the server lists them.</summary>
    public sealed record RegisterMatches(int Count, string Records);

    /// <summary>The body <c>PUT Room</c> binds: whose room changes, and to which room.</summary>
    public sealed class RegisterRoomMove
    {
        public string? ResidentId { get; set; }
        public string? Room { get; set; }
    }

    /// <summary>The server's answer to a room move it made.</summary>
    public sealed record RegisterRoomMoved(string Message);

    /// <summary>Why the server refused a move: the room belongs to another resident.</summary>
    public sealed record RegisterRoomTaken(string Reason);

    /// <summary>The body <c>DELETE Records/{recordId}</c> binds: why the record is removed.</summary>
    public sealed class RegisterRecordRemoval
    {
        public string? Reason { get; set; }
    }

    /// <summary>The server's answer to a record it removed.</summary>
    public sealed record RegisterRecordRemoved(string Message);

    /// <summary>The body <c>POST Roster</c> binds: the wing whose fire-drill roster is prepared.</summary>
    public sealed class RegisterRosterRequest
    {
        public string? Wing { get; set; }
    }

    /// <summary>A wing's fire-drill roster: every record on the wing, by room.</summary>
    public sealed record RegisterRoster(string Wing, IReadOnlyList<RegisterRecord> Records);

    /// <summary>The document events the resident register raises and hears.</summary>
    public static class ResidentRegisterEvents
    {
        /// <summary>A record was removed or a resident moved: the list of records is out of date.</summary>
        public static readonly AppEvent RegisterChanged = AppEvent.Declare();
    }
}
