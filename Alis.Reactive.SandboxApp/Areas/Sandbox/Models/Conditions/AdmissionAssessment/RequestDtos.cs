namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.AdmissionAssessment;

public class VerifyVeteranRequest
{
    public string VaId { get; set; } = "";
    public string ServiceBranch { get; set; } = "";
}

public class AlertElopementRequest
{
    public string ResidentName { get; set; } = "";
    public string WanderFrequency { get; set; } = "";
}

public class AlertHypertensionRequest
{
    public decimal SystolicBP { get; set; }
    public string ResidentName { get; set; } = "";
}

public class AlertUncontrolledRequest
{
    public decimal A1cLevel { get; set; }
    public string DiabetesType { get; set; } = "";
}

public class AlertNeuroRequest
{
    public string InjuryType { get; set; } = "";
    public string ResidentName { get; set; } = "";
}

public class AlertPainRequest
{
    public decimal PainLevel { get; set; }
    public string PainLocation { get; set; } = "";
}

public class RequestRoomSetupRequest
{
    public string MobilityAid { get; set; } = "";
    public string ResidentName { get; set; } = "";
}
