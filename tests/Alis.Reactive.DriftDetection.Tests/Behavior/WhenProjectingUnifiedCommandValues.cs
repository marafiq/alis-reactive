using Alis.Reactive.DriftDetection.Tests.Infrastructure;
using Alis.Reactive.Native.Components;

namespace Alis.Reactive.DriftDetection.Tests.Behavior;

[TestFixture]
public class WhenProjectingUnifiedCommandValues : DriftTestBase
{
    private sealed class UnifiedPayload
    {
        public string? Name { get; set; }
    }

    [Test]
    public void resident_commands_share_one_value_contract()
    {
        Assert.That(Analyzer.GetDefinition("CommandValue").UnionVariants,
            Is.EquivalentTo(new[] { "LiteralValue", "SourceValue" }));
        AssertDefinitionPropertiesExactly("LiteralValue", "kind", "value", "coerce");
        AssertDefinitionPropertiesExactly("SourceValue", "kind", "source", "coerce");
        AssertDefinitionPropertiesExactly("SetPropMutation", "kind", "prop", "value");
        AssertDefinitionPropertiesExactly("CallMutation", "kind", "method", "chain", "args");
        AssertDefinitionPropertiesExactly("MutateElementCommand", "kind", "target", "mutation", "vendor");
        AssertDefinitionPropertiesExactly("MutateEventCommand", "kind", "mutation");
        AssertDefinitionPropertiesExactly("DispatchCommand", "kind", "event", "payload");
    }

    [Test]
    public void resident_ui_text_updates_describe_mutation_values_not_command_level_source_fields()
    {
        var plan = CreatePlan();
        On(plan, t => t.CustomEvent<UnifiedPayload>("updated", (args, p) =>
            p.Element("status").SetText(args, x => x.Name!)));

        var json = plan.Render();
        AssertSchemaValid(json);

        AssertPropertiesExactly(json, "entries[0].reaction.commands[0]", "kind", "target", "mutation");
        AssertPropertiesExactly(json, "entries[0].reaction.commands[0].mutation", "kind", "prop", "value");
        AssertPropertiesExactly(json, "entries[0].reaction.commands[0].mutation.value",
            "kind", "source");
        AssertPropertiesExactly(json, "entries[0].reaction.commands[0].mutation.value.source",
            "kind", "path");
    }

    [Test]
    public void resident_component_coercion_is_part_of_the_value_contract()
    {
        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
            p.Component<NativeCheckBox>(m => m.Name).SetChecked(true)));

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
    public void resident_dispatch_payload_fields_are_wrapped_in_the_same_value_contract()
    {
        var plan = CreatePlan();
        On(plan, t => t.DomReady(p => p.Dispatch("resident-saved", new { status = "ok" })));

        var json = plan.Render();
        AssertSchemaValid(json);

        AssertPropertiesExactly(json, "entries[0].reaction.commands[0]", "kind", "event", "payload");
        AssertPropertiesExactly(json, "entries[0].reaction.commands[0].payload.status", "kind", "value");
    }
}
