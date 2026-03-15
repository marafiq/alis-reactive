using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

/// <summary>
/// Tests the .Reactive() wiring path end-to-end.
/// .Reactive() on the NativeDropDownBuilder creates a ComponentEventTrigger.
/// This produces a "component-event" trigger in the plan JSON — distinct from "custom-event".
///
/// Tests construct ComponentEventTrigger directly (same as what .Reactive() produces)
/// to verify plan serialization + schema conformance.
/// </summary>
[TestFixture]
public class WhenWiringNativeReactiveExtension : NativeTestBase
{
    [Test]
    public Task Component_event_trigger_produces_valid_plan()
    {
        var plan = CreatePlan();
        var descriptor = NativeDropDownEvents.Instance.Changed;

        var pb = new PipelineBuilder<NativeTestModel>();
        pb.Element("echo").SetText("Status changed");

        var trigger = new ComponentEventTrigger("Status", descriptor.JsEvent, "native", "Status", "value");
        plan.AddEntry(new Entry(trigger, pb.BuildReaction()));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Component_event_trigger_with_condition()
    {
        var plan = CreatePlan();
        var descriptor = NativeDropDownEvents.Instance.Changed;
        var args = descriptor.Args;

        var pb = new PipelineBuilder<NativeTestModel>();
        pb.When(args, x => x.Value).Eq("admin")
            .Then(then => then.Element("panel").Show())
            .Else(else_ => else_.Element("panel").Hide());

        var trigger = new ComponentEventTrigger("Status", descriptor.JsEvent, "native", "Status", "value");
        plan.AddEntry(new Entry(trigger, pb.BuildReaction()));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Component_event_trigger_with_multiple_mutations()
    {
        var plan = CreatePlan();
        var descriptor = NativeDropDownEvents.Instance.Changed;

        var pb = new PipelineBuilder<NativeTestModel>();
        pb.Component<NativeDropDown>(m => m.Status).SetValue("active");
        pb.Component<NativeDropDown>(m => m.Category).SetValue("A");
        pb.Element("echo").SetText("Both updated");

        var trigger = new ComponentEventTrigger("Status", descriptor.JsEvent, "native", "Status", "value");
        plan.AddEntry(new Entry(trigger, pb.BuildReaction()));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
