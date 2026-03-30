namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

/// <summary>
/// Dedicated vertical slice for the supported condition syntax.
/// Exercises trigger and .Reactive() flows using only the outer When/Then DSL,
/// with unconditional commands mixed around the conditional branch.
/// </summary>
public class SupportedSyntaxModel
{
    public decimal RiskScore { get; set; }
    public decimal AssessmentScore { get; set; }
    public bool SupervisorOverride { get; set; }
    public string CareTrack { get; set; } = "";
}

/// <summary>
/// Typed payload for the trigger-side supported syntax demo.
/// </summary>
public class SupportedSyntaxTriggerPayload
{
    public int Score { get; set; }
}

/// <summary>
/// Typed payload for the richer trigger-side condition demo.
/// </summary>
public class SupportedSyntaxEscalationPayload
{
    public int Score { get; set; }
    public bool RequiresIsolation { get; set; }
    public bool ManualOverride { get; set; }
}

/// <summary>
/// DropDownList options for the component-source condition demo.
/// </summary>
public class SupportedSyntaxCareTrackOption
{
    public string Value { get; set; } = "";
    public string Text { get; set; } = "";
}
