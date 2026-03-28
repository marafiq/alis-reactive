namespace Alis.Reactive.UnitTests;

[TestFixture]
public class WhenDispatchingAnEvent : PlanTestBase
{
    [Test]
    public Task Event_name_flows_to_plan() =>
        VerifyJson(Build(p => p.Dispatch("user-saved")).Render());

    [Test]
    public Task Payload_flows_when_provided() =>
        VerifyJson(Build(p => p.Dispatch("saved", new TestModel { Id = "abc" })).Render());

    [Test]
    public Task Multiple_commands_in_sequence() =>
        VerifyJson(Build(p =>
        {
            p.Dispatch("first");
            p.Dispatch("second", new TestModel { Id = "x" });
        }).Render());

    private static ReactivePlan<TestModel> Build(Action<Builders.PipelineBuilder<TestModel>> pipeline)
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(pipeline);
        return plan;
    }
}
