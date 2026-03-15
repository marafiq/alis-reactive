namespace Alis.Reactive.UnitTests;

[TestFixture]
public class WhenExecutingAnHttpWorkflow : PlanTestBase
{
    [Test]
    public Task StandaloneHttpExecutesCorrectly()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Post("/api/init")
             .Response(r => r.OnSuccess(s => s.Element("status").SetText("Ready")))
        );
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    [Test]
    public Task UnconditionalActionsExecuteBeforeHttp()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Element("spinner").Show();
            p.Element("status").SetText("Loading...");
            p.Post("/api/load")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("spinner").Hide();
                 s.Element("status").SetText("Loaded");
             }));
        });
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }
}
