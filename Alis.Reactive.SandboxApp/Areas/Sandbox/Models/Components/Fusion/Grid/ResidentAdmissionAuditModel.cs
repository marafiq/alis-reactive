namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>Move-in day on the wing: the roster, who arrives today and who leaves.</summary>
    public class MoveInDayViewModel
    {
        public List<MoveInDayResident> Roster { get; set; } = new();
        public List<MoveInDayResident> Arrivals { get; set; } = new();
        public List<MoveInDayDeparture> Departures { get; set; } = new();

        /// <summary>The resident being moved out, kept until the roster has let them go.</summary>
        public string MovingOut { get; set; } = "";
    }

    /// <summary>A resident on the roster, or arriving to join it.</summary>
    public class MoveInDayResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string CareLevel { get; set; } = "";
        public string Suite { get; set; } = "";

        /// <summary>Whether the residency agreement is signed; nobody moves in before it is.</summary>
        public bool AgreementSigned { get; set; }
    }

    /// <summary>A resident leaving today, and where they are going.</summary>
    public sealed record MoveInDayDeparture(int ResidentId, string ResidentName, string Suite, string Destination);
}
