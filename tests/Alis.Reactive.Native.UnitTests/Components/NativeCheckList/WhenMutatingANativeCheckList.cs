using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

[TestFixture]
public class WhenMutatingANativeCheckList : NativeTestBase
{
    [Test]
    public Task SetValue_produces_dom_mutation()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<NativeCheckList>(m => m.Allergies).SetValue(new[] { "Peanuts", "Dairy" }));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task FocusIn_produces_dom_call()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<NativeCheckList>(m => m.Allergies).FocusIn());
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
            p.Component<NativeCheckList>(m => m.Allergies).SetValue(new[] { "Shellfish", "Gluten" });
            p.Element("echo").SetText("Allergies updated");
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
            var source = p.Component<NativeCheckList>(m => m.Allergies).Value();
            Assert.That(source, Is.InstanceOf<Alis.Reactive.Builders.Conditions.TypedComponentSource<string[]>>());
        });
    }

    [Test]
    public Task SetValue_with_single_item_array()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<NativeCheckList>(m => m.Allergies).SetValue(new[] { "Dairy" }));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
