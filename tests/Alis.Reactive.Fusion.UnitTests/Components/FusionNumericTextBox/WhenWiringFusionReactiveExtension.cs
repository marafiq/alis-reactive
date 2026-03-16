using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

/// <summary>
/// Tests the .Reactive() wiring path end-to-end.
/// .Reactive() on the SF builder creates a ComponentEventTrigger (not CustomEventTrigger).
/// This produces a "component-event" trigger in the plan JSON — a distinct code path.
///
/// Since NumericTextBoxBuilder requires SF infrastructure, we test via the same
/// plan primitives the extension method produces: ComponentEventTrigger + PipelineBuilder.
/// </summary>
[TestFixture]
public class WhenWiringFusionReactiveExtension : FusionTestBase
{
    [Test]
    public Task Component_event_trigger_produces_valid_plan()
    {
        var plan = CreatePlan();
        var pb = new PipelineBuilder<FusionTestModel>();

        // Simulate what .Reactive() does: creates args, builds pipeline, wires trigger
        var descriptor = FusionNumericTextBoxEvents.Instance.Changed;
        var args = descriptor.Args;
        pb.Element("echo").SetText("Amount changed");

        var trigger = new ComponentEventTrigger("Amount", descriptor.JsEvent, "fusion", "Amount", "value");
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
        var descriptor = FusionNumericTextBoxEvents.Instance.Changed;
        var args = descriptor.Args;

        var pb = new PipelineBuilder<FusionTestModel>();
        pb.When(args, x => x.Value).Gte(100m)
            .Then(then => then.Component<FusionNumericTextBox>(m => m.Amount).SetValue(100))
            .Else(else_ => else_.Element("echo").SetText("Under limit"));

        var trigger = new ComponentEventTrigger("Amount", descriptor.JsEvent, "fusion", "Amount", "value");
        plan.AddEntry(new Entry(trigger, pb.BuildReaction()));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Component_event_trigger_with_cross_vendor_mutations()
    {
        var plan = CreatePlan();
        var descriptor = FusionNumericTextBoxEvents.Instance.Changed;

        var pb = new PipelineBuilder<FusionTestModel>();
        pb.Component<FusionNumericTextBox>(m => m.Amount).SetValue(0);
        pb.Element("echo").SetText("Reset");

        var trigger = new ComponentEventTrigger("Amount", descriptor.JsEvent, "fusion", "Amount", "value");
        plan.AddEntry(new Entry(trigger, pb.BuildReaction()));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
