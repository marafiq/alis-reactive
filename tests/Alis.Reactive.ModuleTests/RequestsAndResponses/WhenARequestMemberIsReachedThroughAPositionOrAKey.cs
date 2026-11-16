using System;
using System.Collections.Generic;
using System.Text.Json;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.RequestsAndResponses;

[TestFixture]
public sealed class WhenARequestMemberIsReachedThroughAPositionOrAKey
{
    [Test]
    public void a_json_body_nests_a_member_past_a_list_position_in_an_array_at_that_position()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Post<WardOrder>("/orders", request => request.Json(body => body.Set(r => r.Items[0].Name, "Aspirin"))));
        Assert.That(RequestPage.Field(RequestPage.Send(plan, "body"), "value").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"items","value":{"kind":"list","items":[{"kind":"record","fields":[{"name":"name","value":{"kind":"lit","value":"Aspirin"}}]}]}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_json_body_nests_a_dictionary_entry_as_a_member_named_by_its_key()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Post<WardOrder>("/orders", request => request.Json(body => body.Set(r => r.Beds["E4"], 2))));
        Assert.That(RequestPage.Field(RequestPage.Send(plan, "body"), "value").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"beds","value":{"kind":"record","fields":[{"name":"E4","value":{"kind":"lit","value":2}}]}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_form_body_sends_a_member_past_a_list_position_under_its_binding_name_with_the_position_as_an_index_step()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Post<WardOrder>("/orders", request => request.UrlEncoded(body => body.Set(r => r.Items[0].Name, "Aspirin"))));
        var field = RequestPage.Field(RequestPage.Send(plan, "body"), "fields").GetProperty("items")[0];
        Assert.That(RequestPage.Field(field, "name").GetRawText(), Is.EqualTo("""{"kind":"lit","value":"Items[0].Name"}"""));
        Assert.That(RequestPage.Field(field, "path").GetRawText(), Is.EqualTo(
            """{"kind":"list","items":[{"kind":"lit","value":"Items"},{"kind":"lit","value":0},{"kind":"lit","value":"Name"}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_form_body_sends_a_dictionary_entry_under_its_binding_name_with_the_key_as_a_member_step()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Post<WardOrder>("/orders", request => request.UrlEncoded(body => body.Set(r => r.Beds["E4"], 2))));
        var field = RequestPage.Field(RequestPage.Send(plan, "body"), "fields").GetProperty("items")[0];
        Assert.That(RequestPage.Field(field, "name").GetRawText(), Is.EqualTo("""{"kind":"lit","value":"Beds[E4]"}"""));
        Assert.That(RequestPage.Field(field, "path").GetRawText(), Is.EqualTo("""{"kind":"list","items":[{"kind":"lit","value":"Beds"},{"kind":"lit","value":"E4"}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_query_sends_a_member_past_a_list_position_under_its_binding_name_with_the_position_as_an_index_step()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get<WardOrder>("/orders", request => request.Query(r => r.Items[1].Name, "Aspirin")));
        var field = QueryField(plan);
        Assert.That(RequestPage.Field(field, "name").GetRawText(), Is.EqualTo("""{"kind":"lit","value":"Items[1].Name"}"""));
        Assert.That(RequestPage.Field(field, "path").GetRawText(), Is.EqualTo(
            """{"kind":"list","items":[{"kind":"lit","value":"Items"},{"kind":"lit","value":1},{"kind":"lit","value":"Name"}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_query_sends_a_dictionary_entry_under_its_binding_name_with_the_key_as_a_member_step()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get<WardOrder>("/orders", request => request.Query(r => r.Shifts[3], "Night")));
        var field = QueryField(plan);
        Assert.That(RequestPage.Field(field, "name").GetRawText(), Is.EqualTo("""{"kind":"lit","value":"Shifts[3]"}"""));
        Assert.That(RequestPage.Field(field, "path").GetRawText(), Is.EqualTo("""{"kind":"list","items":[{"kind":"lit","value":"Shifts"},{"kind":"lit","value":"3"}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_dictionary_keyed_by_a_type_system_text_json_writes_no_property_name_for_is_refused()
    {
        var east4 = new WardRoom("East", 4);
        Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Post<WardOrder>("/orders", request => request.Set(r => r.Rooms[east4], 2))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A dictionary keyed by WardRoom cannot be sent: System.Text.Json writes no property name for its keys; key it by text or a number."));
    }

    [Test]
    public void a_json_body_that_sets_a_list_position_and_none_before_it_is_refused_as_not_built()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Post<WardOrder>("/orders", request => request.Json(body => body.Set(r => r.Items[1].Name, "Aspirin")))),
            Throws.InstanceOf<NotImplementedException>().With.Message.EqualTo(
                "The request sets a member at position 1 of a list in its JSON body and none at position 0, and no line states what a JSON body sends at a position the request leaves out; set a member at each position from 0, or send the list whole."));

    [Test]
    public void a_json_body_list_position_set_from_a_value_left_out_when_it_reads_nothing_is_refused_as_not_built()
        => Assert.That(() => RequestPage.Rendered(pipeline =>
            {
                var panel = pipeline.Component<SearchPanel>("panel");
                pipeline.Post<WardOrder>("/orders", request => request.Json(body => body.Set(r => r.Counts[0], panel.Count)));
            }),
            Throws.InstanceOf<NotImplementedException>().With.Message.EqualTo(
                "The request sets position 0 of a list in its JSON body from a value the body leaves out when it reads nothing, and no line states what a list sends at a position that reads nothing; set it from a literal, or send the list whole."));

    [Test]
    public void a_json_body_member_past_a_list_position_under_a_parts_condition_is_refused_as_not_built()
        => Assert.That(() => RequestPage.Rendered(pipeline =>
            {
                var desk = pipeline.Component<PartsDesk>("desk");
                pipeline.Post<WardOrder>("/orders", request => request.Json(body => body.When(desk.Urgent).Then(urgent => urgent.Set(r => r.Items[0].Name, "Aspirin"))));
            }),
            Throws.InstanceOf<NotImplementedException>().With.Message.EqualTo(
                "'Items[0].Name' is set past a list position under a condition on the request's parts, and a JSON body leaves out a part whose condition fails by its member path, which no node of the plan carries through a list yet; set it with no condition, or send it in form data."));

    [Test]
    public void a_json_body_enum_past_a_list_position_read_from_a_source_is_refused_as_not_built()
        => Assert.That(() => RequestPage.Rendered(pipeline =>
            {
                var panel = pipeline.Component<SearchPanel>("panel");
                pipeline.Post<WardOrder>("/orders", request => request.Json(body => body.Set(r => r.Items[0].Shift, panel.Shift)));
            }),
            Throws.InstanceOf<NotImplementedException>().With.Message.EqualTo(
                "'Items[0].Shift' is set past a list position from an enum a source reads, and a JSON body writes that enum in its endpoint's form by its member path, which no node of the plan carries through a list yet; set it from a literal, or send it in form data."));

    private static JsonElement QueryField(string plan) => RequestPage.Field(RequestPage.Send(plan, "query"), "fields").GetProperty("items")[0];
}

public sealed class WardOrder
{
    public List<OrderedItem> Items { get; init; } = [];

    public List<int> Counts { get; init; } = [];

    public Dictionary<string, int> Beds { get; init; } = [];

    public Dictionary<int, string> Shifts { get; init; } = [];

    public Dictionary<WardRoom, int> Rooms { get; init; } = [];
}

public sealed class OrderedItem
{
    public string Name { get; init; } = "";

    public RequestShift Shift { get; init; }
}

public sealed record WardRoom(string Ward, int Room);
