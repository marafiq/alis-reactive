namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// The Cedar wing front desk page: the admission form's name and suite, the suite a resident moves to,
    /// and the residents moving out today.
    /// </summary>
    public class RosterCrudModel
    {
        public string? AdmitName { get; set; }
        public string? AdmitSuite { get; set; }
        public string? NewSuite { get; set; }
        public List<CedarMovingOut> MovingOutToday { get; set; } = new();
    }

    /// <summary>A resident on the Cedar wing roster, one row of the roster grid.</summary>
    public class CedarResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Suite { get; set; } = "";
        public string MovedIn { get; set; } = "";
    }

    /// <summary>A resident whose move-out the business office scheduled for today.</summary>
    public class CedarMovingOut
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Suite { get; set; } = "";
    }

    /// <summary>A new resident on the roster, and the row they take in suite order.</summary>
    public class CedarAdmitted
    {
        public CedarResident Resident { get; set; } = new();
        public int Position { get; set; }
        public string Summary { get; set; } = "";
    }

    /// <summary>The resident picked in the roster, and the suite they move to.</summary>
    public class CedarSuiteChange
    {
        public List<CedarResident> Residents { get; set; } = new();
        public string? NewSuite { get; set; }
    }

    /// <summary>A resident's row after a suite change.</summary>
    public class CedarResidentMoved
    {
        public CedarResident Resident { get; set; } = new();
        public string Summary { get; set; } = "";
    }

    /// <summary>The resident whose move-out the front desk records.</summary>
    public class CedarMoveOut
    {
        public int ResidentId { get; set; }
    }

    /// <summary>The resident who moved out.</summary>
    public class CedarMovedOut
    {
        public int ResidentId { get; set; }
        public string Summary { get; set; } = "";
    }

    /// <summary>Why the front desk's change was refused.</summary>
    public class CedarDeskRefusal
    {
        public string Summary { get; set; } = "";
    }
}
