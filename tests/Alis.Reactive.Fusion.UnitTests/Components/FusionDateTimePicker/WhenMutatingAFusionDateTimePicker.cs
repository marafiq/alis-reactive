using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenMutatingAFusionDateTimePicker : FusionTestBase
{
    [Test]
    public Task SetValue_produces_ej2_mutation()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionDateTimePicker>(m => m.AppointmentTime).SetValue(new DateTime(2026, 6, 15, 14, 30, 0)));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task FocusIn_produces_ej2_call()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionDateTimePicker>(m => m.AppointmentTime).FocusIn());
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
            p.Component<FusionDateTimePicker>(m => m.AppointmentTime).SetValue(new DateTime(2026, 6, 15, 9, 0, 0));
            p.Element("echo").SetText("Appointment updated");
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
            var source = p.Component<FusionDateTimePicker>(m => m.AppointmentTime).Value();
            Assert.That(source, Is.TypeOf<ComponentValueExpression<DateTime>>());
            Assert.That(source.CoercionType, Is.EqualTo("date"));
            Assert.That(source.ElementCoercionType, Is.Null);
            p.Element("echo").SetText(source);
        });

        var json = plan.Render();
        Assert.That(json, Does.Contain("Alis_Reactive_Fusion_UnitTests_FusionTestModel__AppointmentTime"));
        Assert.That(json, Does.Contain("value"));
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
