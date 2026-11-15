using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.ArrayLambdas;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.ValuesAndSources;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.ArrayLambdas;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAListsLambdaReadsDataTheServerWrote
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";

    private const string Item = "{\"kind\":\"ref\",\"name\":\"item1\"}";

    private const string DueWallKey = "{\"kind\":\"op\",\"op\":\"wallKey\",\"args\":[" +
        "{\"kind\":\"data\",\"path\":[{\"name\":\"due\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"0001-01-01T00:00:00\"}}],\"target\":" + Item + "}]}";

    private static readonly DateTime ThreeOnTheNight = new(2026, 3, 8, 3, 0, 0);

    [Test]
    public void a_date_the_server_wrote_is_compared_by_its_wall_key()
    {
        var plan = Rendered(doses => doses.Count(d => d.Due < ThreeOnTheNight), _ => { });

        Assert.That(Predicate(plan).GetRawText(), Is.EqualTo(
            "{\"kind\":\"op\",\"op\":\"lt\",\"args\":[" + DueWallKey + ",{\"kind\":\"op\",\"op\":\"wallKey\",\"args\":[{\"kind\":\"lit\",\"value\":\"2026-03-08T03:00:00\"}]}]}"));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_date_part_of_a_date_the_server_wrote_reads_its_wall_key()
    {
        var plan = Rendered(doses => doses.Count(d => d.Due.Hour == 3), _ => { });

        Assert.That(Predicate(plan).GetRawText(), Is.EqualTo(
            "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[{\"kind\":\"op\",\"op\":\"hour\",\"args\":[" + DueWallKey + "]},{\"kind\":\"lit\",\"value\":3}]}"));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void an_ordering_against_an_enum_the_app_writes_by_name_is_refused_naming_the_enum_as_written()
        => Assert.That(
            () => Rendered(doses => doses.Count(d => d.Status > DoseStatus.Due), json => json.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Status > DoseStatus.Due' orders a value the app's JSON writes as text, which does not order as the value does in .NET; compare it with == or !=, or order on the server."));

    [Test]
    public void a_char_the_view_holds_compared_inside_the_lambda_is_named_as_written()
    {
        var initial = 'B';

        Assert.That(
            () => Rendered(doses => doses.Count(d => initial == 'B'), _ => { }),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'initial == 'B'' computes a value inside a list's lambda; compute it into a local before the lambda, such as var ward = Model.Ward.ToUpper();."));
    }

    [Test]
    public void a_double_the_apps_json_writes_as_text_is_not_compared()
        => Assert.That(
            () => Rendered(doses => doses.Count(d => d.Volume == 0.0), json => json.NumberHandling = JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Volume == 0.0' compares a float or double the JSON writes as text, whose text does not equal as the number does, as -0 and 0 or NaN and NaN; compare it on the server."));

    [Test]
    public void a_double_the_apps_json_writes_as_a_number_is_compared_as_that_number()
    {
        var plan = Rendered(doses => doses.Count(d => d.Volume == 0.0), _ => { });

        Assert.That(Predicate(plan).GetRawText(), Is.EqualTo(
            "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" +
            "{\"kind\":\"data\",\"path\":[{\"name\":\"volume\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":0}}],\"target\":" + Item + "}," +
            "{\"kind\":\"lit\",\"value\":0}]}"));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_member_its_own_converter_writes_is_refused_naming_the_apps_options()
        => Assert.That(
            () => Rendered(doses => doses.Count(d => d.Charted > DoseStatus.Due), _ => { }),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Charted > DoseStatus.Due' orders 'd.Charted', which its own JSON converter writes unlike the app's options, so the browser does not order it as .NET does; order it on the server."));

    private static string Rendered(Func<TypedSource<List<CartDose>>, TypedSource<int>> count, Action<JsonSerializerOptions> json)
    {
        var html = Views.Of(new MedicationRound("North", null), json);
        var plan = html.ReactivePlan<MedicationRound>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Get("/doses")
            .OnSuccess<List<CartDose>>((doses, pipeline) => pipeline.Element("count").Text.Set(count(doses.Body)))));
        var rendered = Views.TextOf(html.RenderPlan(plan));
        return rendered[ScriptOpen.Length..^ScriptClose.Length];
    }

    private static JsonElement Predicate(string plan)
    {
        var request = JsonDocument.Parse(plan).RootElement.GetProperty("handlers")[0].GetProperty("do").EnumerateArray()
            .Single(step => step.GetProperty("kind").GetString() == "request");
        var counted = request.GetProperty("success").GetProperty("do")[0].GetProperty("value").GetProperty("expr");
        return counted.GetProperty("target").GetProperty("args")[0].GetProperty("body");
    }
}
