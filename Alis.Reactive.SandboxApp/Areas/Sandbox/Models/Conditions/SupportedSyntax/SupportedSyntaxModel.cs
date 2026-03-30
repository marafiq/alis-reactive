namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

/// <summary>
/// Dedicated vertical slice for the supported condition syntax.
/// Exercises trigger and .Reactive() flows using only the outer When/Then DSL,
/// with unconditional commands mixed around the conditional branch.
/// </summary>
public class SupportedSyntaxModel
{
    public decimal RiskScore { get; set; }
}

/// <summary>
/// Typed payload for the trigger-side supported syntax demo.
/// </summary>
public class SupportedSyntaxTriggerPayload
{
    public int Score { get; set; }
}
