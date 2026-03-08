using static VerifyNUnit.Verifier;

namespace Alis.Reactive.UnitTests;

[TestFixture]
public class WhenComposingAPlan : PlanTestBase
{
    [Test]
    public Task Empty_plan() =>
        VerifyJson(CreatePlan().Render());

    [Test]
    public Task Multiple_entries()
    {
        var plan = CreatePlan();
        Trigger(plan)
            .DomReady(p => p.Dispatch("init"))
            .CustomEvent("refresh", p => p.Dispatch("reload"))
            .CustomEvent<TestModel>("typed", (_, p) => p.Dispatch("done"));
        return VerifyJson(plan.Render());
    }

    [Test]
    public Task Empty_pipeline()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(_ => { });
        return VerifyJson(plan.Render());
    }
}
