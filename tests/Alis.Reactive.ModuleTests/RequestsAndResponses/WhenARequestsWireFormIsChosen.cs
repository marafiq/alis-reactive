using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Vendors;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.RequestsAndResponses;

[TestFixture]
public sealed class WhenARequestsWireFormIsChosen
{
    [Test]
    public void registered_flags_carry_their_numbers_largest_first()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get<FlagQuery>("/staff", request => request.Query(r => r.Shifts, pipeline.Component<WirePanel>("wire").Flags)),
            services: services => services.AddReactiveFluentValidation(validation => validation.Flags<NamedShifts>()));
        Assert.That(Form(plan).GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"flags"}},{"name":"values","value":{"kind":"list","items":[{"kind":"record","fields":[{"name":"held","value":{"kind":"lit","value":2}},{"name":"name","value":{"kind":"lit","value":"Night"}},{"name":"number","value":{"kind":"lit","value":2}}]},{"kind":"record","fields":[{"name":"held","value":{"kind":"lit","value":1}},{"name":"name","value":{"kind":"lit","value":"Day"}},{"name":"number","value":{"kind":"lit","value":1}}]},{"kind":"record","fields":[{"name":"held","value":{"kind":"lit","value":0}},{"name":"name","value":{"kind":"lit","value":"None"}},{"name":"number","value":{"kind":"lit","value":0}}]}]}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_nullable_enums_own_converter_writes_each_row_at_its_nullable_type()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/staff", request => request.Query(r => r.OptionalShift, pipeline.Component<WirePanel>("wire").Previous.Read(item => item.Shift))));
        Assert.That(Form(plan).GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"enum"}},{"name":"values","value":{"kind":"list","items":[{"kind":"record","fields":[{"name":"held","value":{"kind":"lit","value":"nullable-None"}},{"name":"name","value":{"kind":"lit","value":"None"}}]},{"kind":"record","fields":[{"name":"held","value":{"kind":"lit","value":"nullable-Day"}},{"name":"name","value":{"kind":"lit","value":"Day"}}]},{"kind":"record","fields":[{"name":"held","value":{"kind":"lit","value":"nullable-Night"}},{"name":"name","value":{"kind":"lit","value":"Night"}}]},{"kind":"record","fields":[{"name":"held","value":{"kind":"lit","value":"nullable-Both"}},{"name":"name","value":{"kind":"lit","value":"Both"}}]}]}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void nullable_enum_lists_leave_an_empty_field_for_a_missing_item()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/staff", request => request.Query(r => r.Shifts, pipeline.Component<SearchPanel>("search").Shifts)));
        Assert.That(RequestPage.Field(Form(plan), "itemWhenNothing").GetRawText(), Is.EqualTo("""{"kind":"lit","value":"empty"}"""));
        Assert.That(RequestPage.Field(RequestPage.Field(Form(plan), "item"), "kind").GetRawText(), Is.EqualTo("""{"kind":"lit","value":"enum"}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_literal_flags_combination_needs_no_rows()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get<FlagQuery>("/staff", request => request.Query(r => r.Shifts, NamedShifts.Day | NamedShifts.Night)));
        Assert.That(RequestPage.Field(Field(plan), "value").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"lit","value":"Day, Night"}}]}"""));
        Assert.That(Form(plan).GetRawText(), Is.EqualTo("""{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"text"}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void app_json_enum_sources_keep_the_apps_rows_when_chained()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get("/shift").OnSuccess<RequestShift>((reply, pipeline) => { })
            .Chained((reply, pipeline) => pipeline.Get<SearchRequest>("/staff", request => request.Query(r => r.Shift, reply.Body))),
            json => json.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));
        var next = RequestPage.Request(plan).GetProperty("chained")[1];
        var field = RequestPage.Field(RequestPage.Field(next.GetProperty("send"), "query"), "fields").GetProperty("items")[0];
        Assert.That(RequestPage.Field(RequestPage.Field(field, "form"), "values").GetProperty("items")[1].GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"held","value":{"kind":"lit","value":"day"}},{"name":"name","value":{"kind":"lit","value":"Day"}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void two_enum_values_the_browser_cannot_distinguish_are_refused()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get("/shift").OnSuccess<RequestShift>((reply, pipeline) =>
            pipeline.Get<SearchRequest>("/staff", request => request.Query(r => r.Shift, reply.Body))),
            json => json.Converters.Add(new SameShiftConverter())),
            Throws.InvalidOperationException.With.Message.EqualTo("'r => r.Shift' holds None and Day in one form, \"same\", under the app's options, so the browser cannot tell which one it holds; give one of them a JsonStringEnumMemberName of its own."));

    [Test]
    public void an_enum_written_as_an_object_is_refused()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get("/shift").OnSuccess<RequestShift>((reply, pipeline) =>
            pipeline.Get<SearchRequest>("/staff", request => request.Query(r => r.Shift, reply.Body))),
            json => json.Converters.Add(new ObjectShiftConverter())),
            Throws.InvalidOperationException.With.Message.EqualTo("'r => r.Shift' holds None as {\"code\":0} under its own JSON converter, and a request sends one text or number per member; send a member written as text or a number."));

    [Test]
    public void a_number_written_as_text_keeps_the_written_wire_form()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/staff", request => request.Query(r => r.Count, pipeline.Component<WirePanel>("wire").Previous.Read(item => item.Count))));
        Assert.That(Form(plan).GetRawText(), Is.EqualTo("""{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"written"}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void app_number_handling_is_carried_by_the_wire_form()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get("/count").OnSuccess<int>((reply, pipeline) =>
            pipeline.Get<SearchRequest>("/staff", request => request.Query(r => r.Id, reply.Body))),
            json => json.NumberHandling = JsonNumberHandling.WriteAsString);
        var next = RequestPage.Request(plan).GetProperty("success").GetProperty("do")[1];
        var field = RequestPage.Field(RequestPage.Field(next.GetProperty("send"), "query"), "fields").GetProperty("items")[0];
        Assert.That(RequestPage.Field(field, "form").GetRawText(), Is.EqualTo("""{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"written"}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_dates_own_converter_does_not_make_it_a_path_value()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/staff/{On}", request => request.Path(r => r.On, pipeline.Component<WirePanel>("wire").Previous.Read(item => item.On)))),
            Throws.InvalidOperationException.With.Message.EqualTo("'r => r.On' is a date, and a date cannot fill the placeholder On: IIS refuses a time in a path segment. Name the member so it goes in the query."));

    [Test]
    public void a_source_written_by_an_app_converter_carries_the_written_form()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get("/count").OnSuccess<int>((reply, pipeline) =>
            pipeline.Get<SearchRequest>("/staff", request => request.Query(r => r.Id, reply.Body))),
            json => json.Converters.Add(new CountTextConverter()));
        var next = RequestPage.Request(plan).GetProperty("success").GetProperty("do")[1];
        var field = RequestPage.Field(RequestPage.Field(next.GetProperty("send"), "query"), "fields").GetProperty("items")[0];
        Assert.That(RequestPage.Field(field, "form").GetRawText(), Is.EqualTo("""{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"written"}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void an_app_json_date_becomes_a_date_before_its_wire_form_reads_it()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get("/date").OnSuccess<DateTime>((reply, pipeline) =>
            pipeline.Get<SearchRequest>("/staff", request => request.Query(r => r.On, reply.Body))));
        var capture = RequestPage.Request(plan).GetProperty("success").GetProperty("do")[0];
        Assert.That(capture.GetProperty("expr").GetRawText(), Is.EqualTo(
            """{"kind":"as","expr":{"kind":"get","path":["value"],"members":"optional","target":{"kind":"get","path":["body"],"members":"required","target":{"kind":"ref","name":"reply1"}}},"shape":{"kind":"date"}}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void dates_instants_and_booleans_each_carry_their_scalar_form()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/staff", request => request
            .Query(r => r.On, new DateTime(2026, 11, 15, 9, 30, 0))
            .Query(r => r.At, new DateTimeOffset(2026, 11, 15, 9, 30, 0, TimeSpan.FromHours(-5)))
            .Query(r => r.Active, true)));
        var fields = RequestPage.Field(RequestPage.Send(plan, "query"), "fields").GetProperty("items");
        Assert.That(RequestPage.Field(fields[0], "form").GetRawText(), Is.EqualTo("""{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"date"}}]}"""));
        Assert.That(RequestPage.Field(fields[1], "form").GetRawText(), Is.EqualTo("""{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"instant"}}]}"""));
        Assert.That(RequestPage.Field(fields[2], "form").GetRawText(), Is.EqualTo("""{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"boolean"}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void both_source_path_overloads_preserve_their_captured_values()
    {
        var optional = RequestPage.Rendered(pipeline => pipeline.Head<SearchRequest>("/staff/{count}", request => request.Path(r => r.Count, pipeline.Component<SearchPanel>("search").Count)));
        var nullable = RequestPage.Rendered(pipeline => pipeline.Options<SearchRequest>("/staff/{count}", request => request.Path(r => r.Count, pipeline.Component<SearchPanel>("search").OptionalCount)));
        Assert.That(RequestPage.Steps(optional)[0].GetRawText(), Is.EqualTo(
            """{"kind":"let","name":"requestValue1","await":false,"binds":"line","expr":{"kind":"get","path":["count"],"members":"required","target":{"kind":"obj","id":"search"}}}"""));
        Assert.That(RequestPage.Steps(nullable)[0].GetRawText(), Is.EqualTo(
            """{"kind":"let","name":"requestValue1","await":false,"binds":"line","expr":{"kind":"get","path":["optionalCount"],"members":"required","target":{"kind":"obj","id":"search"}}}"""));
        Assert.That(PlanSchema.ProblemsOf(optional), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(nullable), Is.Empty);
    }

    private static JsonElement Field(string plan) => RequestPage.Field(RequestPage.Send(plan, "query"), "fields").GetProperty("items")[0];

    private static JsonElement Form(string plan) => RequestPage.Field(Field(plan), "form");
}

[Flags]
public enum NamedShifts
{
    None,
    Day,
    Night,
}

public sealed class FlagQuery
{
    public NamedShifts Shifts { get; init; }
}

public sealed class WrittenShift
{
    [JsonConverter(typeof(NullableShiftConverter))]
    public RequestShift? Shift { get; init; }

    [JsonNumberHandling(JsonNumberHandling.WriteAsString)]
    public int Count { get; init; }

    [JsonConverter(typeof(DayConverter))]
    public DateTime On { get; init; }
}

public sealed class WirePanel : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public ReadOnly<NamedShifts> Flags => ReadOnly<NamedShifts>("flags");

    public ReadOnly<WrittenShift> Previous => ReadOnly<WrittenShift>("previous");
}

public sealed class NullableShiftConverter : JsonConverter<RequestShift?>
{
    public override RequestShift? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => null;

    public override void Write(Utf8JsonWriter writer, RequestShift? value, JsonSerializerOptions options) => writer.WriteStringValue("nullable-" + value);
}

public sealed class SameShiftConverter : JsonConverter<RequestShift>
{
    public override RequestShift Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => RequestShift.None;

    public override void Write(Utf8JsonWriter writer, RequestShift value, JsonSerializerOptions options) => writer.WriteStringValue("same");
}

public sealed class ObjectShiftConverter : JsonConverter<RequestShift>
{
    public override RequestShift Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => RequestShift.None;

    public override void Write(Utf8JsonWriter writer, RequestShift value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("code", (int)value);
        writer.WriteEndObject();
    }
}

public sealed class DayConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => new(2026, 11, 15);

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) => writer.WriteStringValue("Sunday");
}

public sealed class CountTextConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => 7;

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
}
