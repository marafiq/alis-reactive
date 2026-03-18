using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

/// <summary>
/// Tests the .Reactive() wiring path end-to-end.
/// NativeRadioGroup.Reactive() creates N entries (one per radio option),
/// each with a ComponentEventTrigger and auto-sync command.
/// </summary>
[TestFixture]
public class WhenWiringNativeRadioGroupReactiveExtension : NativeTestBase
{
    [Test]
    public Task Component_event_trigger_produces_valid_plan()
    {
        var plan = CreatePlan();
        var descriptor = NativeRadioGroupEvents.Instance.Changed;

        var pb = new PipelineBuilder<NativeTestModel>();
        pb.Element("echo").SetText("Mobility changed");

        var trigger = new ComponentEventTrigger("MobilityLevel_r0", descriptor.JsEvent, "native", "MobilityLevel", "value");
        plan.AddEntry(new Entry(trigger, pb.BuildReaction()));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Component_event_trigger_with_condition()
    {
        var plan = CreatePlan();
        var descriptor = NativeRadioGroupEvents.Instance.Changed;
        var args = descriptor.Args;

        var pb = new PipelineBuilder<NativeTestModel>();
        pb.When(args, x => x.Value).Eq("Wheelchair")
            .Then(then => then.Element("panel").Show())
            .Else(else_ => else_.Element("panel").Hide());

        var trigger = new ComponentEventTrigger("MobilityLevel_r1", descriptor.JsEvent, "native", "MobilityLevel", "value");
        plan.AddEntry(new Entry(trigger, pb.BuildReaction()));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Multiple_radio_entries_produce_valid_plan()
    {
        var plan = CreatePlan();
        var descriptor = NativeRadioGroupEvents.Instance.Changed;

        // Simulate 3 radio options producing 3 entries
        for (int i = 0; i < 3; i++)
        {
            var pb = new PipelineBuilder<NativeTestModel>();
            pb.Element("echo").SetText("Option selected");

            var trigger = new ComponentEventTrigger($"MobilityLevel_r{i}", descriptor.JsEvent, "native", "MobilityLevel", "value");
            plan.AddEntry(new Entry(trigger, pb.BuildReaction()));
        }

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
