using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests.Components.NativeDatePicker;

[TestFixture]
public class WhenReactingToNativeDatePickerEvents : NativeTestBase
{
    [Test]
    public Task Changed_event_wires_component_trigger()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<NativeDatePickerChangeArgs>("date-changed",
            (args, p) =>
                p.Component<Native.Components.NativeDatePicker>(m => m.AdmissionDate)
                    .SetValue("2026-01-01"));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Changed_event_with_condition()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<NativeDatePickerChangeArgs>("date-guarded",
            (args, p) =>
                p.When(args, x => x.Value).NotNull()
                    .Then(then => then.Element("status").SetText("date selected"))
                    .Else(else_ => else_.Element("status").SetText("no date")));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
