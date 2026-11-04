namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class RosterCrudModel
    {
        public string? AdmitName { get; set; }
        public string? AdmitSuite { get; set; }
        public string? NewSuite { get; set; }
        public List<CedarMovingOut> MovingOutToday { get; set; } = new();
    }

    public class CedarResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Suite { get; set; } = "";
        public string MovedIn { get; set; } = "";
    }

    public class CedarMovingOut
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Suite { get; set; } = "";
    }

    public class CedarAdmitted
    {
        public CedarResident Resident { get; set; } = new();
        public int Position { get; set; }
        public string Summary { get; set; } = "";
    }

    public class CedarSuiteChange
    {
        public List<CedarResident> Residents { get; set; } = new();
        public string? NewSuite { get; set; }
    }

    public class CedarResidentMoved
    {
        public CedarResident Resident { get; set; } = new();
        public string Summary { get; set; } = "";
    }

    public class CedarMoveOut
    {
        public int ResidentId { get; set; }
    }

    public class CedarMovedOut
    {
        public int ResidentId { get; set; }
        public string Summary { get; set; } = "";
    }

    public class CedarDeskRefusal
    {
        public string Summary { get; set; } = "";
    }
}
