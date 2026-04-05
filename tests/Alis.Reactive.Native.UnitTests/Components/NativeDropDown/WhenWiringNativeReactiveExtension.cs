using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

/// <summary>
/// Tests the .Reactive() wiring path end-to-end.
/// .Reactive() on the NativeDropDownBuilder creates a ComponentEventTrigger.
/// This produces a "component-event" trigger in the plan JSON — distinct from "custom-event".
///
/// Tests use TriggerBuilder to verify plan serialization + schema conformance.
/// </summary>
[TestFixture]
public class WhenWiringNativeReactiveExtension : NativeTestBase
{
    [Test]
    public Task Component_event_trigger_produces_valid_plan()
    {
        var plan = CreatePlan();

        Trigger(plan).CustomEvent<NativeDropDownChangeArgs>("status-changed", (args, p) =>
            p.Element("echo").SetText("Status changed"));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Component_event_trigger_with_condition()
    {
        var plan = CreatePlan();

        Trigger(plan).CustomEvent<NativeDropDownChangeArgs>("status-guarded", (args, p) =>
            p.When(args, x => x.Value).Eq("admin")
                .Then(then => then.Element("panel").Show())
                .Else(else_ => else_.Element("panel").Hide()));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Component_event_trigger_with_multiple_mutations()
    {
        var plan = CreatePlan();

        Trigger(plan).CustomEvent<NativeDropDownChangeArgs>("status-cascade", (args, p) =>
        {
            p.Component<NativeDropDown>(m => m.Status).SetValue("active");
            p.Component<NativeDropDown>(m => m.Category).SetValue("A");
            p.Element("echo").SetText("Both updated");
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
