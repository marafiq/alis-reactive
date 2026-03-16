using System.Text.Json;
using Json.Schema;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Native.UnitTests;

public class NativeTestAddress
{
    public string? City { get; set; }
    public string? State { get; set; }
}

public class NativeTestModel
{
    public string? Status { get; set; }
    public string? Category { get; set; }
    public DateTime? AdmissionDate { get; set; }
    public NativeTestAddress? Address { get; set; }
}

[TestFixture]
public abstract class NativeTestBase
{
    private static JsonSchema? _schema;

    protected static JsonSchema Schema =>
        _schema ??= JsonSchema.FromText(
            File.ReadAllText(
                Path.Combine(
                    TestContext.CurrentContext.TestDirectory,
                    "Schemas",
                    "reactive-plan.schema.json")));

    protected static ReactivePlan<NativeTestModel> CreatePlan() => new();

    protected static TriggerBuilder<NativeTestModel> Trigger(IReactivePlan<NativeTestModel> plan) => new(plan);

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
