namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// A room the administrator adds to the community's rate sheet. The monthly rate is a plain
    /// decimal: the rate sheet has no "rate to be decided", so a blank or $0 rate is no rate. The
    /// one-time community fee is nullable: blank is not decided yet, and $0 means it is waived.
    /// </summary>
    public class RoomRateModel
    {
        public string? RoomName { get; set; }
        public decimal MonthlyRate { get; set; }
        public decimal? CommunityFee { get; set; }
    }

    /// <summary>The confirmation the server returns once the room is on the rate sheet.</summary>
    public sealed record RoomRateConfirmation(string Confirmation);
}
