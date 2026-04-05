using Alis.Reactive.PlanModel;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenMutatingAFusionRichTextEditor : FusionTestBase
{
    [Test]
    public Task SetValue_produces_ej2_mutation()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionRichTextEditor>(m => m.CarePlan).SetValue("<p>Initial care plan notes</p>"));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task FocusIn_produces_ej2_call()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionRichTextEditor>(m => m.CarePlan).FocusIn());
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task SetValue_followed_by_element_mutation()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Component<FusionRichTextEditor>(m => m.CarePlan).SetValue("<p>Updated care plan</p>");
            p.Element("echo").SetText("Care plan updated");
        });
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public void Value_returns_typed_component_source()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var source = p.Component<FusionRichTextEditor>(m => m.CarePlan).Value();
            Assert.That(source, Is.TypeOf<TypedComponentSource<string>>());
        });
    }
}
