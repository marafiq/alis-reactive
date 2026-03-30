using Alis.Reactive.DriftDetection.Tests.Infrastructure;
using Alis.Reactive.Native.Components;
using Alis.Reactive.Native.Extensions;

namespace Alis.Reactive.DriftDetection.Tests.Commands;

[TestFixture]
public class WhenDetectingCommandSchemaDrift : DriftTestBase
{
    [Test]
    public void dispatch_conforms_to_schema()
    {
        // DispatchCommand: kind, event, payload, when
        // Payload exercises the optional property. 'when' is schema-defined
        // but not exposed by the DSL for Dispatch — cannot be populated.
        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
            p.Dispatch("resident-admitted", new { facilityId = "FAC-001" })));

        var json = plan.Render();
        AssertSchemaValid(json);

        // AssertAllPropertiesPresent not used: 'when' not reachable via DSL.
        AssertPropertiesPresent(json, "entries[0].reaction.commands[0]",
            "kind", "event", "payload");
    }

    [Test]
    public void dispatch_with_payload_conforms()
    {
        // DispatchCommand: kind, event, payload
        // 'when' is schema-defined but not exposed by DSL for Dispatch
        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
            p.Dispatch("notify", new { level = "Memory Care", residentId = 42 })));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertPropertiesPresent(json, "entries[0].reaction.commands[0]",
            "kind", "event", "payload");
    }

    [Test]
    public void element_set_prop_conforms()
    {
        // MutateElementCommand: kind, target, mutation, value, source, vendor, when
        // Minimal: exercises kind, target, mutation, value
        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
            p.Element("welcome").SetText("Hello, resident!")));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertPropertiesPresent(json, "entries[0].reaction.commands[0]",
            "kind", "target", "mutation", "value");
    }

    [Test]
    public void element_with_all_properties_conforms()
    {
        // MutateElementCommand has: kind, target, mutation, value, source, vendor, when
        // Exercised across multiple commands to cover all properties.
        var plan = CreatePlan();

        On(plan, t => t.CustomEvent<ResidentModel>("update", (args, p) =>
        {
            // vendor + value: Component<NativeTextBox>.SetValue produces mutation with vendor=native
            var nameComp = p.Component<NativeTextBox>(m => m.Name);
            nameComp.SetValue("new name");

            // source: SetText from event source
            p.Element("name-echo").SetText(args, x => x.Name!);

            // when: ElementBuilder.When attaches guard to the LAST command (cmd[1])
            p.Element("status")
             .When(args, x => x.Name!, g => g.NotEmpty())
             .SetText("updated");
        }));

        var json = plan.Render();
        AssertSchemaValid(json);

        // No single MutateElementCommand carries all 7 properties simultaneously
        // (source and value are mutually exclusive). Verify each across commands.
        // cmd[0] = Component SetValue: vendor, value
        AssertPropertiesPresent(json, "entries[0].reaction.commands[0]",
            "kind", "target", "mutation", "value", "vendor");
        // cmd[1] = SetText from event + When guard attached: source, when
        AssertPropertiesPresent(json, "entries[0].reaction.commands[1]",
            "kind", "target", "mutation", "source", "when");
        // cmd[2] = SetText with literal value (guard was on previous command)
        AssertPropertiesPresent(json, "entries[0].reaction.commands[2]",
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
    public void mutate_event_with_all_properties_conforms()
    {
        // BLOCKED: MutateEventCommand can only be produced through Fusion event arg extensions
        // (FusionAutoComplete.PreventDefault, FusionAutoComplete.UpdateData) which require
        // Syncfusion infrastructure not available in TestHtmlHelper.
        Assert.Inconclusive(
            "MutateEventCommand requires Fusion component event args (PreventDefault/UpdateData) " +
            "which need Syncfusion infrastructure not available in drift detection tests.");
    }

    [Test]
    public void validation_errors_conforms()
    {
        // ValidationErrorsCommand: kind, formId, when
        // 'when' per-command guard is not exposed by DSL for ValidationErrors
        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
            p.ValidationErrors("resident-form")));

        var json = plan.Render();
        AssertSchemaValid(json);

        // AssertAllPropertiesPresent not used: 'when' not reachable via DSL.
        AssertPropertiesPresent(json, "entries[0].reaction.commands[0]",
            "kind", "formId");
    }

    [Test]
    public void into_conforms()
    {
        // IntoCommand: kind, target, when
        // 'when' per-command guard is not exposed by DSL for Into
        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
        {
            p.Get("/api/resident/partial")
             .Response(r => r.OnSuccess(s => s.Into("content-area")));
        }));

        var json = plan.Render();
        AssertSchemaValid(json);

        // AssertAllPropertiesPresent not used: 'when' not reachable via DSL.
        AssertPropertiesPresent(json,
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
        // BLOCKED: SourceArg is produced by FusionAutoComplete.UpdateData and similar
        // Fusion-only code paths requiring Syncfusion infrastructure.
        Assert.Inconclusive(
            "SourceArg requires Fusion component extensions (UpdateData) " +
            "which need Syncfusion infrastructure not available in drift detection tests.");
    }
}
