using System.Text.Json;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.UnitTests.Conditions;

[TestFixture]
public class WhenBuildingConditionOperandShapes : PlanTestBase
{
    public sealed class TextPayload
    {
        public string Notes { get; set; } = "";
    }

    [Test]
    public void min_length_declares_numeric_operand_without_changing_source_shape()
    {
        var plan = CreatePlan();

        Trigger(plan).CustomEvent<TextPayload>("changed", (args, p) =>
        {
            p.When(args, x => x.Notes)
                .MinLength(3)
                .Then(t => t.Element("status").SetText("ok"));
        });

        using var document = JsonDocument.Parse(plan.RenderFormatted());
        var condition = FindCompareCondition(document.RootElement, "min-length");
        var rightValue = condition.GetProperty("right").GetProperty("value");

        Assert.That(condition.GetProperty("shape").GetProperty("kind").GetString(), Is.EqualTo("string"));
        Assert.That(rightValue.GetProperty("shape").GetProperty("kind").GetString(), Is.EqualTo("number"));
    }

    [Test]
    public void min_length_rejects_negative_lengths_before_plan_json_exists()
    {
        var plan = CreatePlan();

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Trigger(plan).CustomEvent<TextPayload>("changed", (args, p) =>
            {
                p.When(args, x => x.Notes)
                    .MinLength(-1)
                    .Then(t => t.Element("status").SetText("ok"));
            }));

        Assert.That(ex!.ParamName, Is.EqualTo("length"));
        Assert.That(ex.Message, Does.Contain("zero or greater"));
    }

    [Test]
    public void compare_condition_rejects_operator_operand_family_mismatches()
    {
        var left = ValueProducer.Literal(1);
        var right = ValueProducer.Literal(2);

        var missingRight = Assert.Throws<ArgumentException>(() =>
            Condition.Compare(
                CompareOperator.Gt,
                ComparisonOperands.Unary(left, Shape.Number)));

        var unexpectedRight = Assert.Throws<ArgumentException>(() =>
            Condition.Compare(
                CompareOperator.Truthy,
                ComparisonOperands.Binary(left, right, Shape.Number)));

        Assert.That(missingRight!.Message, Does.Contain("Comparison operator 'gt' requires a right operand."));
        Assert.That(unexpectedRight!.Message, Does.Contain("Comparison operator 'truthy' requires no right operand."));
    }

    private static JsonElement FindCompareCondition(JsonElement root, string op)
    {
        if (IsCompareCondition(root, op)) return root;

        foreach (var child in ChildrenOf(root))
        {
            var condition = FindCompareCondition(child, op);
            if (condition.ValueKind != JsonValueKind.Undefined) return condition;
        }

        return default;
    }

    private static bool IsCompareCondition(JsonElement element, string op)
    {
        var isObject = element.ValueKind == JsonValueKind.Object;
        if (!isObject) return false;

        var hasCompareKind = element.TryGetProperty("kind", out var kind)
            && kind.GetString() == "compare";
        var hasExpectedOperator = element.TryGetProperty("op", out var operatorValue)
            && operatorValue.GetString() == op;
        return hasCompareKind && hasExpectedOperator;
    }

    private static IEnumerable<JsonElement> ChildrenOf(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
                yield return property.Value;
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
                yield return item;
        }
    }
}
