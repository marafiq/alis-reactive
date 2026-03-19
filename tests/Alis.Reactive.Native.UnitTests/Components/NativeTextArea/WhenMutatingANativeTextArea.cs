using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

[TestFixture]
public class WhenMutatingANativeTextArea : NativeTestBase
{
    [Test]
    public Task SetValue_produces_dom_mutation()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<NativeTextArea>(m => m.CareNotes).SetValue("Patient stable"));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task FocusIn_produces_dom_call()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<NativeTextArea>(m => m.CareNotes).FocusIn());
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
            var source = p.Component<NativeTextArea>(m => m.CareNotes).Value();
            Assert.That(source, Is.InstanceOf<Alis.Reactive.Builders.Conditions.TypedComponentSource<string>>());
        });
    }
}
