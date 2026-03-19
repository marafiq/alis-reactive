using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

[TestFixture]
public class WhenMutatingANativeHiddenField : NativeTestBase
{
    [Test]
    public Task SetValue_produces_dom_mutation()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<NativeHiddenField>(m => m.ResidentId).SetValue("RES-1042"));
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
            var source = p.Component<NativeHiddenField>(m => m.ResidentId).Value();
            Assert.That(source, Is.InstanceOf<Alis.Reactive.Builders.Conditions.TypedComponentSource<string>>());
        });
    }
}
