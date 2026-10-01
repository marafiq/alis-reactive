namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// A hold the coordinator places on a room for a prospective resident. The room is chosen from the
    /// room cards, so it travels in a hidden field rather than an input with its own message.
    /// </summary>
    public class RoomHoldModel
    {
        public string? RoomCode { get; set; }
        public string? ProspectName { get; set; }
    }

    /// <summary>The confirmation the server returns once the room is held.</summary>
    public sealed record RoomHoldConfirmation(string Confirmation);
}
