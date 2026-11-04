namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ResidentRegisterModel
    {
        public string? SearchName { get; set; }

        public string? MoveResidentId { get; set; }

        public string? MoveToRoom { get; set; }

        public IReadOnlyList<RegisterRecord> Records { get; set; } = Array.Empty<RegisterRecord>();
    }

    public sealed record RegisterRecord(int RecordId, string Name, string Room, bool HoldsCarePlan);

    public sealed record RegisterSummary(int Records, int Residents, string NewestResident);

    public sealed class RegisterSearch
    {
        public string? Name { get; set; }
    }

    public sealed record RegisterMatches(int Count, string Records);

    public sealed class RegisterRoomMove
    {
        public string? ResidentId { get; set; }
        public string? Room { get; set; }
    }

    public sealed record RegisterRoomMoved(string Message);

    public sealed record RegisterRoomTaken(string Reason);

    public sealed class RegisterRecordRemoval
    {
        public string? Reason { get; set; }
    }

    public sealed record RegisterRecordRemoved(string Message);

    public sealed class RegisterRosterRequest
    {
        public string? Wing { get; set; }
    }

    public sealed record RegisterRoster(string Wing, IReadOnlyList<RegisterRecord> Records);

    public static class ResidentRegisterEvents
    {
        public static readonly AppEvent RegisterChanged = AppEvent.Declare();
    }
}
