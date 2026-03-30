using System.Text.Json;
using Json.Schema;

namespace Alis.Reactive.UnitTests;

public class TestModel
{
    public string? Id { get; set; }
}

[TestFixture]
public abstract class PlanTestBase
{
    private static JsonSchema? _schema;

    protected static JsonSchema Schema =>
        _schema ??= JsonSchema.FromText(
            File.ReadAllText(
                Path.Combine(
                    TestContext.CurrentContext.TestDirectory,
                    "Schemas",
                    "reactive-plan.schema.json")));

    protected static ReactivePlan<TestModel> CreatePlan() => new();

    protected static Builders.TriggerBuilder<TestModel> Trigger(ReactivePlan<TestModel> plan) => new(plan);

    protected static void AssertSchemaValid(string planJson)
    {
        using var doc = JsonDocument.Parse(planJson);
        var result = Schema.Evaluate(doc.RootElement, new EvaluationOptions
        {
            OutputFormat = OutputFormat.List
        });
        Assert.That(result.IsValid, Is.True, () => FormatErrors(result));
    }

    private static string FormatErrors(EvaluationResults result)
    {
        var errors = result.Details?
            .Where(d => d.Errors != null && d.Errors.Count > 0)
            .SelectMany(d => d.Errors!.Select(e => $"{d.EvaluationPath}: {e.Key} = {e.Value}"))
            .ToList() ?? [];
        return $"Schema validation failed:\n{string.Join("\n", errors)}";
    }
}
