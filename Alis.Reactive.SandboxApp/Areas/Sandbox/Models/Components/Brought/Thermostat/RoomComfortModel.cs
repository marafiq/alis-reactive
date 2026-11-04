namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought
{
    public class RoomComfortModel
    {
        public int Target { get; set; } = 70;

        public string? Pin { get; set; }

        public decimal HoldMinutes { get; set; } = 30;
    }

    public sealed class ComfortPolicy
    {
        public int Min { get; set; }

        public int Max { get; set; }

        public int Night { get; set; }
    }

    public sealed class RoomComfortSaved
    {
        public string Confirmation { get; set; } = "";
    }
}
