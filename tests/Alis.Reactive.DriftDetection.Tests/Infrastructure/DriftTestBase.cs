using System.Text.Json;
using Alis.Reactive.Native.Extensions;
using Json.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.DriftDetection.Tests.Infrastructure;

[TestFixture]
public abstract class DriftTestBase
{
    private static JsonSchema? _schema;
    private static SchemaAnalyzer? _analyzer;
    private static string? _schemaJson;

    protected static IHtmlHelper<ResidentModel> Html { get; } = new TestHtmlHelper<ResidentModel>();

    protected static string SchemaJson => _schemaJson ??= File.ReadAllText(
        Path.Combine(TestContext.CurrentContext.TestDirectory, "Schemas", "reactive-plan.schema.json"));

    protected static JsonSchema Schema => _schema ??= JsonSchema.FromText(SchemaJson);

    protected static SchemaAnalyzer Analyzer => _analyzer ??= new SchemaAnalyzer(SchemaJson);

    // ── Assertion 1: JSON is valid against schema (no extras) ──

    protected static void AssertSchemaValid(string planJson)
    {
        using var doc = JsonDocument.Parse(planJson);
        var result = Schema.Evaluate(doc.RootElement, new EvaluationOptions
        {
            OutputFormat = OutputFormat.List
        });
        Assert.That(result.IsValid, Is.True, () => FormatSchemaErrors(result));
    }

    // ── Assertion 2: JSON exercises ALL properties of a definition (no gaps) ──

    protected static void AssertAllPropertiesPresent(
        string planJson,
        string defName,
        string jsonPath)
    {
        using var doc = JsonDocument.Parse(planJson);
        var element = NavigateToPath(doc.RootElement, jsonPath);
        var jsonProps = new HashSet<string>();

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in element.EnumerateObject())
                jsonProps.Add(prop.Name);
        }

        var schemaDef = Analyzer.GetDefinition(defName);
        var missing = schemaDef.AllProperties.Except(jsonProps).ToList();

        Assert.That(missing, Is.Empty,
            $"Schema $defs/{defName} defines properties not present in JSON at '{jsonPath}': " +
            $"[{string.Join(", ", missing)}]. " +
            "This means the test is not exercising all schema-defined properties — " +
            "populate the missing optional properties in the DSL call.");
    }

    protected static void AssertDefinitionPropertiesExactly(
        string defName,
        params string[] expectedProperties)
    {
        var schemaDef = Analyzer.GetDefinition(defName);
        var actual = schemaDef.AllProperties.OrderBy(x => x).ToList();
        var expected = expectedProperties.OrderBy(x => x).ToList();

        Assert.That(actual, Is.EqualTo(expected),
            $"Schema $defs/{defName} properties drifted. " +
            $"Expected: [{string.Join(", ", expected)}]. " +
            $"Actual: [{string.Join(", ", actual)}].");
    }

    // ── Assertion 3: JSON has specific named properties at path ──

    /// <summary>
    /// Asserts that specific named properties are present in the JSON element at the given path.
    /// Use instead of AssertAllPropertiesPresent when a definition has mutually exclusive
    /// optional properties (e.g., ValueGuard operand vs rightSource) or when some properties
    /// are not reachable via the DSL.
    /// </summary>
    protected static void AssertPropertiesPresent(
        string planJson,
        string jsonPath,
        params string[] expectedProperties)
    {
        var jsonProps = GetPropertyNamesAtPath(planJson, jsonPath);

        var missing = expectedProperties.Where(p => !jsonProps.Contains(p)).ToList();

        Assert.That(missing, Is.Empty,
            $"Expected properties not present in JSON at '{jsonPath}': " +
            $"[{string.Join(", ", missing)}]. " +
            $"Actual properties: [{string.Join(", ", jsonProps)}].");
    }

    protected static void AssertPropertiesExactly(
        string planJson,
        string jsonPath,
        params string[] expectedProperties)
    {
        var actual = GetPropertyNamesAtPath(planJson, jsonPath).OrderBy(x => x).ToList();
        var expected = expectedProperties.OrderBy(x => x).ToList();

        Assert.That(actual, Is.EqualTo(expected),
            $"JSON properties drifted at '{jsonPath}'. " +
            $"Expected: [{string.Join(", ", expected)}]. " +
            $"Actual: [{string.Join(", ", actual)}].");
    }

    // ── Helpers ──

    protected static ReactivePlan<ResidentModel> CreatePlan()
        => Html.ReactivePlan<ResidentModel>();

    protected static void On(
        ReactivePlan<ResidentModel> plan,
        Action<Builders.TriggerBuilder<ResidentModel>> trigger)
        => Html.On(plan, trigger);

    private static JsonElement NavigateToPath(JsonElement root, string path)
    {
        var current = root;
        foreach (var segment in path.Split('.'))
        {
            if (segment.Contains('['))
            {
                var bracketIdx = segment.IndexOf('[');
                var prop = segment[..bracketIdx];
                var indexStr = segment[(bracketIdx + 1)..^1];
                var index = int.Parse(indexStr);

                if (prop.Length > 0)
                    current = current.GetProperty(prop);
                current = current[index];
            }
            else
            {
                current = current.GetProperty(segment);
            }
        }
        return current;
    }

    private static HashSet<string> GetPropertyNamesAtPath(string planJson, string jsonPath)
    {
        using var doc = JsonDocument.Parse(planJson);
        var element = NavigateToPath(doc.RootElement, jsonPath);
        var jsonProps = new HashSet<string>();

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in element.EnumerateObject())
                jsonProps.Add(prop.Name);
        }

        return jsonProps;
    }

    private static string FormatSchemaErrors(EvaluationResults result)
    {
        var errors = result.Details?
            .Where(d => d.Errors != null && d.Errors.Count > 0)
            .SelectMany(d => d.Errors!.Select(e => $"{d.EvaluationPath}: {e.Key} = {e.Value}"))
            .ToList() ?? [];
        return $"Schema validation failed:\n{string.Join("\n", errors)}";
    }
}
