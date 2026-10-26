namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought
{
    // Journey: on the night round, a resident presses the call bell. The desk hears it only while someone
    // is on duty, the bell says so itself when no one is, and every answer is logged. The bell is a widget
    // the sandbox wrote itself that takes each listener as a property (BroughtComponents/CallBell).
    public class NightCallModel
    {
        /// <summary>The id of the room's bell; the desk's partial listens to it by this id.</summary>
        public const string BellId = "room-bell";

        /// <summary>The room the bell is in.</summary>
        public string Room { get; set; } = "12";

        /// <summary>Who lives there.</summary>
        public string Resident { get; set; } = "Margaret Hale";
    }

    /// <summary>The desk's card while someone is on duty.</summary>
    public class DeskOnDutyModel
    {
        /// <summary>Who is at the desk.</summary>
        public string Nurse { get; set; } = "Nurse Patel";
    }
}
