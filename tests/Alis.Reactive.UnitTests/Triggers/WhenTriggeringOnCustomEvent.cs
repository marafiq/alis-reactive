namespace Alis.Reactive.UnitTests;

[TestFixture]
public class WhenTriggeringOnCustomEvent : PlanTestBase
{
    [Test]
    public Task Event_name_is_wired()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent("data-loaded", p => p.Dispatch("refresh"));
        return VerifyJson(plan.Render());
    }

    [Test]
    public Task Typed_payload_access()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<TestModel>("test", (payload, p) =>
        {
            p.Dispatch("output");
            p.Dispatch("typed", new TestModel { Id = "abc" });
        });
        return VerifyJson(plan.Render());
    }
}
