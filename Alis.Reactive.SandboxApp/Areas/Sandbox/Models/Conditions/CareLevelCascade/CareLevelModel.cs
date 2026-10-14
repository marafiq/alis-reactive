namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.CareLevelCascade;

/// <summary>
/// Walter Cho's care after his reassessment: the care level the coordinator sets, the care protocol that
/// follows it, and whether he goes out with an escort.
/// </summary>
public class CareLevelModel
{
    public string CareLevel { get; set; } = "";
    public string Protocol { get; set; } = "";
    public bool RequiresEscort { get; set; }
}
