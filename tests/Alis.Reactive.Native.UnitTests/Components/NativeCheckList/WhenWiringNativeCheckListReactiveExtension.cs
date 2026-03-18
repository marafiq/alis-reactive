using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

/// <summary>
/// Tests the .Reactive() wiring path end-to-end.
/// NativeCheckList.Reactive() creates ONE entry targeting the hidden input's change event.
/// checklist.ts syncs checkbox values into the hidden input and dispatches change.
/// </summary>
[TestFixture]
public class WhenWiringNativeCheckListReactiveExtension : NativeTestBase
{
    [Test]
    public Task Component_event_trigger_produces_valid_plan()
    {
        var plan = CreatePlan();
        var descriptor = NativeCheckListEvents.Instance.Changed;

        var pb = new PipelineBuilder<NativeTestModel>();
        pb.Element("echo").SetText("Allergies changed");

        // Single entry on hidden input (not individual checkboxes)
        var trigger = new ComponentEventTrigger("Allergies", descriptor.JsEvent, "native", "Allergies", "value");
        plan.AddEntry(new Entry(trigger, pb.BuildReaction()));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Component_event_trigger_with_condition()
    {
        var plan = CreatePlan();
        var descriptor = NativeCheckListEvents.Instance.Changed;
        var args = descriptor.Args;

        var pb = new PipelineBuilder<NativeTestModel>();
        pb.When(args, x => x.Value).NotEmpty()
            .Then(then => then.Element("panel").Show())
            .Else(else_ => else_.Element("panel").Hide());

        var trigger = new ComponentEventTrigger("Allergies", descriptor.JsEvent, "native", "Allergies", "value");
        plan.AddEntry(new Entry(trigger, pb.BuildReaction()));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Single_entry_regardless_of_option_count()
    {
        var plan = CreatePlan();
        var descriptor = NativeCheckListEvents.Instance.Changed;

        // Even with many options, only 1 entry on the hidden input
        var pb = new PipelineBuilder<NativeTestModel>();
        pb.Element("echo").SetText("Option toggled");

        var trigger = new ComponentEventTrigger("Allergies", descriptor.JsEvent, "native", "Allergies", "value");
        plan.AddEntry(new Entry(trigger, pb.BuildReaction()));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
