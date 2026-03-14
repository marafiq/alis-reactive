using Alis.Reactive;
using Alis.Reactive.Native.Components;
using static VerifyNUnit.Verifier;

namespace Alis.Reactive.Native.UnitTests;

[TestFixture]
public class WhenMutatingANativeButton : NativeTestBase
{
    [Test]
    public Task SetText_produces_dom_mutation()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<NativeButton>("submitBtn").SetText("Save"));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task FocusIn_produces_dom_call()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<NativeButton>("submitBtn").FocusIn());
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task SetText_chained_with_dropdown()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Component<NativeButton>("submitBtn").SetText("Updated");
            p.Component<NativeDropDown>(m => m.Status).SetValue("active");
        });
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
