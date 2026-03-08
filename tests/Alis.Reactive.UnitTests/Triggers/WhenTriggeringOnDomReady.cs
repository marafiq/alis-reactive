using static VerifyNUnit.Verifier;

namespace Alis.Reactive.UnitTests;

[TestFixture]
public class WhenTriggeringOnDomReady : PlanTestBase
{
    [Test]
    public Task Plan_matches_expected()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p => p.Dispatch("init"));
        return VerifyJson(plan.Render());
    }
}
