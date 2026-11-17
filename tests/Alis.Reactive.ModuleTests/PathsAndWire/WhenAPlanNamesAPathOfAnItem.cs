using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.RenderingAndDelivery;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.PathsAndWire;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAPlanNamesAPathOfAnItem
{
    [Test]
    public void a_holder_and_a_member_that_carry_their_json_names_are_named_by_them()
    {
        var html = Views.Of(new BedBoardView("North"));
        var plan = html.ReactivePlan<BedBoardView>();
        Assert.That(plan.JsonPath((PlacedBed b) => b.Place.RoomNumber), Is.EqualTo(new[] { "spot", "room" }));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_path_converted_to_the_lambdas_type_is_named_by_its_members()
    {
        var html = Views.Of(new BedBoardView("North"));
        var plan = html.ReactivePlan<BedBoardView>();
        Assert.That(plan.JsonPath((PlacedBed b) => (object)b.Place.RoomNumber), Is.EqualTo(new[] { "spot", "room" }));
        html.RenderPlan(plan);
    }

    [Test]
    public void each_member_is_named_in_camel_case_under_the_default_options()
    {
        var html = Views.Of(new BedBoardView("North"));
        var plan = html.ReactivePlan<BedBoardView>();
        Assert.That(plan.JsonPath((PlacedBed b) => b.Place.WardName), Is.EqualTo(new[] { "spot", "wardName" }));
        html.RenderPlan(plan);
    }

    [Test]
    public void each_member_is_named_as_the_apps_json_names_it()
    {
        var html = Views.Of(new BedBoardView("North"), json => json.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        var plan = html.ReactivePlan<BedBoardView>();
        Assert.That(plan.JsonPath((PlacedBed b) => b.Place.WardName), Is.EqualTo(new[] { "spot", "ward_name" }));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_lambda_that_is_no_chain_of_members_is_refused()
    {
        var html = Views.Of(new BedBoardView("North"));
        var plan = html.ReactivePlan<BedBoardView>();
        Action theView = () =>
        {
            plan.JsonPath((PlacedBed b) => b.Place.WardName.Trim());
            html.RenderPlan(plan);
        };
        Assert.That(theView, Throws.InvalidOperationException.With.Message.EqualTo(
            "'b => b.Place.WardName.Trim()' names no member path, so the browser has no member to read; read a member and the members past it, such as record => record.Detail.Name."));
    }

    [Test]
    public void a_path_past_a_member_its_own_converter_writes_is_refused()
    {
        var html = Views.Of(new BedBoardView("North"));
        var plan = html.ReactivePlan<BedBoardView>();
        Action theView = () =>
        {
            plan.JsonPath((PlacedBed b) => b.Tag.Code);
            html.RenderPlan(plan);
        };
        Assert.That(theView, Throws.InvalidOperationException.With.Message.EqualTo(
            "'b => b.Tag.Code' reads a member of 'BedTag' that the app's JSON options do not write, so the browser never holds it."));
    }
}

public sealed record BedBoardView(string Ward);

public sealed record PlacedBed([property: JsonPropertyName("spot")] BedPlace Place, BedTag Tag);

public sealed record BedPlace([property: JsonPropertyName("room")] int RoomNumber, string WardName);

[JsonConverter(typeof(BedTagAsText))]
public sealed record BedTag(string Code);

public sealed class BedTagAsText : JsonConverter<BedTag>
{
    public override BedTag Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => new(reader.GetString() ?? "");

    public override void Write(Utf8JsonWriter writer, BedTag value, JsonSerializerOptions options) => writer.WriteStringValue(value.Code);
}
