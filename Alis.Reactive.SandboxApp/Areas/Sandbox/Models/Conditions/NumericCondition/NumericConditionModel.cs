namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.NumericCondition;

/// <summary>A resident's morning vital signs, and the heart rate limit her care plan sets.</summary>
public class NumericConditionModel
{
    public decimal HeartRate { get; set; }
    public decimal BloodPressure { get; set; }
    public decimal ThresholdValue { get; set; }
}
