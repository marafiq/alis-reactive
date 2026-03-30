using Alis.Reactive.DriftDetection.Tests.Infrastructure;
using Alis.Reactive.Fusion.Components;
using Alis.Reactive.Native.Components;

namespace Alis.Reactive.DriftDetection.Tests.Mutations;

[TestFixture]
public class WhenDetectingMutationSchemaDrift : DriftTestBase
{
    [Test]
    public void set_prop_conforms()
    {
        // SetPropMutation: kind, prop
        // SetText produces set-prop with prop="textContent"
        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
            p.Element("welcome").SetText("Hello!")));

        var json = plan.Render();
        AssertSchemaValid(json);
    }

    [Test]
    public void set_prop_with_coerce_conforms()
    {
        // SetPropMutation: kind, prop, coerce
        // FusionNumericTextBox.SetValue produces set-prop with coerce="number"
        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
        {
            var rate = p.Component<FusionNumericTextBox>(m => m.MonthlyRate);
            rate.SetValue(3200m);
        }));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "SetPropMutation",
            "entries[0].reaction.commands[0].mutation");
    }

    [Test]
    public void call_conforms()
    {
        // CallMutation minimal: kind, method
        // FocusIn produces call with just method="focus" (no chain, no args)
        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
        {
            var nameComp = p.Component<NativeTextBox>(m => m.Name);
            nameComp.FocusIn();
        }));

        var json = plan.Render();
        AssertSchemaValid(json);
    }

    [Test]
    public void call_with_all_properties_conforms()
    {
        // CallMutation: kind, method, chain, args
        // AddClass produces method="add", chain="classList", args=[LiteralArg]
        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
            p.Element("panel").AddClass("active")));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "CallMutation",
            "entries[0].reaction.commands[0].mutation");
    }

    [Test]
    public void event_source_conforms()
    {
        // EventSource: kind, path
        // SetText from event payload produces an EventSource
        var plan = CreatePlan();
        On(plan, t => t.CustomEvent<ResidentModel>("updated", (args, p) =>
            p.Element("name").SetText(args, x => x.Name!)));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "EventSource",
            "entries[0].reaction.commands[0].source");
    }

    [Test]
    public void component_source_conforms()
    {
        // ComponentSource: kind, componentId, vendor, readExpr
        // SetText from component Value() produces a ComponentSource
        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
        {
            var nameComp = p.Component<NativeTextBox>(m => m.Name);
            p.Element("name-echo").SetText(nameComp.Value());
        }));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "ComponentSource",
            "entries[0].reaction.commands[0].source");
    }
}
