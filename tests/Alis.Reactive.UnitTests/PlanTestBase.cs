namespace Alis.Reactive.UnitTests;

public class TestModel
{
    public string? Id { get; set; }
}

[TestFixture]
public abstract class PlanTestBase
{
    protected static ReactivePlan<TestModel> CreatePlan() => new();

    protected static Builders.TriggerBuilder<TestModel> Trigger(ReactivePlan<TestModel> plan) => new(plan, plan.Context);
}
