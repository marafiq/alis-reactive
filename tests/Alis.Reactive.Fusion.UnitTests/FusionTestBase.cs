using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.UnitTests;

public class FusionTestAddress
{
    public decimal PostalCode { get; set; }
    public string? City { get; set; }
}

public class FusionTestModel
{
    public decimal Amount { get; set; }
    public string? Status { get; set; }
    public FusionTestAddress? Address { get; set; }
    public DateTime? AppointmentTime { get; set; }
    public DateTime[]? StayPeriod { get; set; }
    public string? PhoneNumber { get; set; }
    public string? CarePlan { get; set; }
    public bool ReceiveNotifications { get; set; }
    public string? Documents { get; set; }
}

[TestFixture]
public abstract class FusionTestBase
{
    protected static ReactivePlan<FusionTestModel> CreatePlan() => new();

    protected static TriggerBuilder<FusionTestModel> Trigger(ReactivePlan<FusionTestModel> plan) => new(plan, plan.Context);
}
