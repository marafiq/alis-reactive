namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    // Journey: a resident sets their Comfort & Care Preferences.
    // RoomTemperature is a scalar comfort preference (degrees Fahrenheit);
    // QuietHours is the two-handle window (start hour, end hour) when staff
    // avoid non-urgent visits. Both carry over the preference saved last month.
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
