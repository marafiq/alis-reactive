using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

/// <summary>
/// Tests the .Reactive() wiring path end-to-end.
/// NativeRadioGroup.Reactive() creates entries for radio option change events.
/// </summary>
[TestFixture]
public class WhenWiringNativeRadioGroupReactiveExtension : NativeTestBase
{
    [Test]
    public Task Component_event_trigger_produces_valid_plan()
    {
        var plan = CreatePlan();

        Trigger(plan).CustomEvent<NativeRadioGroupChangeArgs>("mobility-changed", (args, p) =>
            p.Element("echo").SetText("Mobility changed"));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Component_event_trigger_with_condition()
    {
        var plan = CreatePlan();

        Trigger(plan).CustomEvent<NativeRadioGroupChangeArgs>("mobility-guarded", (args, p) =>
            p.When(args, x => x.Value).Eq("Wheelchair")
                .Then(then => then.Element("panel").Show())
                .Else(else_ => else_.Element("panel").Hide()));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Multiple_radio_entries_produce_valid_plan()
    {
        var plan = CreatePlan();

        // Multiple triggers simulating multiple radio option changes
        Trigger(plan).CustomEvent<NativeRadioGroupChangeArgs>("option-0", (args, p) =>
            p.Element("echo").SetText("Option selected"));
        Trigger(plan).CustomEvent<NativeRadioGroupChangeArgs>("option-1", (args, p) =>
            p.Element("echo").SetText("Option selected"));
        Trigger(plan).CustomEvent<NativeRadioGroupChangeArgs>("option-2", (args, p) =>
            p.Element("echo").SetText("Option selected"));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
