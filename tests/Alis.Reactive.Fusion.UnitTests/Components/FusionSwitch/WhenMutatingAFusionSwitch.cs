using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenMutatingAFusionSwitch : FusionTestBase
{
    [Test]
    public Task SetChecked_true_produces_ej2_mutation()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionSwitch>(m => m.ReceiveNotifications).SetChecked(true));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task SetChecked_false_produces_ej2_mutation()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionSwitch>(m => m.ReceiveNotifications).SetChecked(false));
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
            var source = p.Component<FusionSwitch>(m => m.ReceiveNotifications).Value();
            Assert.That(source, Is.TypeOf<ComponentValueExpression<bool>>());
            Assert.That(source.CoercionType, Is.EqualTo("boolean"));
            Assert.That(source.ElementCoercionType, Is.Null);
            p.Element("echo").SetText(source);
        });

        var json = plan.Render();
        Assert.That(json, Does.Contain("Alis_Reactive_Fusion_UnitTests_FusionTestModel__ReceiveNotifications"));
        Assert.That(json, Does.Contain("checked"));
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
