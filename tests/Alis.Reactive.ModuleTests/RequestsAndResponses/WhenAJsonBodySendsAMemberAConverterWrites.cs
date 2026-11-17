using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.Vendors;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.RequestsAndResponses;

[TestFixture]
public sealed class WhenAJsonBodySendsAMemberAConverterWrites
{
    [Test]
    public void a_member_read_from_a_member_its_own_converter_writes_is_refused_and_one_no_converter_writes_is_sent()
    {
        Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Post<SearchRequest>("/notes", request => request.Json(body => body.Set(r => r.On, pipeline.Component<WirePanel>("wire").Previous.Read(item => item.On))))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'r => r.On' is read from a member its own JSON converter writes, and a JSON body sends a member in the form its endpoint binds, which the browser cannot read from that writing; set it from a literal, or send it in form data."));
        var sent = RequestPage.Rendered(pipeline => pipeline.Post<SearchRequest>("/notes", request => request.Json(body => body.Set(r => r.On, pipeline.Component<SearchPanel>("panel").On))));
        Assert.That(PlanSchema.ProblemsOf(sent), Is.Empty);
    }

    [Test]
    public void a_member_a_converter_of_the_apps_writes_read_from_a_live_object_is_refused_and_one_no_converter_writes_is_sent()
    {
        Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Post<SearchRequest>("/notes", request => request.Json(body => body.Set(r => r.Id, pipeline.Component<SearchPanel>("panel").Count))),
                json => json.Converters.Add(new CountTextConverter())),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'r => r.Id' holds what a JSON converter of the app's writes, and a JSON body sends a member in the form its endpoint binds, which the browser cannot read from that writing; set it from a literal, or send it in form data."));
        var sent = RequestPage.Rendered(pipeline => pipeline.Post<SearchRequest>("/notes", request => request.Json(body => body.Set(r => r.Id, pipeline.Component<SearchPanel>("panel").Count))));
        Assert.That(PlanSchema.ProblemsOf(sent), Is.Empty);
    }

    [Test]
    public void rows_a_live_object_holds_are_sent_as_held_with_each_date_as_its_wire_text()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Post<BilledSave>("/billing", request => request.Json(body => body.Set(r => r.Changed, pipeline.Component<BillingPanel>("billing").Changed))));
        Assert.That(RequestPage.Field(RequestPage.Send(plan, "body"), "value").GetRawText(), Is.EqualTo(
            """{"kind":"as","expr":{"kind":"record","fields":[{"name":"changed","value":{"kind":"as","expr":{"kind":"ref","name":"requestValue1"},"shape":{"kind":"array","item":{"kind":"object","fields":{"dueOn":{"shape":{"kind":"string","date":"date","negativeZero":"0"},"nothing":"omit"}}}}}}]},"shape":{"kind":"object","fields":{"changed":{"shape":{"kind":"array","item":{"kind":"object","fields":{}}},"nothing":"omit"}}}}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void rows_a_live_object_holds_whose_date_a_converter_of_the_apps_writes_are_refused_and_rows_under_the_apps_default_options_are_sent()
    {
        Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Post<BilledSave>("/billing", request => request.Json(body => body.Set(r => r.Changed, pipeline.Component<BillingPanel>("billing").Changed))),
                json => json.Converters.Add(new DayConverter())),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'r => r.Changed' holds 'dueOn', which a JSON converter of the app's writes, and a JSON body sends it in the form its endpoint binds, which the browser cannot read from that writing; set it from a literal, or set its other members."));
        var sent = RequestPage.Rendered(pipeline => pipeline.Post<BilledSave>("/billing", request => request.Json(body => body.Set(r => r.Changed, pipeline.Component<BillingPanel>("billing").Changed))));
        Assert.That(PlanSchema.ProblemsOf(sent), Is.Empty);
    }

    [Test]
    public void rows_a_live_object_holds_whose_amount_a_converter_of_the_apps_writes_are_refused_and_rows_under_the_apps_default_options_are_sent()
    {
        Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Post<BilledSave>("/billing", request => request.Json(body => body.Set(r => r.Changed, pipeline.Component<BillingPanel>("billing").Changed))),
                json => json.Converters.Add(new CentsConverter())),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'r => r.Changed' holds 'amount', which a JSON converter of the app's writes, and a JSON body sends it in the form its endpoint binds, which the browser cannot read from that writing; set it from a literal, or set its other members."));
        var sent = RequestPage.Rendered(pipeline => pipeline.Post<BilledSave>("/billing", request => request.Json(body => body.Set(r => r.Changed, pipeline.Component<BillingPanel>("billing").Changed))));
        Assert.That(PlanSchema.ProblemsOf(sent), Is.Empty);
    }

    [Test]
    public void rows_a_live_object_holds_whose_enum_number_the_endpoint_does_not_read_are_refused_and_rows_whose_number_it_reads_are_sent()
    {
        Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Post<BilledSave>("/billing", request => request.Json(body => body.Set(r => r.Changed, pipeline.Component<BillingPanel>("billing").Changed))),
                json => json.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'r => r.Changed' holds 'status', whose enum a live object holds as 0 for Due, a form its endpoint does not read as Due; let the endpoint read the form the live object holds, or send the rows from JSON the server writes."));
        var sent = RequestPage.Rendered(pipeline => pipeline.Post<BilledSave>("/billing", request => request.Json(body => body.Set(r => r.Changed, pipeline.Component<BillingPanel>("billing").Changed))),
            json => json.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));
        Assert.That(PlanSchema.ProblemsOf(sent), Is.Empty);
    }

    [Test]
    public void an_object_from_the_servers_json_holding_a_date_its_own_converter_writes_is_refused_and_one_holding_a_plain_date_is_sent()
    {
        Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get("/notes/shift")
                .OnSuccess<WrittenShift>((found, pipeline) => pipeline.Post<ShiftNote>("/notes", request => request.Json(body => body.Set(r => r.Previous, found.Body))))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'r => r.Previous' holds 'on', which its own JSON converter writes, and a JSON body sends it in the form its endpoint binds, which the browser cannot read from that writing; set it from a literal, or set its other members."));
        var sent = RequestPage.Rendered(pipeline => pipeline.Get("/notes/shift")
            .OnSuccess<HandedShift>((found, pipeline) => pipeline.Post<ShiftNote>("/notes", request => request.Json(body => body.Set(r => r.Handed, found.Body)))));
        Assert.That(PlanSchema.ProblemsOf(sent), Is.Empty);
    }

    [Test]
    public void a_value_its_converter_writes_as_a_list_is_kept_as_a_list_and_left_out_when_it_reads_nothing()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Post<BedMove>("/beds", request => request.Json(body => body.Set(r => r.Spot, pipeline.Component<BedPanel>("bed").Spot))));
        Assert.That(RequestPage.Field(RequestPage.Send(plan, "body"), "value").GetRawText(), Is.EqualTo(
            """{"kind":"as","expr":{"kind":"record","fields":[{"name":"spot","value":{"kind":"ref","name":"requestValue1"}}]},"shape":{"kind":"object","fields":{"spot":{"shape":{"kind":"array","item":{"kind":"any"}},"nothing":"omit"}}}}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_value_its_converter_writes_as_null_for_its_default_is_sent_as_the_null_it_holds_and_left_out_when_it_reads_nothing()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Post<RoomChange>("/rooms", request => request.Json(body => body.Set(r => r.Code, pipeline.Component<RoomPanel>("room").Code))));
        Assert.That(RequestPage.Field(RequestPage.Send(plan, "body"), "value").GetRawText(), Is.EqualTo(
            """{"kind":"as","expr":{"kind":"record","fields":[{"name":"code","value":{"kind":"ref","name":"requestValue1"}}]},"shape":{"kind":"object","fields":{"code":{"shape":{"kind":"any"},"nothing":"omit"}}}}"""));
        Assert.That(RequestPage.Field(RequestPage.Send(plan, "body"), "guarded").GetRawText(), Is.EqualTo(
            """{"kind":"list","items":[{"kind":"record","fields":[{"name":"path","value":{"kind":"list","items":[{"kind":"lit","value":"code"}]}},{"name":"when","value":{"kind":"op","op":"or","args":[{"kind":"op","op":"eq","args":[{"kind":"ref","name":"requestValue1"},{"kind":"lit","value":null}]},{"kind":"op","op":"neq","args":[{"kind":"ref","name":"requestValue1"},{"kind":"lit","value":null}]}]}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }
}

public sealed class BilledSave
{
    public IEnumerable<BilledRow> Changed { get; init; } = [];
}

public sealed class BilledRow
{
    public string Name { get; init; } = "";

    public DateTime DueOn { get; init; }

    public decimal Amount { get; init; }

    public BillStatus Status { get; init; }
}

public sealed class CentsConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.GetInt64() / 100m;

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options) => writer.WriteNumberValue(decimal.ToInt64(value * 100m));
}

public enum BillStatus
{
    Due,
    Paid,
}

public sealed class BillingPanel : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public ReadOnly<IReadOnlyList<BilledRow>> Changed => ReadOnly<IReadOnlyList<BilledRow>>("changed");
}

public sealed class RoomChange
{
    public RoomCode Code { get; init; } = new("A1");
}

[JsonConverter(typeof(RoomCodeAsText))]
public readonly struct RoomCode(string text)
{
    public string? Text { get; } = text;
}

public sealed class RoomCodeAsText : JsonConverter<RoomCode>
{
    public override RoomCode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType == JsonTokenType.Null ? default : new RoomCode(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, RoomCode value, JsonSerializerOptions options)
    {
        if (value.Text is null) writer.WriteNullValue();
        else writer.WriteStringValue(value.Text);
    }
}

public sealed class RoomPanel : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public ReadOnly<RoomCode> Code => ReadOnly<RoomCode>("code");
}

public sealed class ShiftNote
{
    public WrittenShift Previous { get; init; } = new();

    public HandedShift Handed { get; init; } = new();
}

public sealed class HandedShift
{
    public DateTime On { get; init; }
}

public sealed class BedMove
{
    public BedSpot Spot { get; init; } = new(0, 0);
}

[JsonConverter(typeof(BedSpotAsList))]
public readonly record struct BedSpot(int Row, int Column);

public sealed class BedSpotAsList : JsonConverter<BedSpot>
{
    public override BedSpot Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray) throw new JsonException("A bed spot is the list of its row and its column.");
        var spot = JsonSerializer.Deserialize<int[]>(ref reader, options)!;
        return new BedSpot(spot[0], spot[1]);
    }

    public override void Write(Utf8JsonWriter writer, BedSpot value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.Row);
        writer.WriteNumberValue(value.Column);
        writer.WriteEndArray();
    }
}

public sealed class BedPanel : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public ReadOnly<BedSpot> Spot => ReadOnly<BedSpot>("spot");
}
