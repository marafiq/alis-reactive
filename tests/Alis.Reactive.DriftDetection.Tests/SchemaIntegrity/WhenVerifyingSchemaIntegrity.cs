using Alis.Reactive.DriftDetection.Tests.Infrastructure;

namespace Alis.Reactive.DriftDetection.Tests.SchemaIntegrity;

/// <summary>
/// Verifies the schema itself is structured to catch drift.
/// additionalProperties: false on every object definition is what makes
/// the C# -> Schema direction work -- without it, extra properties pass silently.
/// </summary>
[TestFixture]
public class WhenVerifyingSchemaIntegrity : DriftTestBase
{
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
