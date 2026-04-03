using System.Text.Json;

namespace Alis.Reactive.UnitTests;

[TestFixture]
public class WhenGatheringRegisteredComponents : PlanTestBase
{
    [Test]
    public Task IncludeAll_expands_registered_components()
    {
        var plan = CreatePlan();
        plan.RegisterComponent("Name", new ComponentRegistration("comp1", "native", "Name", "value", "textbox", "string"));
        plan.RegisterComponent("Amount", new ComponentRegistration("comp2", "fusion", "Amount", "value", "numerictextbox", "number"));

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
        plan.RegisterComponent("Status", new ComponentRegistration("comp1", "native", "Status", "value", "textbox", "string"));

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
        plan.RegisterComponent("Name", new ComponentRegistration("comp1", "native", "Name", "value", "textbox", "string"));

        Trigger(plan).DomReady(p =>
            p.Post("/api/save", g => g.IncludeAll())
             .Response(r => r
                .OnSuccess(s => s.Element("x").SetText("ok"))));

        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void No_registered_components_still_includes_all_marker()
    {
        var plan = CreatePlan();

        Trigger(plan).DomReady(p =>
            p.Post("/api/save", g => g.IncludeAll())
             .Response(r => r
                .OnSuccess(s => s.Element("x").SetText("ok"))));

        var json = plan.Render();
        using var doc = JsonDocument.Parse(json);
        var input = doc.RootElement
            .GetProperty("workflows")[0]
            .GetProperty("run")
            .GetProperty("request")
            .GetProperty("input")
            .GetProperty("value");

        Assert.That(input.GetProperty("kind").GetString(), Is.EqualTo("binding-map"));
        Assert.That(input.GetProperty("include").GetString(), Is.EqualTo("all"));
    }
}
