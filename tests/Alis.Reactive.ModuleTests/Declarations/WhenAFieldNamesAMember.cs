using System;
using System.Text.Json;
using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.ValuesAndSources;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Declarations;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAFieldNamesAMember
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";

    private const string SortBy =
        "{\"version\":7,\"planId\":\"Alis.Reactive.ModuleTests.Declarations.Resident\"," +
        "\"culture\":{\"name\":\"en-US\",\"collation\":\"en-US\",\"decimal\":\".\",\"group\":\",\"," +
        "\"integer\":{\"group\":false,\"exponent\":false},\"fraction\":{\"group\":true,\"exponent\":true}}," +
        "\"timeZone\":{\"kind\":\"clock\"},\"trace\":{\"kind\":\"off\"}," +
        "\"vendors\":{\"native\":{\"locate\":{\"kind\":\"fn\",\"params\":[\"element\"],\"body\":{\"kind\":\"ref\",\"name\":\"element\"}}," +
        "\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"}}}," +
        "\"objects\":{\"$document\":{\"kind\":\"plugin\",\"name\":\"alis.document\",\"key\":\"\"," +
        "\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"},\"data\":{\"kind\":\"none\"}}," +
        "\"roster\":{\"kind\":\"element\",\"id\":\"roster\",\"vendor\":\"native\"}}," +
        "\"handlers\":[{\"on\":\"$document\",\"path\":[],\"event\":\"ready\",\"payload\":[],\"members\":\"required\",\"policy\":\"overlap\",\"do\":[" +
        "{\"kind\":\"do\",\"await\":false,\"expr\":{\"kind\":\"call\",\"path\":[\"sortBy\"],\"members\":\"required\"," +
        "\"target\":{\"kind\":\"obj\",\"id\":\"roster\"},\"args\":[";

    private const string Ends = "]}}]}]}";
    private const string SnakeCase = SortBy + "{\"kind\":\"lit\",\"value\":\"multi_word\"}" + Ends;
    private const string CamelCase = SortBy + "{\"kind\":\"lit\",\"value\":\"multiWord\"}" + Ends;
    private const string ClrName = SortBy + "{\"kind\":\"lit\",\"value\":\"MultiWord\"}" + Ends;
    private const string ExplicitName = SortBy + "{\"kind\":\"lit\",\"value\":\"care-priority\"}" + Ends;
    private const string NestedName = SortBy + "{\"kind\":\"lit\",\"value\":\"room.number\"}" + Ends;
    private const string ConvertedName = SortBy + "{\"kind\":\"lit\",\"value\":\"assigned_room\"}" + Ends;

    [Test]
    public void a_field_uses_the_apps_snake_case_name()
    {
        var field = Field.Of((Resident r) => r.MultiWord);
        var html = Views.Of(new Resident(), json => json.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        var plan = html.ReactivePlan<Resident>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<ResidentRoster>("roster").SortBy(field)));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + SnakeCase + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(SnakeCase), Is.Empty);
    }

    [Test]
    public void a_field_uses_the_apps_camel_case_name()
    {
        var field = Field.Of((Resident r) => r.MultiWord);
        var html = Views.Of(new Resident(), json => json.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);
        var plan = html.ReactivePlan<Resident>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<ResidentRoster>("roster").SortBy(field)));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + CamelCase + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(CamelCase), Is.Empty);
    }

    [Test]
    public void a_field_keeps_its_csharp_name_when_the_app_has_no_naming_policy()
    {
        var field = Field.Of((Resident r) => r.MultiWord);
        var html = Views.Of(new Resident(), json => json.PropertyNamingPolicy = null);
        var plan = html.ReactivePlan<Resident>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<ResidentRoster>("roster").SortBy(field)));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + ClrName + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(ClrName), Is.Empty);
    }

    [Test]
    public void a_fields_explicit_json_name_takes_precedence_over_the_apps_policy()
    {
        var field = Field.Of((Resident r) => r.CarePriority);
        var html = Views.Of(new Resident(), json => json.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        var plan = html.ReactivePlan<Resident>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<ResidentRoster>("roster").SortBy(field)));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + ExplicitName + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(ExplicitName), Is.Empty);
    }

    [Test]
    public void a_nested_field_names_each_member_in_its_json_path()
    {
        var field = Field.Of((Resident r) => r.Room.Number);
        var html = Views.Of(new Resident(), json => json.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        var plan = html.ReactivePlan<Resident>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<ResidentRoster>("roster").SortBy(field)));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + NestedName + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(NestedName), Is.Empty);
    }

    [Test]
    public void a_field_argument_uses_each_calling_apps_json_options()
    {
        Arg<Field> field = Field.Of((Resident r) => r.MultiWord);
        var snake = Views.Of(new Resident(), json => json.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        var snakePlan = snake.ReactivePlan<Resident>();
        snake.On(snakePlan, trigger => trigger.DomReady(pipeline => pipeline.Component<ResidentRoster>("roster").SortBy(field)));
        var camel = Views.Of(new Resident(), json => json.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);
        var camelPlan = camel.ReactivePlan<Resident>();
        camel.On(camelPlan, trigger => trigger.DomReady(pipeline => pipeline.Component<ResidentRoster>("roster").SortBy(field)));

        var snakeRendered = Views.TextOf(snake.RenderPlan(snakePlan));
        var camelRendered = Views.TextOf(camel.RenderPlan(camelPlan));

        Assert.That(snakeRendered, Is.EqualTo(ScriptOpen + SnakeCase + ScriptClose));
        Assert.That(camelRendered, Is.EqualTo(ScriptOpen + CamelCase + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(SnakeCase), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(CamelCase), Is.Empty);
    }

    [Test]
    public void a_field_that_selects_the_row_itself_is_refused_when_the_view_renders()
    {
        var field = Field.Of((Resident r) => r);
        var html = Views.Of(new Resident());
        var plan = html.ReactivePlan<Resident>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<ResidentRoster>("roster").SortBy(field)));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'r => r' names no member path, so the browser has no member to read; read a member and the members past it, such as record => record.Detail.Name."));
    }

    [Test]
    public void a_field_that_selects_a_strings_length_is_refused_when_the_view_renders()
    {
        var field = Field.Of((Resident r) => r.Name.Length);
        var html = Views.Of(new Resident());
        var plan = html.ReactivePlan<Resident>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<ResidentRoster>("roster").SortBy(field)));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'r => r.Name.Length' reads a member of 'string' that the app's JSON options do not write, so the browser never holds it."));
    }

    [Test]
    public void a_field_that_calls_a_method_is_refused_when_the_view_renders()
    {
        var field = Field.Of((Resident r) => r.Name.ToUpperInvariant());
        var html = Views.Of(new Resident());
        var plan = html.ReactivePlan<Resident>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<ResidentRoster>("roster").SortBy(field)));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'r => r.Name.ToUpperInvariant()' names no member path, so the browser has no member to read; read a member and the members past it, such as record => record.Detail.Name."));
    }

    [Test]
    public void a_field_the_apps_json_ignores_is_refused_when_the_view_renders()
    {
        var field = Field.Of((Resident r) => r.PrivateNote);
        var html = Views.Of(new Resident());
        var plan = html.ReactivePlan<Resident>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<ResidentRoster>("roster").SortBy(field)));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'r => r.PrivateNote' reads a member of 'Resident' that the app's JSON options do not write, so the browser never holds it."));
    }

    [Test]
    public void a_field_can_name_a_member_its_own_converter_writes()
    {
        var field = Field.Of((Resident r) => r.AssignedRoom);
        var html = Views.Of(new Resident(), json => json.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        var plan = html.ReactivePlan<Resident>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<ResidentRoster>("roster").SortBy(field)));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + ConvertedName + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(ConvertedName), Is.Empty);
    }

    [Test]
    public void a_field_cannot_name_a_member_past_its_own_converter()
    {
        var field = Field.Of((Resident r) => r.AssignedRoom.Number);
        var html = Views.Of(new Resident());
        var plan = html.ReactivePlan<Resident>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<ResidentRoster>("roster").SortBy(field)));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'r.AssignedRoom.Number' reads past 'r.AssignedRoom', which its own JSON converter writes, so the browser holds what the converter writes, not its members; read 'r.AssignedRoom' itself, or read past it on the server."));
    }
}
