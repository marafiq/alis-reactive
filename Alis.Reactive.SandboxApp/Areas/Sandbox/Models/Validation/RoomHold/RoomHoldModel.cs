namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class RoomHoldModel
    {
        public string? RoomCode { get; set; }
        public string? ProspectName { get; set; }

        public bool FamilyAgreedToHoldTerms { get; set; }
    }

    public sealed record RoomHoldConfirmation(string Confirmation);
}
