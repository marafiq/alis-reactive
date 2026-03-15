using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests.Components.NativeDatePicker;

[TestFixture]
public class WhenMutatingANativeDatePicker : NativeTestBase
{
    [Test]
    public Task SetValue_produces_dom_mutation()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<Native.Components.NativeDatePicker>(m => m.AdmissionDate)
                .SetValue(new DateTime(2026, 3, 15)));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task FocusIn_produces_dom_call()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<Native.Components.NativeDatePicker>(m => m.AdmissionDate)
                .FocusIn());
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
