using System.Text.Json;
using System.Text.Json.Nodes;
using Alis.Reactive.DriftDetection.Tests.Infrastructure;

namespace Alis.Reactive.DriftDetection.Tests.SchemaIntegrity;

[TestFixture]
public class WhenVerifyingSchemaIntegrity : DriftTestBase
{
    [Test]
    public void root_schema_exposes_v2_sections()
    {
        using var doc = JsonDocument.Parse(SchemaJson);
        var root = doc.RootElement;

        var required = root.GetProperty("required").EnumerateArray().Select(x => x.GetString()).ToArray();
        Assert.That(required, Is.EquivalentTo(new[] { "version", "planId", "contracts", "objects", "bindings", "workflows" }));
        Assert.That(root.GetProperty("properties").EnumerateObject().Select(x => x.Name),
            Is.EquivalentTo(new[] { "version", "planId", "sourceId", "contracts", "objects", "bindings", "workflows" }));
    }

    [Test]
    public void all_object_definitions_enforce_additional_properties_false()
    {
        var violations = Analyzer.AllDefinitions
            .Where(x => x.Value.IsObjectDef && x.Value.AdditionalProperties != false)
            .Select(x => x.Key)
            .ToList();

        Assert.That(violations, Is.Empty);
    }

    [Test]
    public void v2_unions_have_expected_variants()
    {
        Assert.That(Analyzer.GetDefinition("PlanSubscription").UnionVariants, Is.EquivalentTo(new[]
        {
            "DomReadySubscription",
            "DocumentEventSubscription",
            "ObjectEventSubscription",
            "ServerPushSubscription",
            "SignalRSubscription"
        }));

        Assert.That(Analyzer.GetDefinition("PlanAction").UnionVariants, Is.EquivalentTo(new[]
        {
            "SequenceAction",
            "BranchAction",
            "ParallelAction",
            "SetAction",
            "CallAction",
            "DispatchAction",
            "RequestAction",
            "InjectAction",
            "ShowValidationErrorsAction"
        }));

        Assert.That(Analyzer.GetDefinition("PlanPredicate").UnionVariants, Is.EquivalentTo(new[]
        {
            "ComparePredicate",
            "AllPredicate",
            "AnyPredicate",
            "NotPredicate",
            "ConfirmPredicate"
        }));

        Assert.That(Analyzer.GetDefinition("ValueExpr").UnionVariants, Is.EquivalentTo(new[]
        {
            "LiteralValueExpr",
            "BindingValueExpr",
            "MemberValueExpr",
            "ContextValueExpr",
            "ObjectValueExpr",
            "ArrayValueExpr",
            "ConvertValueExpr"
        }));
    }

    [Test]
    public void schema_evaluation_rejects_unknown_properties_on_rendered_v2_json()
    {
        var plan = CreatePlan();
        On(plan, t => t.DomReady(p => p.Dispatch("booted")));

        var json = plan.Render();
        AssertSchemaValid(json);

        var mutated = AddUnknownProperty(
            json,
            "workflows[0].run",
            "unexpected",
            JsonValue.Create("drifted")!);

        AssertSchemaInvalid(mutated, "additionalProperties: false should reject unknown V2 action fields");
    }
}
