using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Sources;
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
    public void Value_returns_typed_component_source()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var source = p.Component<FusionDateTimePicker>(m => m.AppointmentTime).Value();
            Assert.That(source, Is.TypeOf<TypedComponentSource<DateTime>>());

            var bindSource = source.ToBindSource();
            Assert.That(bindSource, Is.TypeOf<ComponentSource>());

            var cs = (ComponentSource)bindSource;
            Assert.That(cs.ComponentId, Is.EqualTo("Alis_Reactive_Fusion_UnitTests_FusionTestModel__AppointmentTime"));
            Assert.That(cs.Vendor, Is.EqualTo("fusion"));
            Assert.That(cs.ReadExpr, Is.EqualTo("value"));
        });
    }
}
