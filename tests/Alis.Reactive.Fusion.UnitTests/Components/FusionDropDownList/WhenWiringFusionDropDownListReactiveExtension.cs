using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

/// <summary>
/// Tests the .Reactive() wiring path end-to-end for FusionDropDownList.
/// .Reactive() on the SF builder creates a ComponentEventTrigger (not CustomEventTrigger).
/// This produces a "component-event" trigger in the plan JSON — a distinct code path.
///
/// Since DropDownListBuilder requires SF infrastructure, we test via the same
/// plan primitives the extension method produces: ComponentEventTrigger + PipelineBuilder.
/// </summary>
[TestFixture]
public class WhenWiringFusionDropDownListReactiveExtension : FusionTestBase
{
    [Test]
    public Task Component_event_trigger_produces_valid_plan()
    {
        var plan = CreatePlan();
        var pb = new PipelineBuilder<FusionTestModel>();

        // Simulate what .Reactive() does: creates args, builds pipeline, wires trigger
        var descriptor = FusionDropDownListEvents.Instance.Changed;
        var args = descriptor.Args;
        pb.Element("echo").SetText("Status changed");

        var trigger = new ComponentEventTrigger("Status", descriptor.JsEvent, "fusion", "Status", "value");
        var entry = new Entry(trigger, pb.BuildReaction());
        plan.AddEntry(entry);

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Component_event_trigger_with_condition()
    {
        var plan = CreatePlan();
        var descriptor = FusionDropDownListEvents.Instance.Changed;
        var args = descriptor.Args;

        var pb = new PipelineBuilder<FusionTestModel>();
        pb.When(args, x => x.Value).Eq("US")
            .Then(then => then.Component<FusionDropDownList>(m => m.Status).SetValue("US"))
            .Else(else_ => else_.Element("echo").SetText("Not US"));

        var trigger = new ComponentEventTrigger("Status", descriptor.JsEvent, "fusion", "Status", "value");
        plan.AddEntry(new Entry(trigger, pb.BuildReaction()));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Component_event_trigger_with_cross_vendor_mutations()
    {
        var plan = CreatePlan();
        var descriptor = FusionDropDownListEvents.Instance.Changed;

        var pb = new PipelineBuilder<FusionTestModel>();
        pb.Component<FusionDropDownList>(m => m.Status).SetValue(null);
        pb.Element("echo").SetText("Reset");

        var trigger = new ComponentEventTrigger("Status", descriptor.JsEvent, "fusion", "Status", "value");
        plan.AddEntry(new Entry(trigger, pb.BuildReaction()));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
