using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Vendors;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.ValuesAndSources;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenASourcesEnumIsWrittenIntoALiveObject
{
    private const string StageRows = """{"kind":"enum","flags":false,"values":[{"held":"waiting","live":0,"number":0},{"held":"ready","live":1,"number":0},{"held":"held over","live":2,"number":0}]}""";

    [Test]
    public void an_enum_the_apps_json_names_is_mapped_to_the_number_a_live_object_holds_each_name_as_the_app_writes_it()
    {
        var plan = Rendered(NamesInCamelCase, pipeline => pipeline.Get("/turnover").OnSuccess<TurnoverReply>((reply, pipeline) =>
            pipeline.Component<TurnoverPanel>("panel").Stage.Set(reply.Body.Read(b => b.Stage))));

        Assert.That(SetOf(plan, "stage").GetProperty("kind").GetString(), Is.EqualTo("as"));
        Assert.That(SetOf(plan, "stage").GetProperty("shape").GetRawText(), Is.EqualTo(StageRows));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void an_enum_both_forms_write_alike_is_written_as_held()
    {
        var plan = Rendered(_ => { }, pipeline => pipeline.Get("/turnover").OnSuccess<TurnoverReply>((reply, pipeline) =>
            pipeline.Component<TurnoverPanel>("panel").Stage.Set(reply.Body.Read(b => b.Stage))));

        Assert.That(SetOf(plan, "stage").GetProperty("kind").GetString(), Is.EqualTo("data"));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_source_a_live_object_already_holds_is_written_as_held()
    {
        var plan = Rendered(NamesInCamelCase, pipeline =>
            pipeline.Component<TurnoverPanel>("panel").Stage.Set(pipeline.Component<TurnoverPanel>("other").Stage));

        Assert.That(SetOf(plan, "stage").GetRawText(), Is.EqualTo("""{"kind":"get","path":["stage"],"members":"required","target":{"kind":"obj","id":"other"}}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_flags_enum_the_app_names_carries_each_values_number_the_largest_first()
    {
        var plan = Rendered(NamesInCamelCase, pipeline => pipeline.Get("/turnover").OnSuccess<TurnoverReply>((reply, pipeline) =>
            pipeline.Component<TurnoverPanel>("panel").Days.Set(reply.Body.Read(b => b.Days))),
            services => services.AddReactiveFluentValidation(validation => validation.Flags<TurnoverDays>()));

        Assert.That(SetOf(plan, "days").GetProperty("shape").GetRawText(), Is.EqualTo(
            """{"kind":"enum","flags":true,"values":[{"held":"weekend","live":96,"number":96},{"held":"sun","live":64,"number":64},{"held":"sat","live":32,"number":32},{"held":"mon","live":1,"number":1},{"held":"none","live":0,"number":0}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_flags_enum_the_app_does_not_name_is_mapped_value_by_value()
    {
        var plan = Rendered(NamesInCamelCase, pipeline => pipeline.Get("/turnover").OnSuccess<TurnoverReply>((reply, pipeline) =>
            pipeline.Component<TurnoverPanel>("panel").Days.Set(reply.Body.Read(b => b.Days))));

        Assert.That(SetOf(plan, "days").GetProperty("shape").GetRawText(), Is.EqualTo(
            """{"kind":"enum","flags":false,"values":[{"held":"none","live":0,"number":0},{"held":"mon","live":1,"number":0},{"held":"sat","live":32,"number":0},{"held":"sun","live":64,"number":0},{"held":"weekend","live":96,"number":0}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_flags_enum_its_type_writes_by_name_is_mapped_to_the_names_a_live_object_holds()
    {
        var plan = Rendered(NamesInCamelCase, pipeline => pipeline.Get("/turnover").OnSuccess<TurnoverReply>((reply, pipeline) =>
            pipeline.Component<TurnoverPanel>("panel").Rounds.Set(reply.Body.Read(b => b.Rounds))),
            services => services.AddReactiveFluentValidation(validation => validation.Flags<NamedRounds>()));

        Assert.That(SetOf(plan, "rounds").GetProperty("shape").GetRawText(), Is.EqualTo(
            """{"kind":"enum","flags":true,"values":[{"held":"night","live":"Night","number":2},{"held":"day","live":"Day","number":1}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_nullable_enum_is_mapped_inside_nullable_so_its_null_stays_null()
    {
        var plan = Rendered(NamesInCamelCase, pipeline => pipeline.Get("/turnover").OnSuccess<TurnoverReply>((reply, pipeline) =>
            pipeline.Component<TurnoverPanel>("panel").Next.Set(reply.Body.Read(b => b.Next))));

        Assert.That(SetOf(plan, "next").GetProperty("shape").GetRawText(), Is.EqualTo("""{"kind":"nullable","inner":""" + StageRows + "}"));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void an_enum_is_mapped_at_its_depth_in_a_list_and_in_a_record_in_a_list_and_a_member_its_own_converter_writes_is_left_as_held()
    {
        var plan = Rendered(NamesInCamelCase, pipeline => pipeline.Get("/turnover").OnSuccess<TurnoverReply>((reply, pipeline) =>
            pipeline.Component<TurnoverPanel>("panel").Beds.Set(reply.Body.Read(b => b.Beds))));

        Assert.That(SetOf(plan, "beds").GetProperty("shape").GetRawText(), Is.EqualTo(
            """{"kind":"array","item":{"kind":"object","fields":{"stage":{"shape":""" + StageRows + ""","nothing":"omit"},"stages":{"shape":{"kind":"array","item":""" + StageRows
            + """},"nothing":"omit"},"next":{"shape":{"kind":"nullable","inner":""" + StageRows + """},"nothing":"null"}}}}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void an_elements_text_html_and_appended_text_show_the_enum_as_its_source_holds_it()
    {
        var plan = Rendered(NamesInCamelCase, pipeline => pipeline.Get("/turnover").OnSuccess<TurnoverReply>((reply, pipeline) =>
        {
            pipeline.Element("stage").Text.Set(reply.Body.Read(b => b.Stage));
            pipeline.Element("card").Html.Set(reply.Body.Read(b => b.Stage));
            pipeline.Element("log").Text.Append(reply.Body.Read(b => b.Stage));
        }));

        Assert.That(SetOf(plan, "textContent").GetProperty("shape").GetRawText(), Is.EqualTo("""{"kind":"string","date":"none","negativeZero":"0"}"""));
        Assert.That(SetOf(plan, "textContent").GetProperty("expr").GetProperty("kind").GetString(), Is.EqualTo("data"));
        Assert.That(SetOf(plan, "innerHTML").GetProperty("expr").GetProperty("kind").GetString(), Is.EqualTo("data"));
        Assert.That(plan, Does.Not.Contain("\"kind\":\"enum\""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_json_body_keeps_an_objects_enum_in_the_form_its_endpoint_binds()
    {
        var plan = Rendered(NamesInCamelCase, pipeline => pipeline.Get("/turnover").OnSuccess<TurnoverReply>((reply, pipeline) =>
            pipeline.Post<SavedBed>("/beds", request => request.Json(body => body.Set(r => r.Bed, reply.Body.Read(b => b.Bed))))));

        Assert.That(plan, Does.Not.Contain("\"kind\":\"enum\""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_value_a_converter_of_the_apps_writes_is_refused_naming_its_read()
        => Assert.That(() => Rendered(DatesDayFirst, pipeline => pipeline.Get("/turnover").OnSuccess<TurnoverReply>((reply, pipeline) =>
                pipeline.Component<TurnoverPanel>("panel").Day.Set(reply.Body.Read(b => b.Day)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A live object takes 'b.Day' as JavaScript holds its type, and the app's JSON converter writes it otherwise; pass the value from the server."));

    [Test]
    public void a_body_a_converter_of_the_apps_writes_whole_is_refused_naming_its_type()
        => Assert.That(() => Rendered(DatesDayFirst, pipeline => pipeline.Get("/day").OnSuccess<DateTime>((reply, pipeline) =>
                pipeline.Component<TurnoverPanel>("panel").Day.Set(reply.Body))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A live object takes 'DateTime' as JavaScript holds its type, and the app's JSON converter writes it otherwise; pass the value from the server."));

    [Test]
    public void a_member_at_depth_a_converter_of_the_apps_writes_is_refused_naming_the_read_and_the_member_and_renders_under_the_apps_own_writing()
    {
        Assert.That(() => Rendered(DatesDayFirst, pipeline => pipeline.Get("/turnover").OnSuccess<TurnoverReply>((reply, pipeline) =>
                pipeline.Component<TurnoverPanel>("panel").Dated.Set(reply.Body.Read(b => b.Dated)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A live object takes 'b.Dated' as JavaScript holds its type, and the app's JSON converter writes 'DatedBed.Day' otherwise; give the member a JSON converter of its own, or pass the value from the server."));

        var plan = Rendered(_ => { }, pipeline => pipeline.Get("/turnover").OnSuccess<TurnoverReply>((reply, pipeline) =>
            pipeline.Component<TurnoverPanel>("panel").Dated.Set(reply.Body.Read(b => b.Dated))));
        Assert.That(SetOf(plan, "dated").GetProperty("shape").GetRawText(), Is.EqualTo(
            """{"kind":"array","item":{"kind":"object","fields":{"day":{"shape":{"kind":"date"},"nothing":"omit"}}}}"""));
    }

    [Test]
    public void a_body_passed_whole_names_the_member_alone()
        => Assert.That(() => Rendered(DatesDayFirst, pipeline => pipeline.Get("/dated").OnSuccess<IReadOnlyList<DatedBed>>((reply, pipeline) =>
                pipeline.Component<TurnoverPanel>("panel").Dated.Set(reply.Body))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A live object takes 'DatedBed.Day' as JavaScript holds its type, and the app's JSON converter writes 'DatedBed.Day' otherwise; give the member a JSON converter of its own, or pass the value from the server."));

    [Test]
    public void a_dictionary_whose_values_hold_an_enum_the_apps_converter_writes_is_refused()
        => Assert.That(() => Rendered(NamesInCamelCase, pipeline => pipeline.Get("/turnover").OnSuccess<TurnoverReply>((reply, pipeline) =>
                pipeline.Component<TurnoverPanel>("panel").Rooms.Set(reply.Body.Read(b => b.Rooms)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A value written into a live member is a dictionary whose values hold an enum the app's JSON converter writes. Such an enum reaches a live member by its member's name: pass the entries as a list of objects that name each key."));

    [Test]
    public void a_type_that_holds_itself_and_an_enum_the_apps_converter_writes_is_refused()
        => Assert.That(() => Rendered(NamesInCamelCase, pipeline => pipeline.Get("/turnover").OnSuccess<TurnoverReply>((reply, pipeline) =>
                pipeline.Component<TurnoverPanel>("panel").Chain.Set(reply.Body.Read(b => b.Chain)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A value written into a live member holds its own type and an enum the app's JSON converter writes in it. Such an enum reaches a live member through a shape that ends: pass the values as a list of their own."));

    [Test]
    public void two_values_the_apps_converter_writes_in_one_form_are_refused()
        => Assert.That(() => Rendered(json => json.Converters.Add(new EveryWingAsOneText()), pipeline => pipeline.Get("/turnover").OnSuccess<TurnoverReply>((reply, pipeline) =>
                pipeline.Component<TurnoverPanel>("panel").Wing.Set(reply.Body.Read(b => b.Wing)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'b.Wing' holds North and South in one form, \"wing\", under the app's options, so the browser cannot tell which one it holds; give one of them a JsonStringEnumMemberName of its own."));

    private static void NamesInCamelCase(JsonSerializerOptions json) => json.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

    private static void DatesDayFirst(JsonSerializerOptions json) => json.Converters.Add(new DayFirstDate());

    private static string Rendered(Action<JsonSerializerOptions> json, Action<Pipeline<TurnoverDesk>> write, Action<IServiceCollection>? app = null)
    {
        var html = Views.Of(new TurnoverDesk(), json, services =>
        {
            app?.Invoke(services);
            services.AddReactiveRequests(requests => requests.NoAntiforgery());
        });
        var plan = html.ReactivePlan<TurnoverDesk>();
        html.On(plan, trigger => trigger.DomReady(write));
        var rendered = Views.TextOf(html.RenderPlan(plan));
        return rendered["<script type=\"application/json\" data-reactive-plan>".Length..^"</script>".Length];
    }

    private static JsonElement SetOf(string plan, string member)
    {
        using var document = JsonDocument.Parse(plan);
        return Sets(document.RootElement, member).Single().Clone();
    }

    private static IEnumerable<JsonElement> Sets(JsonElement node, string member) => node.ValueKind switch
    {
        JsonValueKind.Object when IsTheSet(node, member) => [node.GetProperty("value")],
        JsonValueKind.Object => node.EnumerateObject().SelectMany(held => Sets(held.Value, member)),
        JsonValueKind.Array => node.EnumerateArray().SelectMany(item => Sets(item, member)),
        _ => [],
    };

    private static bool IsTheSet(JsonElement node, string member)
        => node.TryGetProperty("kind", out var kind) && kind.ValueKind == JsonValueKind.String && kind.GetString() == "set"
            && node.TryGetProperty("path", out var path) && path.ValueKind == JsonValueKind.Array && path.GetArrayLength() == 1
            && path[0].ValueKind == JsonValueKind.String && path[0].GetString() == member;
}

public sealed class TurnoverDesk;

public enum TurnoverStage
{
    Waiting,
    Ready,

    [JsonStringEnumMemberName("held over")]
    HeldOver,
}

public enum TurnoverWing
{
    North,
    South,
}

[Flags]
public enum TurnoverDays
{
    None = 0,
    Mon = 1,
    Sat = 32,
    Sun = 64,
    Weekend = Sat | Sun,
}

[Flags]
[JsonConverter(typeof(JsonStringEnumConverter<NamedRounds>))]
public enum NamedRounds
{
    Day = 1,
    Night = 2,
}

public sealed class TurnoverBed
{
    public string Name { get; init; } = "";

    public TurnoverStage Stage { get; init; }

    public IReadOnlyList<TurnoverStage> Stages { get; init; } = [];

    public TurnoverStage? Next { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter<TurnoverWing>))]
    public TurnoverWing Wing { get; init; }
}

public sealed class DatedBed
{
    public string Name { get; init; } = "";

    public DateTime Day { get; init; }
}

public sealed class BedChain
{
    public TurnoverStage Stage { get; init; }

    public BedChain? Previous { get; init; }
}

public sealed class RoomStages
{
    public Dictionary<string, TurnoverStage> ByRoom { get; init; } = [];
}

public sealed class SavedBed
{
    public TurnoverBed Bed { get; init; } = new();
}

public sealed class TurnoverReply
{
    public TurnoverStage Stage { get; init; }

    public TurnoverStage? Next { get; init; }

    public TurnoverDays Days { get; init; }

    public NamedRounds Rounds { get; init; }

    public TurnoverWing Wing { get; init; }

    public DateTime Day { get; init; }

    public TurnoverBed Bed { get; init; } = new();

    public IReadOnlyList<TurnoverBed> Beds { get; init; } = [];

    public IReadOnlyList<DatedBed> Dated { get; init; } = [];

    public RoomStages Rooms { get; init; } = new();

    public BedChain Chain { get; init; } = new();
}

public sealed class TurnoverPanel : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public Property<TurnoverStage> Stage => Property<TurnoverStage>();

    public Property<TurnoverStage?> Next => Property<TurnoverStage?>();

    public Property<TurnoverDays> Days => Property<TurnoverDays>();

    public Property<NamedRounds> Rounds => Property<NamedRounds>();

    public Property<TurnoverWing> Wing => Property<TurnoverWing>();

    public Property<DateTime> Day => Property<DateTime>();

    public Property<IReadOnlyList<TurnoverBed>> Beds => Property<IReadOnlyList<TurnoverBed>>();

    public Property<IReadOnlyList<DatedBed>> Dated => Property<IReadOnlyList<DatedBed>>();

    public Property<RoomStages> Rooms => Property<RoomStages>();

    public Property<BedChain> Chain => Property<BedChain>();
}

public sealed class DayFirstDate : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DateTime.ParseExact(reader.GetString()!, "dd.MM.yyyy", CultureInfo.InvariantCulture);

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture));
}

public sealed class EveryWingAsOneText : JsonConverter<TurnoverWing>
{
    public override TurnoverWing Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => TurnoverWing.North;

    public override void Write(Utf8JsonWriter writer, TurnoverWing value, JsonSerializerOptions options) => writer.WriteStringValue("wing");
}
