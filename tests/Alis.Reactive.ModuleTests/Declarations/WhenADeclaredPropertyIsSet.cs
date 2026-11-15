using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Declarations;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenADeclaredPropertyIsSet
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";

    private const string Head =
        "{\"version\":7,\"planId\":\"Alis.Reactive.ModuleTests.Declarations.Infusion\"," +
        "\"culture\":{\"name\":\"en-US\",\"collation\":\"en-US\",\"decimal\":\".\",\"group\":\",\"," +
        "\"integer\":{\"group\":false,\"exponent\":false},\"fraction\":{\"group\":true,\"exponent\":true}}," +
        "\"timeZone\":{\"kind\":\"clock\"},\"trace\":{\"kind\":\"off\"}," +
        "\"vendors\":{\"native\":{\"locate\":{\"kind\":\"fn\",\"params\":[\"element\"],\"body\":{\"kind\":\"ref\",\"name\":\"element\"}}," +
        "\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"}}}," +
        "\"objects\":{" +
        "\"$document\":{\"kind\":\"plugin\",\"name\":\"alis.document\",\"key\":\"\"," +
        "\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"},\"data\":{\"kind\":\"none\"}}," +
        "\"pump\":{\"kind\":\"element\",\"id\":\"pump\",\"vendor\":\"native\"}";

    private const string Handlers =
        "}," +
        "\"handlers\":[{\"on\":\"$document\",\"path\":[],\"event\":\"ready\",\"payload\":[],\"members\":\"required\",\"policy\":\"overlap\",\"do\":[";

    private const string OnLoad = Head + Handlers;

    private const string OnLoadWithStandby = Head + ",\"standby\":{\"kind\":\"element\",\"id\":\"standby\",\"vendor\":\"native\"}" + Handlers;

    private const string Ends = "]}]}";

    private const string Pump = "{\"kind\":\"obj\",\"id\":\"pump\"}";

    private const string StandbyFlowRate = "{\"kind\":\"get\",\"path\":[\"flowRate\"],\"members\":\"required\",\"target\":{\"kind\":\"obj\",\"id\":\"standby\"}}";

    private const string NumbersAsJsonNumbers = OnLoad +
        "{\"kind\":\"set\",\"path\":[\"rate\"],\"members\":\"required\",\"content\":false,\"target\":" + Pump + ",\"value\":{\"kind\":\"lit\",\"value\":40}}," +
        "{\"kind\":\"set\",\"path\":[\"bolus\"],\"members\":\"required\",\"content\":false,\"target\":" + Pump + ",\"value\":{\"kind\":\"lit\",\"value\":2}}" + Ends;

    private const string DatesAsDates = OnLoad +
        "{\"kind\":\"set\",\"path\":[\"startsAt\"],\"members\":\"required\",\"content\":false,\"target\":" + Pump + "," +
        "\"value\":{\"kind\":\"as\",\"expr\":{\"kind\":\"lit\",\"value\":\"2026-11-14T08:30:00\"},\"shape\":{\"kind\":\"date\"}}}," +
        "{\"kind\":\"set\",\"path\":[\"stopsAt\"],\"members\":\"required\",\"content\":false,\"target\":" + Pump + "," +
        "\"value\":{\"kind\":\"as\",\"expr\":{\"kind\":\"lit\",\"value\":null},\"shape\":{\"kind\":\"nullable\",\"inner\":{\"kind\":\"date\"}}}}," +
        "{\"kind\":\"set\",\"path\":[\"pausedUntil\"],\"members\":\"required\",\"content\":false,\"target\":" + Pump + "," +
        "\"value\":{\"kind\":\"as\",\"expr\":{\"kind\":\"lit\",\"value\":\"2026-11-14T08:30:00-05:00\"},\"shape\":{\"kind\":\"instant\",\"from\":\"instant\"}}}" + Ends;

    private const string OptionalWrites = OnLoad +
        "{\"kind\":\"set\",\"path\":[\"label\"],\"members\":\"optional\",\"content\":false,\"target\":" + Pump + ",\"value\":{\"kind\":\"lit\",\"value\":\"Night shift\"}}," +
        "{\"kind\":\"set\",\"path\":[\"lock\"],\"members\":\"optional\",\"content\":false,\"target\":" + Pump + ",\"value\":{\"kind\":\"lit\",\"value\":null}}" + Ends;

    private const string SourcesAsHeld = OnLoadWithStandby +
        "{\"kind\":\"set\",\"path\":[\"rate\"],\"members\":\"required\",\"content\":false,\"target\":" + Pump + ",\"value\":" + StandbyFlowRate + "}," +
        "{\"kind\":\"set\",\"path\":[\"bolus\"],\"members\":\"required\",\"content\":false,\"target\":" + Pump + ",\"value\":" + StandbyFlowRate + "}," +
        "{\"kind\":\"set\",\"path\":[\"limit\"],\"members\":\"required\",\"content\":false,\"target\":" + Pump + ",\"value\":" + StandbyFlowRate + "}," +
        "{\"kind\":\"set\",\"path\":[\"lock\"],\"members\":\"optional\",\"content\":false,\"target\":" + Pump + ",\"value\":" + StandbyFlowRate + "}" + Ends;

    [Test]
    public void a_number_is_written_as_a_json_number_though_the_apps_options_write_numbers_as_text()
    {
        var html = Views.Of(new Infusion("North"), json => json.NumberHandling = JsonNumberHandling.WriteAsString);
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Rate.Set(40);
            pump.Bolus.Set(2);
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + NumbersAsJsonNumbers + ScriptClose));
    }

    [Test]
    public void a_date_is_its_wall_time_and_an_instant_its_offset_each_converted_where_it_is_set_whatever_the_apps_converter()
    {
        var html = Views.Of(new Infusion("North"), json => json.Converters.Add(new DayFirstDates()));
        var plan = html.ReactivePlan<Infusion>();
        DateTime? notYet = null;
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.StartsAt.Set(new DateTime(2026, 11, 14, 8, 30, 0, DateTimeKind.Utc));
            pump.StopsAt.Set(notYet);
            pump.PausedUntil.Set(new DateTimeOffset(2026, 11, 14, 8, 30, 0, TimeSpan.FromHours(-5)));
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + DatesAsDates + ScriptClose));
    }

    [Test]
    public void a_set_of_an_optional_member_writes_it_whether_or_not_the_object_has_it()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        int? unlocked = null;
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Label.Set("Night shift");
            pump.Lock.Set(unlocked);
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + OptionalWrites + ScriptClose));
    }

    [Test]
    public void a_live_source_is_written_as_it_is_held_and_a_value_without_null_sets_a_nullable_member()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var standby = pipeline.Component<InfusionPump>("standby");
            pump.Rate.Set(standby.FlowRate);
            pump.Bolus.Set(standby.FlowRate);
            pump.Limit.Set(standby.FlowRate);
            pump.Lock.Set(standby.FlowRate);
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + SourcesAsHeld + ScriptClose));
    }

    [Test]
    public void the_plans_of_each_set_are_valid_by_the_plan_schema()
    {
        Assert.That(PlanSchema.ProblemsOf(NumbersAsJsonNumbers), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(DatesAsDates), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(OptionalWrites), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(SourcesAsHeld), Is.Empty);
    }

    private sealed class DayFirstDates : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => DateTime.ParseExact(reader.GetString()!, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture));
    }
}
