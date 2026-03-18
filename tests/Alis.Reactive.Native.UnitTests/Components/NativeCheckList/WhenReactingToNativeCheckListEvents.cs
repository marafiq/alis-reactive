using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

[TestFixture]
public class WhenReactingToNativeCheckListEvents : NativeTestBase
{
    [Test]
    public Task Changed_event_wires_component_trigger()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<NativeCheckListChangeArgs>("allergy-changed", (args, p) =>
            p.Component<NativeCheckList>(m => m.Allergies).SetValue("Peanuts,Dairy"));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Changed_event_with_condition()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<NativeCheckListChangeArgs>("allergy-guarded", (args, p) =>
            p.When(args, x => x.Value).NotEmpty()
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
        Trigger(plan).CustomEvent<NativeCheckListChangeArgs>("allergy-cascade", (args, p) =>
        {
            p.Component<NativeCheckList>(m => m.Allergies).SetValue("Peanuts");
            p.Component<NativeRadioGroup>(m => m.Status).SetValue("Active");
            p.Element("echo").SetText("Both updated");
        });
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
