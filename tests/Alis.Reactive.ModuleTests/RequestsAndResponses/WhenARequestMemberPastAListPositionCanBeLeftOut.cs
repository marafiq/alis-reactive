using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Declarations;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Vendors;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.RequestsAndResponses;

[TestFixture]
public sealed class WhenARequestMemberPastAListPositionCanBeLeftOut
{
    [Test]
    public void a_query_member_read_from_a_date_is_refused_where_it_is_its_items_only_member()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get<RoundBooking>("/bookings", request => request
                .Query(r => r.Visits[0].Note, "Bloods")
                .Query(r => r.Visits[1].When, pipeline.Component<RoundDesk>("desk").Due))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "The request sends 'Visits[1].When' in its query past a list position from a value it leaves out when it reads nothing, and no member of 'Visits[1]' is always sent, so the item would send no field and the endpoint's list would end before it on both hosts; send beside it a member of the item that is always sent, a literal, a text or a nullable member outside every condition, or send the list in a JSON body."));

    [Test]
    public void a_query_member_under_a_condition_is_refused_where_its_item_holds_no_member_always_sent()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get<RoundBooking>("/bookings", request => request
                .When(pipeline.Component<RoundDesk>("desk").Urgent).Then(urgent => urgent.Query(r => r.Visits[0].Note, "Urgent")))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "The request sends 'Visits[0].Note' in its query past a list position under a condition, which leaves it out where the condition fails, and no member of 'Visits[0]' is always sent, so the item would send no field and the endpoint's list would end before it on both hosts; send beside it a member of the item that is always sent, a literal, a text or a nullable member outside every condition, or send the list in a JSON body."));

    [Test]
    public void a_query_member_read_from_a_date_under_a_condition_is_refused_where_it_is_its_items_only_member()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get<RoundBooking>("/bookings", request => request
                .When(pipeline.Component<RoundDesk>("desk").Urgent).Then(urgent => urgent.Query(r => r.Visits[0].When, pipeline.Component<RoundDesk>("desk").Due)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "The request sends 'Visits[0].When' in its query past a list position under a condition, from a value it leaves out when it reads nothing, and no member of 'Visits[0]' is always sent, so the item would send no field and the endpoint's list would end before it on both hosts; send beside it a member of the item that is always sent, a literal, a text or a nullable member outside every condition, or send the list in a JSON body."));

    [Test]
    public void a_form_data_member_read_from_a_date_is_refused_where_it_is_its_items_only_member()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Post<RoundBooking>("/bookings", request => request.UrlEncoded(body => body
                .Set(r => r.Visits[0].When, pipeline.Component<RoundDesk>("desk").Due)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "The request sends 'Visits[0].When' in its form data past a list position from a value it leaves out when it reads nothing, and no member of 'Visits[0]' is always sent, so the item would send no field and the endpoint's list would end before it on both hosts; send beside it a member of the item that is always sent, a literal, a text or a nullable member outside every condition, or send the list in a JSON body."));

    [Test]
    public void a_query_member_read_from_a_date_is_sent_beside_a_literal_or_a_text_of_its_item()
    {
        var literal = RequestPage.Rendered(pipeline => pipeline.Get<RoundBooking>("/bookings", request => request
            .Query(r => r.Visits[0].When, pipeline.Component<RoundDesk>("desk").Due).Query(r => r.Visits[0].Note, "Review")));
        var text = RequestPage.Rendered(pipeline => pipeline.Get<RoundBooking>("/bookings", request => request
            .Query(r => r.Visits[0].When, pipeline.Component<RoundDesk>("desk").Due).Query(r => r.Visits[0].Note, pipeline.Component<RoundDesk>("desk").Note)));
        Assert.That(NamesOf(literal), Is.EqualTo(new[] { """{"kind":"lit","value":"Visits[0].When"}""", """{"kind":"lit","value":"Visits[0].Note"}""" }));
        Assert.That(NamesOf(text), Is.EqualTo(new[] { """{"kind":"lit","value":"Visits[0].When"}""", """{"kind":"lit","value":"Visits[0].Note"}""" }));
        Assert.That(PlanSchema.ProblemsOf(literal), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(text), Is.Empty);
    }

    private static string[] NamesOf(string plan)
        => RequestPage.Field(RequestPage.Send(plan, "query"), "fields").GetProperty("items").EnumerateArray().Select(field => RequestPage.Field(field, "name").GetRawText()).ToArray();
}

public sealed class RoundBooking
{
    public List<RoundBookingVisit> Visits { get; init; } = [];
}

public sealed class RoundBookingVisit
{
    public DateTime When { get; init; }

    public string? Note { get; init; }
}

public sealed class RoundDesk : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public ReadOnly<DateTime> Due => ReadOnly<DateTime>("due");

    public ReadOnly<bool> Urgent => ReadOnly<bool>("urgent");

    public ReadOnly<string?> Note => ReadOnly<string?>("note");
}
