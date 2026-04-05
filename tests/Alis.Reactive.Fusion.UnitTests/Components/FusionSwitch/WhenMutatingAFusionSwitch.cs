using Alis.Reactive.PlanModel;
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
    public void Value_returns_typed_component_source()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var source = p.Component<FusionSwitch>(m => m.ReceiveNotifications).Value();
            Assert.That(source, Is.TypeOf<TypedComponentSource<bool>>());
        });
    }
}
