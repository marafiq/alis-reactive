namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class SliderModel
    {
        public double RoomTemperature { get; set; }

        public double[] QuietHours { get; set; } = [];
    }

    public sealed class ComfortPreferencesRequest
    {
        public double RoomTemperature { get; set; }

        public double[] QuietHours { get; set; } = [];
    }

    public sealed class ComfortPreferencesResponse
    {
        public string Summary { get; set; } = string.Empty;
    }
}
