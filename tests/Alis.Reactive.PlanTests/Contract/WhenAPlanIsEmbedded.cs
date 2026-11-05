using System.Text.Json;
using Alis.Reactive.PlanTests.Support;
using Json.Schema;

namespace Alis.Reactive.PlanTests.Contract;

[TestFixture]
public class WhenAPlanIsEmbedded
{
    private static readonly JsonSchema Schema =
        JsonSchema.FromFile(Path.Combine(Repository.Root, "Alis.Reactive.Assets", "runtime", "types", "plan.schema.json"));

    [TestCaseSource(nameof(Goldens))]
    public void every_golden_conforms_to_the_plan_schema(string golden)
    {
        using var plan = JsonDocument.Parse(File.ReadAllText(Path.Combine(Golden.Root, golden)));

        var result = Schema.Evaluate(plan.RootElement, new EvaluationOptions { OutputFormat = OutputFormat.List });

        Assert.That(result.IsValid, Is.True, $"{golden} does not conform to plan.schema.json:\n{Errors(result)}");
    }

    private static IEnumerable<string> Goldens()
        => Directory.EnumerateFiles(Golden.Root, "*.json", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(Golden.Root, path))
            .OrderBy(path => path, StringComparer.Ordinal);

    private static string Errors(EvaluationResults result)
        => string.Join("\n", (result.Details ?? [])
            .Where(detail => detail.Errors != null)
            .Select(detail => $"  {detail.InstanceLocation}: {string.Join("; ", detail.Errors!.Select(error => error.Key + ": " + error.Value))}"));
}
