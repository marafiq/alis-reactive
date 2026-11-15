using System;
using System.Linq;
using System.Text.Json;
using Alis.Reactive.Conditions;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.ValuesAndSources;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Conditions;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAConditionReadsDataTheServerWrote
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";

    private const string HandedOverAt = "{\"kind\":\"op\",\"op\":\"wallKey\",\"args\":[" +
        "{\"kind\":\"data\",\"path\":[{\"name\":\"at\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"0001-01-01T00:00:00\"}}],\"target\":" +
        "{\"kind\":\"get\",\"path\":[\"value\"],\"members\":\"optional\",\"target\":{\"kind\":\"get\",\"path\":[\"body\"],\"members\":\"required\",\"target\":{\"kind\":\"ref\",\"name\":\"reply1\"}}}}]}";

    [Test]
    public void a_date_the_server_wrote_is_ordered_by_its_wall_key()
    {
        var plan = Rendered((handover, cart) => handover.Lt(new DateTime(2026, 10, 31, 7, 0, 0)), _ => { });

        Assert.That(Condition(plan).GetRawText(), Is.EqualTo(
            "{\"kind\":\"op\",\"op\":\"lt\",\"args\":[" + HandedOverAt + ",{\"kind\":\"op\",\"op\":\"wallKey\",\"args\":[{\"kind\":\"lit\",\"value\":\"2026-10-31T07:00:00\"}]}]}"));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_date_the_server_wrote_and_one_a_component_holds_are_each_keyed_by_their_own_form()
    {
        var plan = Rendered((handover, cart) => handover.Eq(cart.Due), _ => { });

        Assert.That(Condition(plan).GetRawText(), Is.EqualTo(
            "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + HandedOverAt + "," +
            "{\"kind\":\"op\",\"op\":\"dateKey\",\"args\":[{\"kind\":\"get\",\"path\":[\"due\"],\"members\":\"required\",\"target\":{\"kind\":\"obj\",\"id\":\"cart\"}}]}]}"));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_date_the_apps_converter_writes_is_not_ordered_and_the_refusal_names_its_read()
        => Assert.That(
            () => Rendered((handover, cart) => handover.Lt(new DateTime(2026, 10, 31, 7, 0, 0)), json => json.Converters.Add(new DayText())),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'handover.At' compares a DateTime the app's JSON options write by a converter of their own, whose text the browser cannot read as the value .NET holds; compare it on the server."));

    [Test]
    public void a_date_the_apps_converter_writes_is_not_compared_with_a_date_a_component_holds()
        => Assert.That(
            () => Rendered((handover, cart) => handover.Eq(cart.Due), json => json.Converters.Add(new DayText())),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'handover.At' compares a DateTime the app's JSON options write by a converter of their own, whose text the browser cannot read as the value .NET holds; compare it on the server."));

    [Test]
    public void a_date_a_component_holds_is_ordered_by_its_date_key_whatever_converter_the_app_adds()
    {
        var plan = Rendered((handover, cart) => cart.Due.Lt(new DateTime(2026, 10, 31, 7, 0, 0)), json => json.Converters.Add(new DayText()));

        Assert.That(Condition(plan).GetRawText(), Is.EqualTo(
            "{\"kind\":\"op\",\"op\":\"lt\",\"args\":[" +
            "{\"kind\":\"op\",\"op\":\"dateKey\",\"args\":[{\"kind\":\"get\",\"path\":[\"due\"],\"members\":\"required\",\"target\":{\"kind\":\"obj\",\"id\":\"cart\"}}]}," +
            "{\"kind\":\"op\",\"op\":\"dateKey\",\"args\":[{\"kind\":\"lit\",\"value\":\"2026-10-31T07:00:00\"}]}]}"));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    private static string Rendered(Func<TypedSource<DateTime>, DoseCart, TypedSource<bool>> condition, Action<JsonSerializerOptions> json)
    {
        var html = Views.Of(new DoseRound("North"), json);
        var plan = html.ReactivePlan<DoseRound>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var cart = pipeline.Component<DoseCart>("cart");
            pipeline.Get("/handover").OnSuccess<WardHandover>((answer, pipeline) => pipeline
                .When(condition(answer.Body.Read(handover => handover.At), cart))
                .Then(pipeline => pipeline.Element("handover").Text.Set("early")));
        }));
        var rendered = Views.TextOf(html.RenderPlan(plan));
        return rendered[ScriptOpen.Length..^ScriptClose.Length];
    }

    private static JsonElement Condition(string plan)
    {
        var request = JsonDocument.Parse(plan).RootElement.GetProperty("handlers")[0].GetProperty("do").EnumerateArray()
            .Single(step => step.GetProperty("kind").GetString() == "request");
        return request.GetProperty("success").GetProperty("do")[0].GetProperty("when");
    }
}

public sealed record WardHandover(string Ward, DateTime At);
