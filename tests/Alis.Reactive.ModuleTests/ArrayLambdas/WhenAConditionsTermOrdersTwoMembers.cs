using System;
using System.Text.Json.Serialization;
using Alis.Reactive.RenderingAndDelivery;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.ArrayLambdas;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAConditionsTermOrdersTwoMembers
{
    [Test]
    public void two_numbers_are_let_through()
    {
        var html = Views.Of(new BedCheckBoard("North"));
        var plan = html.ReactivePlan<BedCheckBoard>();
        Action theView = () =>
        {
            plan.RequireMembersTheBrowserOrders((BoardBed b) => b.Done > b.Due);
            html.RenderPlan(plan);
        };
        Assert.That(theView, Throws.Nothing);
    }

    [Test]
    public void two_enums_are_refused_naming_what_the_browser_orders()
    {
        var html = Views.Of(new BedCheckBoard("North"));
        var plan = html.ReactivePlan<BedCheckBoard>();
        Action theView = () =>
        {
            plan.RequireMembersTheBrowserOrders((BoardBed b) => b.Stage > b.Wanted);
            html.RenderPlan(plan);
        };
        Assert.That(theView, Throws.InvalidOperationException.With.Message.EqualTo(
            "'b.Stage > b.Wanted' orders values the browser cannot order as .NET does; order numbers, a char, or a date or a time, or order on the server."));
    }

    [Test]
    public void two_enums_whose_type_carries_a_string_converter_are_refused_the_same()
    {
        var html = Views.Of(new BedCheckBoard("North"));
        var plan = html.ReactivePlan<BedCheckBoard>();
        Action theView = () =>
        {
            plan.RequireMembersTheBrowserOrders((BoardBed b) => b.Linen <= b.LinenWanted);
            html.RenderPlan(plan);
        };
        Assert.That(theView, Throws.InvalidOperationException.With.Message.EqualTo(
            "'b.Linen <= b.LinenWanted' orders values the browser cannot order as .NET does; order numbers, a char, or a date or a time, or order on the server."));
    }

    [Test]
    public void a_number_its_own_number_handling_writes_is_refused_beside_another()
    {
        var html = Views.Of(new BedCheckBoard("North"));
        var plan = html.ReactivePlan<BedCheckBoard>();
        Action theView = () =>
        {
            plan.RequireMembersTheBrowserOrders((BoardBed b) => b.Held > b.Due);
            html.RenderPlan(plan);
        };
        Assert.That(theView, Throws.InvalidOperationException.With.Message.EqualTo(
            "'b.Held > b.Due' compares two members, and its own JSON number handling writes 'b.Held' unlike JavaScript's value, so the browser does not compare them as .NET does; compare them on the server."));
    }

    [Test]
    public void a_term_that_orders_no_two_members_of_the_item_is_refused()
    {
        var html = Views.Of(new BedCheckBoard("North"));
        var plan = html.ReactivePlan<BedCheckBoard>();
        Action theView = () =>
        {
            plan.RequireMembersTheBrowserOrders((BoardBed b) => b.Done == b.Due);
            html.RenderPlan(plan);
        };
        Assert.That(theView, Throws.InvalidOperationException.With.Message.EqualTo(
            "'b.Done == b.Due' orders no two members of the item; pass an ordering such as m => m.Beds > m.Taken."));
    }
}

public sealed record BedCheckBoard(string Ward);

public sealed record BoardBed(
    int Done,
    int Due,
    [property: JsonNumberHandling(JsonNumberHandling.WriteAsString)] int Held,
    BoardBedStage Stage,
    BoardBedStage Wanted,
    BoardBedLinen Linen,
    BoardBedLinen LinenWanted);

public enum BoardBedStage
{
    Stripped,
    Made,
}

[JsonConverter(typeof(JsonStringEnumConverter<BoardBedLinen>))]
public enum BoardBedLinen
{
    Due,
    Changed,
}
