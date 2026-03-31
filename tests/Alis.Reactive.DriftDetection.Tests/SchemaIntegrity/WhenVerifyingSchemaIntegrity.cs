using Alis.Reactive.DriftDetection.Tests.Infrastructure;
using System.Text.Json.Nodes;

namespace Alis.Reactive.DriftDetection.Tests.SchemaIntegrity;

/// <summary>
/// Verifies the schema itself is structured to catch drift.
/// additionalProperties: false on every object definition is what makes
/// the C# -> Schema direction work -- without it, extra properties pass silently.
/// </summary>
[TestFixture]
public class WhenVerifyingSchemaIntegrity : DriftTestBase
{
    private static readonly HashSet<string> ObjectDefinitionsCoveredByExactAssertions =
    [
        "AllGather",
        "AllGuard",
        "AnyGuard",
        "Branch",
        "CallMutation",
        "ComponentEntry",
        "ComponentEventTrigger",
        "ComponentGather",
        "ComponentSource",
        "ConditionalReaction",
        "ConfirmGuard",
        "CustomEventTrigger",
        "DispatchCommand",
        "DomReadyTrigger",
        "Entry",
        "EventGather",
        "EventSource",
        "HttpReaction",
        "IntoCommand",
        "InvertGuard",
        "LiteralArg",
        "MutateElementCommand",
        "MutateEventCommand",
        "ParallelHttpReaction",
        "RequestDescriptor",
        "SequentialReaction",
        "ServerPushTrigger",
        "SetPropMutation",
        "SignalRTrigger",
        "SourceArg",
        "StaticGather",
        "StatusHandler",
        "ValidationCondition",
        "ValidationDescriptor",
        "ValidationErrorsCommand",
        "ValidationField",
        "ValidationRule",
        "ValueGuard"
    ];

    // Unions, enums, and non-object types that don't have additionalProperties
    private static readonly HashSet<string> NonObjectDefs = new()
    {
        "Trigger", "Reaction", "Command", "Guard", "BindSource",
        "Mutation", "MethodArg", "GatherItem", "BindExpr",
        "GuardOp", "Vendor", "CoercionType", "ValidationRuleType"
    };

    [Test]
    public void all_object_definitions_enforce_additional_properties_false()
    {
        var violations = new List<string>();

        foreach (var (name, def) in Analyzer.AllDefinitions)
        {
            if (NonObjectDefs.Contains(name)) continue;
            if (!def.IsObjectDef) continue;

            if (def.AdditionalProperties != false)
                violations.Add(name);
        }

        Assert.That(violations, Is.Empty,
            $"Schema object definitions missing additionalProperties: false: " +
            $"[{string.Join(", ", violations)}]. Without this constraint, " +
            "new C# properties pass schema validation silently -- drift detection breaks.");
    }

    [Test]
    public void schema_evaluation_rejects_unknown_properties_on_rendered_public_dsl_json()
    {
        var plan = CreatePlan();
        On(plan, t => t.DomReady(p => p.Dispatch("booted")));

        var json = plan.Render();
        AssertSchemaValid(json);

        var mutated = AddUnknownProperty(
            json,
            "entries[0].reaction.commands[0]",
            "unexpected",
            JsonValue.Create("drifted")!);

        AssertSchemaInvalid(mutated, "additionalProperties: false should reject unknown command fields");
    }

    [Test]
    public void all_object_definitions_have_a_matching_exact_definition_coverage_entry()
    {
        var objectDefs = Analyzer.AllDefinitions
            .Where(x => x.Value.IsObjectDef)
            .Select(x => x.Key)
            .ToHashSet();

        Assert.That(ObjectDefinitionsCoveredByExactAssertions, Is.EquivalentTo(objectDefs),
            "Every schema object definition should be represented in the exact-definition coverage set.");
    }

    [Test]
    public void all_enum_definitions_have_values()
    {
        var enumDefs = new[] { "GuardOp", "Vendor", "CoercionType", "ValidationRuleType" };

        foreach (var defName in enumDefs)
        {
            var def = Analyzer.GetDefinition(defName);
            Assert.That(def.IsEnumDef, Is.True,
                $"Schema $defs/{defName} should be an enum definition.");
            Assert.That(def.EnumValues, Is.Not.Empty,
                $"Schema $defs/{defName} has no enum values.");
        }
    }

    [Test]
    public void all_union_definitions_have_variants()
    {
        var expectedUnions = new Dictionary<string, string[]>
        {
            ["Trigger"] = new[]
            {
                "DomReadyTrigger", "CustomEventTrigger", "ComponentEventTrigger",
                "ServerPushTrigger", "SignalRTrigger"
            },
            ["Reaction"] = new[]
            {
                "SequentialReaction", "ConditionalReaction",
                "HttpReaction", "ParallelHttpReaction"
            },
            ["Command"] = new[]
            {
                "DispatchCommand", "MutateElementCommand", "MutateEventCommand",
                "ValidationErrorsCommand", "IntoCommand"
            },
            ["Guard"] = new[]
            {
                "ValueGuard", "AllGuard", "AnyGuard", "InvertGuard", "ConfirmGuard"
            },
            ["BindSource"] = new[] { "EventSource", "ComponentSource" },
            ["Mutation"] = new[] { "SetPropMutation", "CallMutation" },
            ["MethodArg"] = new[] { "LiteralArg", "SourceArg" },
            ["GatherItem"] = new[]
            {
                "ComponentGather", "StaticGather", "AllGather", "EventGather"
            }
        };

        foreach (var (defName, expectedVariants) in expectedUnions)
        {
            var def = Analyzer.GetDefinition(defName);
            Assert.That(def.IsUnionDef, Is.True,
                $"Schema $defs/{defName} should be a oneOf union.");
            Assert.That(def.UnionVariants, Is.EquivalentTo(expectedVariants),
                $"Schema $defs/{defName} variants don't match expected.");
        }
    }
}
