using System.Text.Json;
using static VerifyNUnit.Verifier;

namespace Alis.Reactive.UnitTests;

[TestFixture]
public class WhenGatheringRegisteredComponents : PlanTestBase
{
    [Test]
    public Task IncludeAll_expands_registered_components()
    {
        var plan = CreatePlan();
        plan.RegisterComponent("comp1", "native", "Name", "value");
        plan.RegisterComponent("comp2", "fusion", "Amount", "value");

        Trigger(plan).DomReady(p =>
            p.Post("/api/save", g => g.IncludeAll())
             .Response(r => r
                .OnSuccess(s => s.Element("result").SetText("saved"))
             )
        );

        return VerifyJson(plan.Render());
    }

    [Test]
    public Task IncludeAll_with_static_mixes_both()
    {
        var plan = CreatePlan();
        plan.RegisterComponent("comp1", "native", "Status", "value");

        Trigger(plan).DomReady(p =>
            p.Post("/api/save", g => g
                .Static("extra", "fixed")
                .IncludeAll())
             .Response(r => r
                .OnSuccess(s => s.Element("result").SetText("saved"))
             )
        );

        return VerifyJson(plan.Render());
    }

    [Test]
    public void IncludeAll_expands_conforms_to_schema()
    {
        var plan = CreatePlan();
        plan.RegisterComponent("comp1", "native", "Name", "value");

        Trigger(plan).DomReady(p =>
            p.Post("/api/save", g => g.IncludeAll())
             .Response(r => r
                .OnSuccess(s => s.Element("x").SetText("ok"))));

        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void FormId_IncludeAll_is_unaffected_by_registry()
    {
        var plan = CreatePlan();
        plan.RegisterComponent("comp1", "native", "Name", "value");

        Trigger(plan).DomReady(p =>
            p.Post("/api/save", g => g.IncludeAll("my-form"))
             .Response(r => r
                .OnSuccess(s => s.Element("x").SetText("ok"))));

        var json = plan.Render();
        using var doc = JsonDocument.Parse(json);
        var gather = doc.RootElement
            .GetProperty("entries")[0]
            .GetProperty("reaction")
            .GetProperty("request")
            .GetProperty("gather");

        Assert.That(gather.GetArrayLength(), Is.EqualTo(1));
        Assert.That(gather[0].GetProperty("kind").GetString(), Is.EqualTo("all"));
        Assert.That(gather[0].GetProperty("formId").GetString(), Is.EqualTo("my-form"));
    }

    [Test]
    public void No_registered_components_produces_empty_gather()
    {
        var plan = CreatePlan();

        Trigger(plan).DomReady(p =>
            p.Post("/api/save", g => g.IncludeAll())
             .Response(r => r
                .OnSuccess(s => s.Element("x").SetText("ok"))));

        var json = plan.Render();
        using var doc = JsonDocument.Parse(json);
        var gather = doc.RootElement
            .GetProperty("entries")[0]
            .GetProperty("reaction")
            .GetProperty("request")
            .GetProperty("gather");

        Assert.That(gather.GetArrayLength(), Is.EqualTo(0));
    }
}
