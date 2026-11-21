using System;
using System.Text.Json.Serialization;
using Alis.Reactive.RenderingAndDelivery;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.ArrayLambdas;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAConditionsTermHoldsAViewsValue
{
    [Test]
    public void a_value_the_view_holds_is_written_as_the_plan_writes_it()
    {
        var board = new RoomBoardView(2, true);
        var html = Views.Of(board);
        var plan = html.ReactivePlan<RoomBoardView>();
        Assert.That(plan.ViewValueJson((BoardRoom r) => r.Beds > board.MinSpare), Is.EqualTo("2"));
        html.RenderPlan(plan);
    }

    [Test]
    public void an_enum_the_compiler_folds_to_its_number_is_written_as_the_member_its_compared_with()
    {
        var html = Views.Of(new RoomBoardView(2, true));
        var plan = html.ReactivePlan<RoomBoardView>();
        Assert.That(plan.ViewValueJson((BoardRoom r) => r.State == BoardRoomState.Open), Is.EqualTo("1"));
        html.RenderPlan(plan);
    }

    [Test]
    public void an_enum_whose_type_carries_a_string_converter_is_written_by_its_name()
    {
        var html = Views.Of(new RoomBoardView(2, true));
        var plan = html.ReactivePlan<RoomBoardView>();
        Assert.That(plan.ViewValueJson((BoardRoom r) => r.Cleaning == BoardRoomCleaning.Done), Is.EqualTo("\"Done\""));
        html.RenderPlan(plan);
    }

    [Test]
    public void an_enum_whose_member_carries_its_own_converter_is_written_as_that_converter_writes_it()
    {
        var html = Views.Of(new RoomBoardView(2, true));
        var plan = html.ReactivePlan<RoomBoardView>();
        Assert.That(plan.ViewValueJson((ConvertedRoom r) => r.State == BoardRoomState.Open), Is.EqualTo("\"Open\""));
        html.RenderPlan(plan);
    }

    [Test]
    public void an_ordering_against_an_enum_written_by_its_name_is_refused()
    {
        var html = Views.Of(new RoomBoardView(2, true));
        var plan = html.ReactivePlan<RoomBoardView>();
        Action theView = () =>
        {
            plan.ViewValueJson((BoardRoom r) => r.Cleaning > BoardRoomCleaning.Due);
            html.RenderPlan(plan);
        };
        Assert.That(theView, Throws.InvalidOperationException.With.Message.EqualTo(
            "'r.Cleaning > BoardRoomCleaning.Due' orders a value the app's JSON writes as text, which does not order as the value does in .NET; compare it with == or !=, or order on the server."));
    }

    [Test]
    public void a_term_that_reads_no_member_of_the_item_is_written_as_its_value()
    {
        var board = new RoomBoardView(2, true);
        var html = Views.Of(board);
        var plan = html.ReactivePlan<RoomBoardView>();
        Assert.That(plan.ViewValueJson((BoardRoom r) => board.ShowsSpare), Is.EqualTo("true"));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_term_that_compares_two_members_of_the_item_is_refused()
    {
        var html = Views.Of(new RoomBoardView(2, true));
        var plan = html.ReactivePlan<RoomBoardView>();
        Action theView = () =>
        {
            plan.ViewValueJson((BoardRoom r) => r.Beds > r.Spare);
            html.RenderPlan(plan);
        };
        Assert.That(theView, Throws.InvalidOperationException.With.Message.EqualTo(
            "'r.Beds > r.Spare' compares no member of the item with a value the view holds; pass a comparison such as m => m.Beds > Model.MinSpare, or a term that reads no member of the item."));
    }
}

public sealed record RoomBoardView(int MinSpare, bool ShowsSpare);

public sealed record BoardRoom(int Beds, int Spare, BoardRoomState State, BoardRoomCleaning Cleaning);

public sealed record ConvertedRoom([property: JsonConverter(typeof(JsonStringEnumConverter<BoardRoomState>))] BoardRoomState State);

public enum BoardRoomState
{
    Closed,
    Open,
}

[JsonConverter(typeof(JsonStringEnumConverter<BoardRoomCleaning>))]
public enum BoardRoomCleaning
{
    Due,
    Done,
}
