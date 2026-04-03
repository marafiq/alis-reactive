using Alis.Reactive.Builders;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

/// <summary>
/// Tests the .Reactive() wiring path end-to-end.
/// .Reactive() on the Fusion builder creates an object-event workflow, not a document event.
///
/// Since NumericTextBoxBuilder requires SF infrastructure, we test via the same
/// plan primitives the extension method produces: workflow subscription + pipeline authoring.
/// </summary>
[TestFixture]
public class WhenWiringFusionReactiveExtension : FusionTestBase
{
    [Test]
    public Task Component_event_trigger_produces_valid_plan()
    {
        var plan = CreatePlan();
        WireObjectEvent(
            plan,
            "Amount",
            "fusion",
            "Amount",
            "value",
            FusionNumericTextBoxEvents.Instance.Changed.EventName,
            pb => pb.Element("echo").SetText("Amount changed"));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Component_event_trigger_with_condition()
    {
        var plan = CreatePlan();
        var reactiveEvent = FusionNumericTextBoxEvents.Instance.Changed;
        WireObjectEvent(
            plan,
            "Amount",
            "fusion",
            "Amount",
            "value",
            reactiveEvent.EventName,
            reactiveEvent.Payload,
            (args, pb) =>
            {
                pb.When(args, x => x.Value).Gte(100m)
                    .Then(then => then.Component<FusionNumericTextBox>(m => m.Amount).SetValue(100))
                    .Else(else_ => else_.Element("echo").SetText("Under limit"));
            });

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Component_event_trigger_with_cross_vendor_mutations()
    {
        var plan = CreatePlan();
        WireObjectEvent(
            plan,
            "Amount",
            "fusion",
            "Amount",
            "value",
            FusionNumericTextBoxEvents.Instance.Changed.EventName,
            pb =>
            {
                pb.Component<FusionNumericTextBox>(m => m.Amount).SetValue(0);
                pb.Element("echo").SetText("Reset");
            });

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
