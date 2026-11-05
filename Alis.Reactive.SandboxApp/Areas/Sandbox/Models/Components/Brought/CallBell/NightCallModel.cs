namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought
{
    public class NightCallModel
    {
        public const string BellId = "room-bell";

        public string Room { get; set; } = "12";

        public string Resident { get; set; } = "Margaret Hale";
    }

    public class DeskOnDutyModel
    {
        public string Nurse { get; set; } = "Nurse Patel";
    }
}
