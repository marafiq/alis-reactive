using System.Collections.Generic;
using Alis.Reactive.DriftDetection.Tests.Infrastructure;
using Alis.Reactive.Fusion.Components;
using Alis.Reactive.Native.Components;
using Alis.Reactive.Native.Extensions;

namespace Alis.Reactive.DriftDetection.Tests.Commands;

[TestFixture]
public class WhenDetectingCommandSchemaDrift : DriftTestBase
{
    private sealed class FilterResultsResponse
    {
        public List<string> Items { get; set; } = [];
    }

    [Test]
    public void dispatch_conforms_to_schema()
    {
        // DispatchCommand: kind, event, payload
        // Minimal variant: no payload.
        AssertDefinitionPropertiesExactly("DispatchCommand", "kind", "event", "payload");

        var plan = CreatePlan();
        On(plan, t => t.DomReady(p => p.Dispatch("resident-admitted")));

        var json = plan.Render();
        AssertSchemaValid(json);

        AssertPropertiesExactly(json, "entries[0].reaction.commands[0]",
            "kind", "event");
    }

    [Test]
    public void dispatch_with_payload_conforms()
    {
        // DispatchCommand: kind, event, payload
        AssertDefinitionPropertiesExactly("DispatchCommand", "kind", "event", "payload");

        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
            p.Dispatch("notify", new { level = "Memory Care", residentId = 42 })));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertPropertiesExactly(json, "entries[0].reaction.commands[0]",
            "kind", "event", "payload");
    }

    [Test]
    public void element_set_prop_conforms()
    {
        // MutateElementCommand: kind, target, mutation, value, source, vendor
        // Minimal: exercises kind, target, mutation, value
        AssertDefinitionPropertiesExactly("MutateElementCommand",
            "kind", "target", "mutation", "value", "source", "vendor");

        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
            p.Element("welcome").SetText("Hello, resident!")));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertPropertiesExactly(json, "entries[0].reaction.commands[0]",
            "kind", "target", "mutation", "value");
    }

    [Test]
    public void element_with_all_properties_conforms()
    {
        // MutateElementCommand has: kind, target, mutation, value, source, vendor
        // Exercised across multiple commands to cover all properties.
        AssertDefinitionPropertiesExactly("MutateElementCommand",
            "kind", "target", "mutation", "value", "source", "vendor");

        var plan = CreatePlan();

        On(plan, t => t.CustomEvent<ResidentModel>("update", (args, p) =>
        {
            // vendor + value: Component<NativeTextBox>.SetValue produces mutation with vendor=native
            var nameComp = p.Component<NativeTextBox>(m => m.Name);
            nameComp.SetValue("new name");

            // source: SetText from event source
            p.Element("name-echo").SetText(args, x => x.Name!);

            // value: literal SetText
            p.Element("status").SetText("updated");
        }));

        var json = plan.Render();
        AssertSchemaValid(json);

        // No single MutateElementCommand carries all 6 properties simultaneously
        // (source and value are mutually exclusive). Verify each across commands.
        // cmd[0] = Component SetValue: vendor, value
        AssertPropertiesExactly(json, "entries[0].reaction.commands[0]",
            "kind", "target", "mutation", "value", "vendor");
        // cmd[1] = SetText from event source
        AssertPropertiesExactly(json, "entries[0].reaction.commands[1]",
            "kind", "target", "mutation", "source");
        // cmd[2] = SetText with literal value
        AssertPropertiesExactly(json, "entries[0].reaction.commands[2]",
            "kind", "target", "mutation", "value");
    }

    [Test]
    public void element_call_mutation_conforms()
    {
        // CallMutation has: kind, method, chain, args
        // AddClass produces: method="add", chain="classList", args=[LiteralArg]
        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
            p.Element("panel").AddClass("active")));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "CallMutation",
            "entries[0].reaction.commands[0].mutation");
    }

    [Test]
    public void mutate_event_with_publicly_reachable_properties_conforms()
    {
        // MutateEventCommand: kind, mutation, value, source
        // Public Fusion filtering extensions currently exercise the value variant.
        AssertDefinitionPropertiesExactly("MutateEventCommand",
            "kind", "mutation", "value", "source");

        var plan = CreatePlan();
        On(plan, t => t.CustomEvent("filtering", p =>
        {
            var args = new FusionAutoCompleteFilteringArgs();
            args.PreventDefault(p);
        }));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertPropertiesExactly(json, "entries[0].reaction.commands[0]",
            "kind", "mutation", "value");
    }

    [Test]
    public void validation_errors_conforms()
    {
        // ValidationErrorsCommand: kind, formId
        AssertDefinitionPropertiesExactly("ValidationErrorsCommand", "kind", "formId");

        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
            p.ValidationErrors("resident-form")));

        var json = plan.Render();
        AssertSchemaValid(json);

        AssertPropertiesExactly(json, "entries[0].reaction.commands[0]",
            "kind", "formId");
    }

    [Test]
    public void into_conforms()
    {
        // IntoCommand: kind, target
        AssertDefinitionPropertiesExactly("IntoCommand", "kind", "target");

        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
        {
            p.Get("/api/resident/partial")
             .Response(r => r.OnSuccess(s => s.Into("content-area")));
        }));

        var json = plan.Render();
        AssertSchemaValid(json);

        AssertPropertiesExactly(json,
            "entries[0].reaction.request.onSuccess[0].commands[0]",
            "kind", "target");
    }

    [Test]
    public void literal_arg_conforms()
    {
        // LiteralArg: kind, value
        // Hide produces CallMutation("setAttribute", args: [LiteralArg("hidden"), LiteralArg("")])
        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
            p.Element("alert").Hide()));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "LiteralArg",
            "entries[0].reaction.commands[0].mutation.args[0]");
    }

    [Test]
    public void source_arg_conforms()
    {
        AssertDefinitionPropertiesExactly("SourceArg", "kind", "source", "coerce");

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
        AssertPropertiesExactly(json,
            "entries[0].reaction.request.onSuccess[0].commands[0].mutation.args[0]",
            "kind", "source");
    }
}
