namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class MoveInDayViewModel
    {
        public List<MoveInDayResident> Roster { get; set; } = new();
        public List<MoveInDayResident> Arrivals { get; set; } = new();
        public List<MoveInDayDeparture> Departures { get; set; } = new();

        public string MovingOut { get; set; } = "";
    }

    public class MoveInDayResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string CareLevel { get; set; } = "";
        public string Suite { get; set; } = "";

        public bool AgreementSigned { get; set; }
    }

    public sealed record MoveInDayDeparture(int ResidentId, string ResidentName, string Suite, string Destination);
}
