namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class RoomRateModel
    {
        public string? RoomName { get; set; }
        public decimal MonthlyRate { get; set; }
        public decimal? CommunityFee { get; set; }

        public int? SquareFeet { get; set; }
    }

    public sealed record RoomRateConfirmation(string Confirmation);

    public sealed record RateSheetPolicy(string Community, decimal MaxMonthlyRate);
}
