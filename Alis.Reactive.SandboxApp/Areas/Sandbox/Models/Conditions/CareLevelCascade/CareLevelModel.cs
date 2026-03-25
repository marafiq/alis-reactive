namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

/// <summary>
/// Model for the Care Level Cascade vertical slice.
/// Exercises condition → component mutation patterns:
///   When(dropdown.Value()).Eq("Memory Care") → SetValue on protocol dropdown
///   When(dropdown.Value()).In("Memory Care","Skilled Nursing") → SetChecked on switch
/// Senior living domain: care level drives protocol assignment and escort requirements.
/// </summary>
public class CareLevelModel
{
    public string CareLevel { get; set; } = "";
    public string Protocol { get; set; } = "";
    public bool RequiresEscort { get; set; }
}
