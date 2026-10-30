namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought
{
    // Journey: the overnight nurse sets a resident's room comfort before rounds. The wing's policy gives
    // the range and the night setting; the thermostat is a widget the sandbox wrote itself and brings like
    // any app would (BroughtComponents/Thermostat).
    public class RoomComfortModel
    {
        /// <summary>The target temperature; the thermostat's value.</summary>
        public int Target { get; set; } = 70;

        /// <summary>The code that unlocks the thermostat's own buttons.</summary>
        public string? Pin { get; set; }

        /// <summary>How long the target is held.</summary>
        public decimal HoldMinutes { get; set; } = 30;
    }

    /// <summary>The wing's comfort policy from the Policy endpoint.</summary>
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
