using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

[TestFixture]
public class WhenReactingToNativeRadioGroupEvents : NativeTestBase
{
    [Test]
    public Task Changed_event_wires_component_trigger()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<NativeRadioGroupChangeArgs>("mobility-changed", (args, p) =>
            p.Component<NativeRadioGroup>(m => m.MobilityLevel).SetValue("Ambulatory"));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Changed_event_with_condition()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<NativeRadioGroupChangeArgs>("mobility-guarded", (args, p) =>
            p.When(args, x => x.Value).Eq("Wheelchair")
                .Then(then => then.Element("panel").Show())
                .Else(else_ => else_.Element("panel").Hide()));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Changed_event_with_cross_component_mutation()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<NativeRadioGroupChangeArgs>("mobility-cascade", (args, p) =>
        {
            p.Component<NativeRadioGroup>(m => m.MobilityLevel).SetValue("Ambulatory");
            p.Component<NativeRadioGroup>(m => m.Status).SetValue("Active");
            p.Element("echo").SetText("Both updated");
        });
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
