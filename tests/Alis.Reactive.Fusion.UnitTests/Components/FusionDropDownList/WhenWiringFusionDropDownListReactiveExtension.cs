using Alis.Reactive.Builders;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

/// <summary>
/// Tests the .Reactive() wiring path end-to-end for FusionDropDownList.
/// .Reactive() on the Fusion builder creates an object-event workflow, not a document event.
///
/// Since DropDownListBuilder requires SF infrastructure, we test via the same
/// plan primitives the extension method produces: workflow subscription + pipeline authoring.
/// </summary>
[TestFixture]
public class WhenWiringFusionDropDownListReactiveExtension : FusionTestBase
{
    [Test]
    public Task Component_event_trigger_produces_valid_plan()
    {
        var plan = CreatePlan();
        WireObjectEvent(
            plan,
            "Status",
            "fusion",
            "Status",
            "value",
            FusionDropDownListEvents.Instance.Changed.EventName,
            pb => pb.Element("echo").SetText("Status changed"));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Component_event_trigger_with_condition()
    {
        var plan = CreatePlan();
        var reactiveEvent = FusionDropDownListEvents.Instance.Changed;
        WireObjectEvent(
            plan,
            "Status",
            "fusion",
            "Status",
            "value",
            reactiveEvent.EventName,
            reactiveEvent.Payload,
            (args, pb) =>
            {
                pb.When(args, x => x.Value).Eq("US")
                    .Then(then => then.Component<FusionDropDownList>(m => m.Status).SetValue("US"))
                    .Else(else_ => else_.Element("echo").SetText("Not US"));
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
            "Status",
            "fusion",
            "Status",
            "value",
            FusionDropDownListEvents.Instance.Changed.EventName,
            pb =>
            {
                pb.Component<FusionDropDownList>(m => m.Status).SetValue(null);
                pb.Element("echo").SetText("Reset");
            });

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
