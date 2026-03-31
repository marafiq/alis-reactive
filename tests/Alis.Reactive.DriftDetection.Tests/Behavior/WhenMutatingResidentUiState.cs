using Alis.Reactive.DriftDetection.Tests.Infrastructure;
using Alis.Reactive.Fusion.Components;
using Alis.Reactive.Native.Components;

namespace Alis.Reactive.DriftDetection.Tests.Behavior;

[TestFixture]
public class WhenMutatingResidentUiState : DriftTestBase
{
    private sealed class FilterResultsResponse
    {
        public List<string> Items { get; set; } = [];
    }

    [Test]
    public void resident_panels_can_call_methods_with_literal_arguments()
    {
        AssertDefinitionPropertiesExactly("MutateElementCommand",
            "kind", "target", "mutation", "vendor");
        AssertDefinitionPropertiesExactly("CallMutation", "kind", "method", "chain", "args");
        AssertDefinitionPropertiesExactly("LiteralValue", "kind", "value", "coerce");

        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
            p.Element("panel").AddClass("active")));

        var json = plan.Render();
        AssertSchemaValid(json);

        AssertPropertiesExactly(json, "entries[0].reaction.commands[0]", "kind", "target", "mutation");
        AssertPropertiesExactly(json, "entries[0].reaction.commands[0].mutation",
            "kind", "method", "chain", "args");
        AssertPropertiesExactly(json, "entries[0].reaction.commands[0].mutation.args[0]",
            "kind", "value");
    }

    [Test]
    public void resident_ui_text_updates_cover_literal_event_and_component_sources()
    {
        AssertDefinitionPropertiesExactly("MutateElementCommand",
            "kind", "target", "mutation", "vendor");
        AssertDefinitionPropertiesExactly("SetPropMutation", "kind", "prop", "value");
        AssertDefinitionPropertiesExactly("EventSource", "kind", "path");
        AssertDefinitionPropertiesExactly("ComponentSource",
            "kind", "componentId", "vendor", "readExpr");

        var plan = CreatePlan();
        On(plan, t => t.CustomEvent<ResidentModel>("updated", (args, p) =>
        {
            var nameComp = p.Component<NativeTextBox>(m => m.Name);

            p.Element("status").SetText("Updated");
            p.Element("name-from-event").SetText(args, x => x.Name!);
            p.Element("name-from-component").SetText(nameComp.Value());
        }));

        var json = plan.Render();
        AssertSchemaValid(json);

        AssertPropertiesExactly(json, "entries[0].reaction.commands[0]",
            "kind", "target", "mutation");
        AssertPropertiesExactly(json, "entries[0].reaction.commands[0].mutation", "kind", "prop", "value");
        AssertPropertiesExactly(json, "entries[0].reaction.commands[0].mutation.value", "kind", "value");
        AssertPropertiesExactly(json, "entries[0].reaction.commands[1]",
            "kind", "target", "mutation");
        AssertPropertiesExactly(json, "entries[0].reaction.commands[1].mutation", "kind", "prop", "value");
        AssertPropertiesExactly(json, "entries[0].reaction.commands[1].mutation.value", "kind", "source");
        AssertPropertiesExactly(json, "entries[0].reaction.commands[1].mutation.value.source", "kind", "path");
        AssertPropertiesExactly(json, "entries[0].reaction.commands[2]",
            "kind", "target", "mutation");
        AssertPropertiesExactly(json, "entries[0].reaction.commands[2].mutation", "kind", "prop", "value");
        AssertPropertiesExactly(json, "entries[0].reaction.commands[2].mutation.value", "kind", "source");
        AssertPropertiesExactly(json, "entries[0].reaction.commands[2].mutation.value.source",
            "kind", "componentId", "vendor", "readExpr");
    }

    [Test]
    public void resident_components_can_accept_typed_values_with_vendor_and_coercion()
    {
        AssertDefinitionPropertiesExactly("MutateElementCommand",
            "kind", "target", "mutation", "vendor");
        AssertDefinitionPropertiesExactly("SetPropMutation", "kind", "prop", "value");

        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
        {
            var rate = p.Component<FusionNumericTextBox>(m => m.MonthlyRate);
            rate.SetValue(3200m);
        }));

        var json = plan.Render();
        AssertSchemaValid(json);

        AssertPropertiesExactly(json, "entries[0].reaction.commands[0]",
            "kind", "target", "mutation", "vendor");
        AssertPropertiesExactly(json, "entries[0].reaction.commands[0].mutation",
            "kind", "prop", "value");
        AssertPropertiesExactly(json, "entries[0].reaction.commands[0].mutation.value",
            "kind", "value", "coerce");
    }

    [Test]
    public void resident_filtering_helpers_can_prevent_default_via_public_event_value_mutation()
    {
        AssertDefinitionPropertiesExactly("MutateEventCommand",
            "kind", "mutation");

        var plan = CreatePlan();
        On(plan, t => t.CustomEvent("filtering", p =>
        {
            var args = new FusionAutoCompleteFilteringArgs();
            args.PreventDefault(p);
        }));

        var json = plan.Render();
        AssertSchemaValid(json);

        AssertPropertiesExactly(json, "entries[0].reaction.commands[0]",
            "kind", "mutation");
        AssertPropertiesExactly(json, "entries[0].reaction.commands[0].mutation",
            "kind", "prop", "value");
        AssertPropertiesExactly(json, "entries[0].reaction.commands[0].mutation.value",
            "kind", "value");
    }

    [Test]
    public void resident_filtering_helpers_cover_only_publicly_reachable_source_arg_shape()
    {
        AssertDefinitionPropertiesExactly("MutateEventCommand",
            "kind", "mutation");
        AssertDefinitionPropertiesExactly("SourceValue", "kind", "source", "coerce");

        var plan = CreatePlan();
        On(plan, t => t.CustomEvent("filtering", p =>
        {
            var args = new FusionAutoCompleteFilteringArgs();
            p.Get("/api/filter")
             .Response(r => r.OnSuccess<FilterResultsResponse>((json, s) =>
             {
                 args.UpdateData(s, json, x => x.Items);
             }));
        }));

        var json = plan.Render();
        AssertSchemaValid(json);

        // The fluent DSL reaches source-backed event mutation through CallMutation args here.
        // It does not emit any separate command-level source lane.
        AssertPropertiesExactly(json,
            "entries[0].reaction.request.onSuccess[0].commands[0]",
            "kind", "mutation");
        AssertPropertiesExactly(json,
            "entries[0].reaction.request.onSuccess[0].commands[0].mutation",
            "kind", "method", "args");
        AssertPropertiesExactly(json,
            "entries[0].reaction.request.onSuccess[0].commands[0].mutation.args[0]",
            "kind", "source");
    }
}
