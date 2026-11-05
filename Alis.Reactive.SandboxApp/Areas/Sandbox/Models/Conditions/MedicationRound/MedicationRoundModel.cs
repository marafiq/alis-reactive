namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.MedicationRound;

public class MedicationRoundModel
{
    public DateTime RoundDate { get; set; }

    public List<RoundDose> Doses { get; set; } = new();

    public bool PhysicianSaysGive { get; set; }

    public string LabelScan { get; set; } = "";

    public string WitnessEmail { get; set; } = "";

    public string HoldReason { get; set; } = "";
}

public class RoundDose
{
    public string Resident { get; set; } = "";
    public string Room { get; set; } = "";
    public string Medication { get; set; } = "";
    public bool OnWing { get; set; }

    public bool TimeCritical { get; set; }

    public int Pulse { get; set; }

    public double Temperature { get; set; }

    public long MinutesSinceLastDose { get; set; }

    public string Status { get; set; } = "";

    public RoundDoseOrder? Order { get; set; }

    public string? Allergies { get; set; }

    public bool ForPain { get; set; }

    public int? PainScore { get; set; }
}

public class RoundDoseOrder
{
    public string? SignedBy { get; set; }

    public DateTime StopDate { get; set; }

    public string? Route { get; set; }
}

public static class MedicationRoundEvents
{
    public static readonly AppEvent<RoundDose> DoseOpened = AppEvent.Declare<RoundDose>();
}
