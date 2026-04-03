using System.Text.Json;
using Json.Schema;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.UnitTests;

public class FusionTestAddress
{
    public decimal PostalCode { get; set; }
    public string? City { get; set; }
}

public class FusionTestModel
{
    public decimal Amount { get; set; }
    public string? Status { get; set; }
    public FusionTestAddress? Address { get; set; }
    public DateTime? AppointmentTime { get; set; }
    public DateTime[]? StayPeriod { get; set; }
    public string? PhoneNumber { get; set; }
    public string? CarePlan { get; set; }
    public bool ReceiveNotifications { get; set; }
    public string? Documents { get; set; }
}

[TestFixture]
public abstract class FusionTestBase
{
    private static JsonSchema? _schema;

    protected static JsonSchema Schema =>
        _schema ??= JsonSchema.FromText(
            File.ReadAllText(
                Path.Combine(
                    TestContext.CurrentContext.TestDirectory,
                    "Schemas",
                    "reactive-plan.schema.json")));

    protected static ReactivePlan<FusionTestModel> CreatePlan() => new();

    protected static TriggerBuilder<FusionTestModel> Trigger(ReactivePlan<FusionTestModel> plan) => new(plan);

    protected static PipelineBuilder<FusionTestModel> CreateDomReadyPipeline()
    {
        var plan = CreatePlan();
        var scope = plan.Authoring.CreateDomReadyScope();
        return new PipelineBuilder<FusionTestModel>(plan.Authoring, scope);
    }

    protected static void WireObjectEvent(
        ReactivePlan<FusionTestModel> plan,
        string componentId,
        string vendor,
        string bindingPath,
        string valueMemberPath,
        string eventName,
        Action<PipelineBuilder<FusionTestModel>> configure)
    {
        var scope = plan.Authoring.CreateObjectEventScope(componentId, vendor, bindingPath, valueMemberPath, eventName);
        var pipeline = new PipelineBuilder<FusionTestModel>(plan.Authoring, scope);
        configure(pipeline);
        plan.AddWorkflow(scope, pipeline);
    }

    protected static void WireObjectEvent<TArgs>(
        ReactivePlan<FusionTestModel> plan,
        string componentId,
        string vendor,
        string bindingPath,
        string valueMemberPath,
        string eventName,
        TArgs args,
        Action<TArgs, PipelineBuilder<FusionTestModel>> configure)
    {
        var scope = plan.Authoring.CreateObjectEventScope(componentId, vendor, bindingPath, valueMemberPath, eventName);
        var pipeline = new PipelineBuilder<FusionTestModel>(plan.Authoring, scope);
        configure(args, pipeline);
        plan.AddWorkflow(scope, pipeline);
    }

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
