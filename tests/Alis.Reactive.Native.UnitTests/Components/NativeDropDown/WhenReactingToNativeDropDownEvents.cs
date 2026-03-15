using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

[TestFixture]
public class WhenReactingToNativeDropDownEvents : NativeTestBase
{
    [Test]
    public Task Changed_event_wires_component_trigger()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<NativeDropDownChangeArgs>("status-changed", (args, p) =>
            p.Component<NativeDropDown>(m => m.Status).SetValue("active"));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Changed_event_with_condition()
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
    public Task Changed_event_with_cross_component_mutation()
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
