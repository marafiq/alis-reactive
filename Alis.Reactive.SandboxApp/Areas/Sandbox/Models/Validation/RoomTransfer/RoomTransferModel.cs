namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// A room transfer the coordinator requests. The page checks what it can; which rooms are occupied,
    /// when housekeeping works and who approves a weekend move are the server's to know.
    /// </summary>
    public class RoomTransferModel
    {
        public string? ResidentName { get; set; }
        public string? FromRoom { get; set; }
        public string? ToRoom { get; set; }
        public DateTime? TransferDate { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }

        /// <summary>The administrator on call who approves a transfer on a Saturday or Sunday.</summary>
        public string? WeekendApprover { get; set; }
    }

    /// <summary>What the server says once the transfer is requested, or housekeeping has answered.</summary>
    public sealed record RoomTransferConfirmation(string Confirmation);
}
