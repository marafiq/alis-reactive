using System.Text.Json;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.Descriptors.Mutations;
using Alis.Reactive.Descriptors.Values;

namespace Alis.Reactive.UnitTests;

public sealed class LoweringHarnessInput : IComponent, IInputComponent
{
    public string Vendor => "native";
    public string ReadExpr => "payload.current.displayName";
}

public sealed class LoweringHarnessWidget : IComponent
{
    public string Vendor => "fusion";
}

public sealed class LoweringHarnessToast : IAppLevelComponent
{
    public string Vendor => "fusion";
    public string DefaultId => "app-toast";
}

public static class LoweringHarnessInputExtensions
{
    private static readonly LoweringHarnessInput Component = new();

    public static ComponentRef<LoweringHarnessInput, TModel> SetSemanticValue<TModel>(
        this ComponentRef<LoweringHarnessInput, TModel> self,
        string value)
        where TModel : class
        => self.Emit(new SetPropMutation("value", CommandValue.FromLiteral(value)));

    public static TypedComponentSource<string> SemanticValue<TModel>(
        this ComponentRef<LoweringHarnessInput, TModel> self)
        where TModel : class
        => new(self.TargetId, Component.Vendor, Component.ReadExpr);
}

public static class LoweringHarnessWidgetExtensions
{
    public static ComponentRef<LoweringHarnessWidget, TModel> Open<TModel>(
        this ComponentRef<LoweringHarnessWidget, TModel> self)
        where TModel : class
        => self.Emit(new CallMutation("open"));

    public static ComponentRef<LoweringHarnessWidget, TModel> SetMode<TModel>(
        this ComponentRef<LoweringHarnessWidget, TModel> self,
        string mode)
        where TModel : class
        => self.Emit(new CallMutation("setMode", args: [CommandValue.FromLiteral(mode)]));

    public static ComponentRef<LoweringHarnessWidget, TModel> SetStatusFrom<TModel>(
        this ComponentRef<LoweringHarnessWidget, TModel> self,
        TypedSource<string> source)
        where TModel : class
        => self.Emit(new SetPropMutation("status", CommandValue.FromSource(source.ToBindSource())));
}

public static class LoweringHarnessToastExtensions
{
    public static ComponentRef<LoweringHarnessToast, TModel> Show<TModel>(
        this ComponentRef<LoweringHarnessToast, TModel> self,
        string message)
        where TModel : class
        => self.Emit(new CallMutation("show", args: [CommandValue.FromLiteral(message)]));
}

public class LoweringHarnessModel
{
    public string? Name { get; set; }
    public string? Status { get; set; }
}

[TestFixture]
public class WhenLoweringDslSurfacesToRuntimeAlgebra : PlanTestBase
{
    [Test]
    public void Bound_explicit_and_app_level_component_refs_share_the_same_mutation_envelope()
    {
        var plan = new ReactivePlan<LoweringHarnessModel>();

        new Builders.TriggerBuilder<LoweringHarnessModel>(plan).DomReady(p =>
        {
            p.Component<LoweringHarnessInput>(m => m.Name).SetSemanticValue("Amina");
            p.Component<LoweringHarnessWidget>("resident-tabs").SetMode("summary");
            p.Component<LoweringHarnessToast>().Show("saved");
        });

        using var doc = RenderDocument(plan);
        var commands = doc.RootElement.GetProperty("entries")[0].GetProperty("reaction").GetProperty("commands");

        Assert.That(commands[0].GetProperty("kind").GetString(), Is.EqualTo("mutate-element"));
        Assert.That(commands[0].GetProperty("target").GetString(), Is.EqualTo(IdGenerator.For<LoweringHarnessModel>(m => m.Name)));
        Assert.That(commands[0].GetProperty("vendor").GetString(), Is.EqualTo("native"));
        Assert.That(commands[0].GetProperty("mutation").GetProperty("kind").GetString(), Is.EqualTo("set-prop"));
        Assert.That(commands[0].GetProperty("mutation").GetProperty("prop").GetString(), Is.EqualTo("value"));

        Assert.That(commands[1].GetProperty("kind").GetString(), Is.EqualTo("mutate-element"));
        Assert.That(commands[1].GetProperty("target").GetString(), Is.EqualTo("resident-tabs"));
        Assert.That(commands[1].GetProperty("vendor").GetString(), Is.EqualTo("fusion"));
        Assert.That(commands[1].GetProperty("mutation").GetProperty("kind").GetString(), Is.EqualTo("call"));
        Assert.That(commands[1].GetProperty("mutation").GetProperty("method").GetString(), Is.EqualTo("setMode"));

        Assert.That(commands[2].GetProperty("kind").GetString(), Is.EqualTo("mutate-element"));
        Assert.That(commands[2].GetProperty("target").GetString(), Is.EqualTo("app-toast"));
        Assert.That(commands[2].GetProperty("vendor").GetString(), Is.EqualTo("fusion"));
        Assert.That(commands[2].GetProperty("mutation").GetProperty("kind").GetString(), Is.EqualTo("call"));
        Assert.That(commands[2].GetProperty("mutation").GetProperty("method").GetString(), Is.EqualTo("show"));
    }

    [Test]
    public void Gather_can_lower_expression_bound_and_explicit_component_refs_through_the_same_input_contract()
    {
        var plan = new ReactivePlan<LoweringHarnessModel>();

        new Builders.TriggerBuilder<LoweringHarnessModel>(plan).DomReady(p =>
            p.Post("/api/save", g =>
            {
                g.Include<LoweringHarnessInput, LoweringHarnessModel>(m => m.Name);
                g.Include<LoweringHarnessInput, LoweringHarnessModel>("resident-summary", "SelectedResident");
            }).Response(r => r.OnSuccess(s => s.Dispatch("saved"))));

        using var doc = RenderDocument(plan);
        var gather = doc.RootElement.GetProperty("entries")[0]
            .GetProperty("reaction")
            .GetProperty("request")
            .GetProperty("gather");

        Assert.That(gather[0].GetProperty("kind").GetString(), Is.EqualTo("component"));
        Assert.That(gather[0].GetProperty("componentId").GetString(), Is.EqualTo(IdGenerator.For<LoweringHarnessModel>(m => m.Name)));
        Assert.That(gather[0].GetProperty("vendor").GetString(), Is.EqualTo("native"));
        Assert.That(gather[0].GetProperty("name").GetString(), Is.EqualTo("Name"));
        Assert.That(gather[0].GetProperty("readExpr").GetString(), Is.EqualTo("payload.current.displayName"));

        Assert.That(gather[1].GetProperty("kind").GetString(), Is.EqualTo("component"));
        Assert.That(gather[1].GetProperty("componentId").GetString(), Is.EqualTo("resident-summary"));
        Assert.That(gather[1].GetProperty("vendor").GetString(), Is.EqualTo("native"));
        Assert.That(gather[1].GetProperty("name").GetString(), Is.EqualTo("SelectedResident"));
        Assert.That(gather[1].GetProperty("readExpr").GetString(), Is.EqualTo("payload.current.displayName"));
    }

    [Test]
    public void Bound_and_explicit_component_sources_lower_the_same_way_in_conditions_and_command_values()
    {
        var plan = new ReactivePlan<LoweringHarnessModel>();

        new Builders.TriggerBuilder<LoweringHarnessModel>(plan).DomReady(p =>
        {
            var bound = p.Component<LoweringHarnessInput>(m => m.Name);
            var explicitRef = p.Component<LoweringHarnessInput>("resident-summary");

            p.When(bound.SemanticValue()).Eq(explicitRef.SemanticValue())
                .Then(t => t.Component<LoweringHarnessWidget>("resident-tabs").SetStatusFrom(bound.SemanticValue()));
        });

        using var doc = RenderDocument(plan);
        var reaction = doc.RootElement.GetProperty("entries")[0].GetProperty("reaction");
        var branch = reaction.GetProperty("branches")[0];
        var guard = branch.GetProperty("guard");
        var commandValueSource = branch.GetProperty("reaction")
            .GetProperty("commands")[0]
            .GetProperty("mutation")
            .GetProperty("value")
            .GetProperty("source");

        Assert.That(guard.GetProperty("source").GetProperty("kind").GetString(), Is.EqualTo("component"));
        Assert.That(guard.GetProperty("source").GetProperty("componentId").GetString(), Is.EqualTo(IdGenerator.For<LoweringHarnessModel>(m => m.Name)));
        Assert.That(guard.GetProperty("source").GetProperty("vendor").GetString(), Is.EqualTo("native"));
        Assert.That(guard.GetProperty("source").GetProperty("readExpr").GetString(), Is.EqualTo("payload.current.displayName"));

        Assert.That(guard.GetProperty("rightSource").GetProperty("kind").GetString(), Is.EqualTo("component"));
        Assert.That(guard.GetProperty("rightSource").GetProperty("componentId").GetString(), Is.EqualTo("resident-summary"));
        Assert.That(guard.GetProperty("rightSource").GetProperty("vendor").GetString(), Is.EqualTo("native"));
        Assert.That(guard.GetProperty("rightSource").GetProperty("readExpr").GetString(), Is.EqualTo("payload.current.displayName"));

        Assert.That(commandValueSource.GetProperty("kind").GetString(), Is.EqualTo("component"));
        Assert.That(commandValueSource.GetProperty("componentId").GetString(), Is.EqualTo(IdGenerator.For<LoweringHarnessModel>(m => m.Name)));
        Assert.That(commandValueSource.GetProperty("vendor").GetString(), Is.EqualTo("native"));
        Assert.That(commandValueSource.GetProperty("readExpr").GetString(), Is.EqualTo("payload.current.displayName"));
    }

    [Test]
    public void Request_stages_remain_request_owned_in_the_lowered_plan_shape()
    {
        var plan = new ReactivePlan<LoweringHarnessModel>();

        new Builders.TriggerBuilder<LoweringHarnessModel>(plan).DomReady(p =>
            p.Post("/api/save", g => g.Include<LoweringHarnessInput, LoweringHarnessModel>(m => m.Name))
             .AsFormData()
             .WhileLoading(l => l.Component<LoweringHarnessWidget>("resident-tabs").Open())
             .Response(r => r
                .OnSuccess(s => s.Component<LoweringHarnessToast>().Show("saved"))
                .OnError(400, e => e.Component<LoweringHarnessWidget>("resident-tabs").SetMode("error"))
                .Chained(c => c.Get("/api/next"))));

        using var doc = RenderDocument(plan);
        var request = doc.RootElement.GetProperty("entries")[0]
            .GetProperty("reaction")
            .GetProperty("request");

        Assert.That(request.GetProperty("verb").GetString(), Is.EqualTo("POST"));
        Assert.That(request.GetProperty("url").GetString(), Is.EqualTo("/api/save"));
        Assert.That(request.GetProperty("contentType").GetString(), Is.EqualTo("form-data"));
        Assert.That(request.GetProperty("gather").GetArrayLength(), Is.EqualTo(1));
        Assert.That(request.GetProperty("whileLoading")[0].GetProperty("kind").GetString(), Is.EqualTo("mutate-element"));
        Assert.That(request.GetProperty("onSuccess")[0].GetProperty("commands")[0].GetProperty("target").GetString(), Is.EqualTo("app-toast"));
        Assert.That(request.GetProperty("onError")[0].GetProperty("statusCode").GetInt32(), Is.EqualTo(400));
        Assert.That(request.GetProperty("onError")[0].GetProperty("commands")[0].GetProperty("target").GetString(), Is.EqualTo("resident-tabs"));
        Assert.That(request.GetProperty("chained").GetProperty("verb").GetString(), Is.EqualTo("GET"));
        Assert.That(request.GetProperty("chained").GetProperty("url").GetString(), Is.EqualTo("/api/next"));
    }

    private static JsonDocument RenderDocument<TModel>(ReactivePlan<TModel> plan)
        where TModel : class
    {
        var json = plan.Render();
        AssertSchemaValid(json);
        return JsonDocument.Parse(json);
    }
}
