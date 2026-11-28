using System;
using System.Collections.Generic;
using System.Text.Json;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.RequestsAndResponses;

[TestFixture]
public sealed class WhenARequestSetsAMember
{
    private const string Multipart = """{"kind":"lit","value":"multipart"}""";

    [Test]
    public void a_get_fills_a_placeholder_with_the_member_named_like_it_ignoring_case_and_queries_every_other_member()
    {
        var plan = RequestPage.Rendered(pipeline =>
        {
            var panel = pipeline.Component<SearchPanel>("panel");
            pipeline.Get<SearchRequest>("/residents/{id}", request => request.Set(r => r.Id, 7).Set(r => r.Address.Ward, "North").Set(r => r.Name, panel.Name).Set(r => r.Count, panel.Count));
        });
        Assert.That(RequestPage.Send(plan, "url").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"parts","value":{"kind":"list","items":[{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"text"}},{"name":"text","value":{"kind":"lit","value":"/residents/"}}]},{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"member"}},{"name":"name","value":{"kind":"lit","value":"Id"}},{"name":"value","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"lit","value":7}}]}},{"name":"form","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"number"}}]}}]}]}},{"name":"query","value":{"kind":"lit","value":""}}]}"""));
        Assert.That(Names(RequestPage.Field(RequestPage.Send(plan, "query"), "fields")), Is.EqualTo(new[] { "Address.Ward", "Name", "Count" }));
        Assert.That(RequestPage.Send(plan, "body").GetRawText(), Is.EqualTo("""{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"none"}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_post_sends_a_member_that_fills_no_placeholder_as_a_multipart_part_and_one_that_fills_it_in_the_path_and_the_body()
    {
        var plan = RequestPage.Rendered(pipeline =>
        {
            var panel = pipeline.Component<SearchPanel>("panel");
            pipeline.Post<SearchRequest>("/notes/{ID}", request => request.Set(r => r.Name, panel.Name).Set(r => r.Id, 7).Set(r => r.Address.Ward, "North").Set(r => r.Count, panel.Count));
        });
        var body = RequestPage.Send(plan, "body");
        Assert.That(RequestPage.Field(body, "kind").GetRawText(), Is.EqualTo(Multipart));
        Assert.That(PartNames(RequestPage.Field(body, "parts")), Is.EqualTo(new[] { "Id", "Name", "Address.Ward", "Count" }));
        Assert.That(RequestPage.Field(RequestPage.Send(plan, "url"), "parts").GetProperty("items")[1].GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"member"}},{"name":"name","value":{"kind":"lit","value":"Id"}},{"name":"value","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"lit","value":7}}]}},{"name":"form","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"number"}}]}}]}"""));
        Assert.That(Names(RequestPage.Field(RequestPage.Send(plan, "query"), "fields")), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_put_whose_one_member_fills_its_placeholder_sends_that_member_in_a_multipart_body_too()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Put<SearchRequest>("/notes/{id}", request => request.Set(r => r.Id, 7)));
        var body = RequestPage.Send(plan, "body");
        Assert.That(RequestPage.Field(body, "kind").GetRawText(), Is.EqualTo(Multipart));
        Assert.That(PartNames(RequestPage.Field(body, "parts")), Is.EqualTo(new[] { "Id" }));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_member_set_after_the_request_names_its_body_goes_in_that_body()
    {
        var json = RequestPage.Rendered(pipeline => pipeline.Delete<SearchRequest>("/notes/{id}", request => request.Json(body => body.Set(r => r.Name, "Ada")).Set(r => r.Id, 7).Set(r => r.Active, true)));
        var jsonBody = RequestPage.Send(json, "body");
        Assert.That(RequestPage.Field(jsonBody, "kind").GetRawText(), Is.EqualTo("""{"kind":"lit","value":"json"}"""));
        Assert.That(RequestPage.Field(jsonBody, "value").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"id","value":{"kind":"lit","value":7}},{"name":"not.the.binding","value":{"kind":"lit","value":"Ada"}},{"name":"active","value":{"kind":"lit","value":true}}]}"""));
        var form = RequestPage.Rendered(pipeline =>
        {
            var panel = pipeline.Component<SearchPanel>("panel");
            pipeline.Put<SearchRequest>("/notes/{id}", request => request.UrlEncoded(body => body.Set(r => r.Address.Ward, "North")).Set(r => r.Id, 7).Set(r => r.Name, panel.Name).Set(r => r.Count, panel.Count));
        });
        var formBody = RequestPage.Send(form, "body");
        Assert.That(RequestPage.Field(formBody, "kind").GetRawText(), Is.EqualTo("""{"kind":"lit","value":"urlEncoded"}"""));
        Assert.That(Names(RequestPage.Field(formBody, "fields")), Is.EqualTo(new[] { "Id", "Address.Ward", "Name", "Count" }));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(form), Is.Empty);
    }

    [Test]
    public void a_nested_member_named_like_a_placeholder_is_queried_so_the_placeholder_it_leaves_unfilled_is_refused()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/staff/{Ward}", request => request.Set(r => r.Address.Ward, "North"))),
            Throws.InvalidOperationException.With.Message.EqualTo("The URL '/staff/{Ward}' has the placeholder Ward, and the request has no member named Ward; add the member, or take the placeholder out of the URL."));

    [Test]
    public void a_date_named_like_a_placeholder_is_refused_since_iis_refuses_a_time_in_a_path_segment()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Post<SearchRequest>("/rounds/{on}", request => request.Set(r => r.On, new DateTime(2026, 1, 12)))),
            Throws.InvalidOperationException.With.Message.EqualTo("'r => r.On' is a date, and a date cannot fill the placeholder On: IIS refuses a time in a path segment. Name the member so it goes in the query."));

    [Test]
    public void a_member_set_twice_is_refused()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Post<SearchRequest>("/notes", request => request.Set(r => r.Name, "Ada").Set(r => r.Name, "Ben"))),
            Throws.InvalidOperationException.With.Message.EqualTo("Request member 'Name' overlaps a member already supplied; send each member once, and send either an object or its members."));

    private static string[] Names(JsonElement fields)
    {
        var names = new List<string>();
        foreach (var field in fields.GetProperty("items").EnumerateArray()) names.Add(RequestPage.Field(field, "name").GetProperty("value").GetString()!);
        return [.. names];
    }

    private static string[] PartNames(JsonElement parts)
    {
        var names = new List<string>();
        foreach (var part in parts.GetProperty("items").EnumerateArray()) names.Add(RequestPage.Field(RequestPage.Field(part, "field"), "name").GetProperty("value").GetString()!);
        return [.. names];
    }
}
