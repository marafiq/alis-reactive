using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenMutatingAFusionNumericTextBox : FusionTestBase
{
    [Test]
    public Task SetValue_produces_ej2_mutation()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionNumericTextBox>(m => m.Amount).SetValue(42));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task FocusIn_produces_ej2_call()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionNumericTextBox>(m => m.Amount).FocusIn());
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
            p.Component<FusionNumericTextBox>(m => m.Amount).SetValue(100);
            p.Element("echo").SetText("Amount updated");
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
            var source = p.Component<FusionNumericTextBox>(m => m.Amount).Value();
            Assert.That(source, Is.TypeOf<ComponentValueExpression<decimal>>());
            Assert.That(source.CoercionType, Is.EqualTo("number"));
            Assert.That(source.ElementCoercionType, Is.Null);
            p.Element("echo").SetText(source);
        });

        var json = plan.Render();
        Assert.That(json, Does.Contain("Alis_Reactive_Fusion_UnitTests_FusionTestModel__Amount"));
        Assert.That(json, Does.Contain("value"));
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
