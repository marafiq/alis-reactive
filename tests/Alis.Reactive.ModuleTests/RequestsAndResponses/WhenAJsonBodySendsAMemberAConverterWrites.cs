using System;
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
    public void an_object_read_from_a_live_object_is_refused_and_one_read_from_the_servers_json_is_sent()
    {
        Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Post<SearchRequest>("/notes", request => request.Json(body => body.Set(r => r.Address, pipeline.Component<SearchPanel>("panel").Address)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'r => r.Address' holds an object as a live object holds it, and a JSON body sends an object as the app's JSON options write it, which the browser cannot read from a live object; set it from a literal, or from JSON the server writes."));
        var sent = RequestPage.Rendered(pipeline => pipeline.Get("/notes/address")
            .OnSuccess<SearchAddress>((found, pipeline) => pipeline.Post<SearchRequest>("/notes", request => request.Json(body => body.Set(r => r.Address, found.Body)))));
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
