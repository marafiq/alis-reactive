namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.MedicationRound;

/// <summary>The morning medication round on one wing: the doses due, and what the aide scans and records.</summary>
public class MedicationRoundModel
{
    /// <summary>The day of the round. An order is current on it when it stops after it.</summary>
    public DateTime RoundDate { get; set; }

    /// <summary>The doses due on the round, as the eMAR lists them when the round starts.</summary>
    public List<RoundDose> Doses { get; set; } = new();

    /// <summary>The physician has been called about a pulse outside the range and says give the dose.</summary>
    public bool PhysicianSaysGive { get; set; }

    /// <summary>The pharmacy label the aide scans before giving the dose.</summary>
    public string LabelScan { get; set; } = "";

    /// <summary>The second nurse who witnesses a controlled medication, by staff email.</summary>
    public string WitnessEmail { get; set; } = "";

    /// <summary>Why a dose is held.</summary>
    public string HoldReason { get; set; } = "";
}

/// <summary>One dose due on the round, with what the aide checks before giving it.</summary>
public class RoundDose
{
    public string Resident { get; set; } = "";
    public string Room { get; set; } = "";
    public string Medication { get; set; } = "";
    public bool OnWing { get; set; }

    /// <summary>A dose that must be given on time (Parkinson's medication, insulin).</summary>
    public bool TimeCritical { get; set; }

    public int Pulse { get; set; }

    /// <summary>Degrees Fahrenheit.</summary>
    public double Temperature { get; set; }

    public long MinutesSinceLastDose { get; set; }

    /// <summary>"Due", "On hold" or "Not available".</summary>
    public string Status { get; set; } = "";

    /// <summary>The physician's order for the dose; none when the pharmacy has no order on file.</summary>
    public RoundDoseOrder? Order { get; set; }

    /// <summary>The resident's recorded allergies; none when nothing was ever recorded.</summary>
    public string? Allergies { get; set; }

    public bool ForPain { get; set; }

    /// <summary>The resident's pain score out of 10; none until the aide asks.</summary>
    public int? PainScore { get; set; }
}

/// <summary>The physician's order behind a dose.</summary>
public class RoundDoseOrder
{
    /// <summary>The physician who signed it; none for a verbal order not yet signed.</summary>
    public string? SignedBy { get; set; }

    public DateTime StopDate { get; set; }

    /// <summary>How the dose is given; none when the order does not say.</summary>
    public string? Route { get; set; }
}

/// <summary>The document events the medication round raises and hears.</summary>
public static class MedicationRoundEvents
{
    /// <summary>The aide opened a dose on the round to check it.</summary>
    public static readonly AppEvent<RoundDose> DoseOpened = AppEvent.Named<RoundDose>("round-dose-opened");
}
