using Alis.Reactive.Builders;
using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

/// <summary>
/// Tests the .Reactive() wiring path end-to-end.
/// .Reactive() on the NativeDropDownBuilder creates an object-event workflow.
///
/// Tests construct the same workflow subscription directly to verify plan serialization
/// and schema conformance.
/// </summary>
[TestFixture]
public class WhenWiringNativeReactiveExtension : NativeTestBase
{
    [Test]
    public Task Component_event_trigger_produces_valid_plan()
    {
        var plan = CreatePlan();
        WireObjectEvent(
            plan,
            "Status",
            "native",
            "Status",
            "value",
            NativeDropDownEvents.Instance.Changed.EventName,
            pb => pb.Element("echo").SetText("Status changed"));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Component_event_trigger_with_condition()
    {
        var plan = CreatePlan();
        var reactiveEvent = NativeDropDownEvents.Instance.Changed;
        WireObjectEvent(
            plan,
            "Status",
            "native",
            "Status",
            "value",
            reactiveEvent.EventName,
            reactiveEvent.Payload,
            (args, pb) =>
            {
                pb.When(args, x => x.Value).Eq("admin")
                    .Then(then => then.Element("panel").Show())
                    .Else(else_ => else_.Element("panel").Hide());
            });

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Component_event_trigger_with_multiple_mutations()
    {
        var plan = CreatePlan();
        WireObjectEvent(
            plan,
            "Status",
            "native",
            "Status",
            "value",
            NativeDropDownEvents.Instance.Changed.EventName,
            pb =>
            {
                pb.Component<NativeDropDown>(m => m.Status).SetValue("active");
                pb.Component<NativeDropDown>(m => m.Category).SetValue("A");
                pb.Element("echo").SetText("Both updated");
            });

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
