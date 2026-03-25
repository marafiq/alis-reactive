namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

/// <summary>
/// Model for the Numeric Condition vertical slice.
/// Exercises conditions with FusionNumericTextBox: simple Gt, ElseIf ladder, AND compound,
/// and cross-component source-vs-source comparison.
/// Senior living domain: vital sign thresholds.
/// </summary>
public class NumericConditionModel
{
    public decimal HeartRate { get; set; }
    public decimal BloodPressure { get; set; }
    public decimal ThresholdValue { get; set; }
}
