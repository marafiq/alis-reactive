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
    public Task Value_returns_component_value_expression()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var source = p.Component<FusionRichTextEditor>(m => m.CarePlan).Value();
            Assert.That(source, Is.TypeOf<ComponentValueExpression<string>>());
            Assert.That(source.CoercionType, Is.EqualTo("string"));
            Assert.That(source.ElementCoercionType, Is.Null);
            p.Element("echo").SetText(source);
        });

        var json = plan.Render();
        Assert.That(json, Does.Contain("Alis_Reactive_Fusion_UnitTests_FusionTestModel__CarePlan"));
        Assert.That(json, Does.Contain("value"));
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
