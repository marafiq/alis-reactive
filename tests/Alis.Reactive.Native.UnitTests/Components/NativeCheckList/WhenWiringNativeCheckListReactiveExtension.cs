using Alis.Reactive.Builders;
using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

/// <summary>
/// Tests the .Reactive() wiring path end-to-end.
/// NativeCheckList.Reactive() creates one workflow targeting the checklist root change event.
/// The checklist root owns the canonical array value exposed to gather, conditions, and event payloads.
/// </summary>
[TestFixture]
public class WhenWiringNativeCheckListReactiveExtension : NativeTestBase
{
    [Test]
    public Task Component_event_trigger_produces_valid_plan()
    {
        var plan = CreatePlan();
        WireObjectEvent(
            plan,
            "Allergies",
            "native",
            "Allergies",
            "value",
            NativeCheckListEvents.Instance.Changed.EventName,
            pb => pb.Element("echo").SetText("Allergies changed"));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Component_event_trigger_with_condition()
    {
        var plan = CreatePlan();
        var reactiveEvent = NativeCheckListEvents.Instance.Changed;
        WireObjectEvent(
            plan,
            "Allergies",
            "native",
            "Allergies",
            "value",
            reactiveEvent.EventName,
            reactiveEvent.Payload,
            (args, pb) =>
            {
                pb.When(args, x => x.Value).NotEmpty()
                    .Then(then => then.Element("panel").Show())
                    .Else(else_ => else_.Element("panel").Hide());
            });

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Single_workflow_regardless_of_option_count()
    {
        var plan = CreatePlan();
        WireObjectEvent(
            plan,
            "Allergies",
            "native",
            "Allergies",
            "value",
            NativeCheckListEvents.Instance.Changed.EventName,
            pb => pb.Element("echo").SetText("Option toggled"));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
