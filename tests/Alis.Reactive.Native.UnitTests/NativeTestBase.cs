using Alis.Reactive.Builders;

namespace Alis.Reactive.Native.UnitTests;

public class NativeTestAddress
{
    public string? City { get; set; }
    public string? State { get; set; }
}

public class NativeTestModel
{
    public string? Status { get; set; }
    public string? Category { get; set; }
    public NativeTestAddress? Address { get; set; }
    public string? MobilityLevel { get; set; }
    public string[]? Allergies { get; set; }
    public string? CareNotes { get; set; }
    public string? ResidentId { get; set; }
}

[TestFixture]
public abstract class NativeTestBase
{
    protected static ReactivePlan<NativeTestModel> CreatePlan() => new();

    protected static TriggerBuilder<NativeTestModel> Trigger(ReactivePlan<NativeTestModel> plan) => new(plan, plan.Context);
}
