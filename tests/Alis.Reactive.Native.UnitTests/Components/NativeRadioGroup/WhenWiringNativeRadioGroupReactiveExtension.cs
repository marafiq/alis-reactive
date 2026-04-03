using Alis.Reactive.Builders;
using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

/// <summary>
/// Tests the .Reactive() wiring path end-to-end.
/// NativeRadioGroup.Reactive() creates N workflows (one per radio option),
/// each with its own object-event subscription and auto-sync action.
/// </summary>
[TestFixture]
public class WhenWiringNativeRadioGroupReactiveExtension : NativeTestBase
{
    [Test]
    public Task Component_event_trigger_produces_valid_plan()
    {
        var plan = CreatePlan();
        WireObjectEvent(
            plan,
            "MobilityLevel_r0",
            "native",
            "MobilityLevel",
            "value",
            NativeRadioGroupEvents.Instance.Changed.EventName,
            pb => pb.Element("echo").SetText("Mobility changed"));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Component_event_trigger_with_condition()
    {
        var plan = CreatePlan();
        var reactiveEvent = NativeRadioGroupEvents.Instance.Changed;
        WireObjectEvent(
            plan,
            "MobilityLevel_r1",
            "native",
            "MobilityLevel",
            "value",
            reactiveEvent.EventName,
            reactiveEvent.Payload,
            (args, pb) =>
            {
                pb.When(args, x => x.Value).Eq("Wheelchair")
                    .Then(then => then.Element("panel").Show())
                    .Else(else_ => else_.Element("panel").Hide());
            });

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Multiple_radio_workflows_produce_valid_plan()
    {
        var plan = CreatePlan();
        // Simulate 3 radio options producing 3 workflows
        for (int i = 0; i < 3; i++)
        {
            WireObjectEvent(
                plan,
                $"MobilityLevel_r{i}",
                "native",
                "MobilityLevel",
                "value",
                NativeRadioGroupEvents.Instance.Changed.EventName,
                pb => pb.Element("echo").SetText("Option selected"));
        }

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
