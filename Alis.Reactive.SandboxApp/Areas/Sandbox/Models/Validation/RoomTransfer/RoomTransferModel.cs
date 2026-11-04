namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class RoomTransferModel
    {
        public string? ResidentName { get; set; }
        public string? FromRoom { get; set; }
        public string? ToRoom { get; set; }
        public DateTime? TransferDate { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }

        public string? WeekendApprover { get; set; }
    }

    public sealed record RoomTransferConfirmation(string Confirmation);
}
