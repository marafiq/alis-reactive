using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

[TestFixture]
public class WhenMutatingANativeRadioGroup : NativeTestBase
{
    [Test]
    public Task SetValue_produces_dom_mutation()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<NativeRadioGroup>(m => m.MobilityLevel).SetValue("Ambulatory"));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task FocusIn_produces_dom_call()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<NativeRadioGroup>(m => m.MobilityLevel).FocusIn());
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
            p.Component<NativeRadioGroup>(m => m.MobilityLevel).SetValue("Wheelchair");
            p.Element("echo").SetText("Mobility updated");
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
            var source = p.Component<NativeRadioGroup>(m => m.MobilityLevel).Value();
            Assert.That(source, Is.InstanceOf<Alis.Reactive.Builders.Conditions.TypedComponentSource<string>>());
        });
    }

    [Test]
    public Task String_ref_overload()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<NativeRadioGroup>("mobilityRadios").SetValue("Bedridden"));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
