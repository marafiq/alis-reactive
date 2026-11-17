using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    public void a_json_body_that_sets_a_list_position_and_none_before_it_is_refused_where_the_view_renders()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Post<WardOrder>("/orders", request => request.Json(body => body.Set(r => r.Items[1].Name, "Aspirin")))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "The request sets a member at position 1 of a list in its JSON body and none at position 0, and a JSON list holds an item at every position before its last, which the endpoint binds as an item the request never set; set a member at each position from 0, or send the list whole."));

    [Test]
    public void a_json_body_list_position_set_from_a_value_left_out_when_it_reads_nothing_is_refused_where_the_view_renders()
    {
        Assert.That(() => RequestPage.Rendered(pipeline =>
            {
                var panel = pipeline.Component<SearchPanel>("panel");
                pipeline.Post<WardOrder>("/orders", request => request.Json(body => body.Set(r => r.Counts[0], panel.Count)));
            }),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "The request sets position 0 of a list in its JSON body from a value the body leaves out when it reads nothing, and a JSON list holds an item at every position, which the endpoint binds as an item the request never set; set it from a literal, or send the list whole."));
        var whole = RequestPage.Rendered(pipeline => pipeline.Post<WardOrder>("/orders", request => request.Json(body => body.Set(r => r.Counts, pipeline.Component<BodyDesk>("desk").Rooms))));
        Assert.That(PlanSchema.ProblemsOf(whole), Is.Empty);
    }

    [Test]
    public void a_dictionary_keyed_by_a_bool_is_refused_where_the_view_renders()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Post<WardOrder>("/orders", request => request.Json(body => body.Set(r => r.Settled[true], 2)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A dictionary keyed by bool cannot be sent: System.Text.Json and MVC's binding name write its keys apart, as true and True, so a JSON body and form data name one entry two ways; key it by text or a number."));

    [Test]
    public void a_dictionary_keyed_by_a_date_is_refused_where_the_view_renders()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get<WardOrder>("/orders", request => request.Query(r => r.Rounds[new DateTime(2026, 11, 17)], 2))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A dictionary keyed by DateTime cannot be sent: System.Text.Json and MVC's binding name write its keys apart, as 2026-11-17T08:30:00 and 11/17/2026 08:30:00, so a JSON body and form data name one entry two ways; key it by text or a number."));

    [Test]
    public void a_dictionary_keyed_by_an_enum_the_apps_options_name_by_a_policy_is_refused_and_one_named_as_csharp_names_it_is_sent()
    {
        Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Post<WardOrder>("/orders", request => request.Json(body => body.Set(r => r.ByShift[RequestShift.Day], 2))),
                json => json.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A dictionary keyed by RequestShift cannot be sent: System.Text.Json and MVC's binding name write its keys apart, as none and None, so a JSON body and form data name one entry two ways; key it by text or a number."));
        var plan = RequestPage.Rendered(pipeline => pipeline.Post<WardOrder>("/orders", request => request.Json(body => body.Set(r => r.ByShift[RequestShift.Day], 2))));
        Assert.That(RequestPage.Field(RequestPage.Send(plan, "body"), "value").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"byShift","value":{"kind":"record","fields":[{"name":"Day","value":{"kind":"lit","value":2}}]}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_json_body_member_past_a_list_position_under_a_parts_condition_is_guarded_by_its_path_with_the_position_as_its_digits()
    {
        var plan = RequestPage.Rendered(pipeline =>
        {
            var desk = pipeline.Component<PartsDesk>("desk");
            pipeline.Post<WardOrder>("/orders", request => request.Json(body => body.Set(r => r.Items[0].Shift, RequestShift.Day)
                .When(desk.Urgent).Then(urgent => urgent.Set(r => r.Items[0].Name, "Aspirin"))));
        });
        Assert.That(RequestPage.Field(RequestPage.Send(plan, "body"), "guarded").GetRawText(), Is.EqualTo(
            """{"kind":"list","items":[{"kind":"record","fields":[{"name":"path","value":{"kind":"list","items":[{"kind":"lit","value":"items"},{"kind":"lit","value":"0"},{"kind":"lit","value":"name"}]}},{"name":"when","value":{"kind":"ref","name":"partCondition1"}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_json_body_list_item_whose_every_member_a_condition_guards_keeps_its_position_and_its_list_is_the_holder_left_out()
    {
        var plan = RequestPage.Rendered(pipeline =>
        {
            var desk = pipeline.Component<PartsDesk>("desk");
            pipeline.Post<WardOrder>("/orders", request => request.Json(body => body.When(desk.Urgent).Then(urgent => urgent.Set(r => r.Items[0].Name, "Aspirin"))));
        });
        Assert.That(RequestPage.Field(RequestPage.Send(plan, "body"), "guarded").GetRawText(), Is.EqualTo(
            """{"kind":"list","items":[""" +
            """{"kind":"record","fields":[{"name":"path","value":{"kind":"list","items":[{"kind":"lit","value":"items"}]}},{"name":"when","value":{"kind":"ref","name":"partCondition1"}}]},""" +
            """{"kind":"record","fields":[{"name":"path","value":{"kind":"list","items":[{"kind":"lit","value":"items"},{"kind":"lit","value":"0"},{"kind":"lit","value":"name"}]}},{"name":"when","value":{"kind":"ref","name":"partCondition1"}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_json_body_list_position_itself_under_a_parts_condition_is_refused_where_the_view_renders()
        => Assert.That(() => RequestPage.Rendered(pipeline =>
            {
                var desk = pipeline.Component<PartsDesk>("desk");
                pipeline.Post<WardOrder>("/orders", request => request.Json(body => body.Set(r => r.Counts[0], 2).When(desk.Urgent).Then(urgent => urgent.Set(r => r.Counts[1], 3))));
            }),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'Counts[1]' is a position of a list in the request's JSON body set under a condition on its parts, and a JSON list leaves out no position: the items after it would move into its place. Set the list's items with no condition, or set a member of the item under the condition."));

    [Test]
    public void a_json_body_enum_past_a_list_position_read_from_a_source_is_sent_by_its_path_with_the_position_as_its_digits()
    {
        var plan = RequestPage.Rendered(
            pipeline => pipeline.Post<WardOrder>("/orders", request => request.Json(body => body.Set(r => r.Items[0].Shift, pipeline.Component<SearchPanel>("panel").Shift))),
            json => json.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));
        Assert.That(RequestPage.Field(RequestPage.Send(plan, "body"), "enums").GetRawText(), Is.EqualTo(
            """{"kind":"list","items":[{"kind":"record","fields":[{"name":"name","value":{"kind":"lit","value":"Items[0].Shift"}},{"name":"path","value":{"kind":"list","items":[{"kind":"lit","value":"items"},{"kind":"lit","value":"0"},{"kind":"lit","value":"shift"}]}},{"name":"form","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"enum"}},{"name":"values","value":{"kind":"list","items":[{"kind":"record","fields":[{"name":"held","value":{"kind":"lit","value":0}},{"name":"sent","value":{"kind":"lit","value":"none"}}]},{"kind":"record","fields":[{"name":"held","value":{"kind":"lit","value":1}},{"name":"sent","value":{"kind":"lit","value":"day"}}]},{"kind":"record","fields":[{"name":"held","value":{"kind":"lit","value":2}},{"name":"sent","value":{"kind":"lit","value":"night"}}]},{"kind":"record","fields":[{"name":"held","value":{"kind":"lit","value":3}},{"name":"sent","value":{"kind":"lit","value":"both"}}]}]}}]}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    private static JsonElement QueryField(string plan) => RequestPage.Field(RequestPage.Send(plan, "query"), "fields").GetProperty("items")[0];
}

public sealed class WardOrder
{
    public List<OrderedItem> Items { get; init; } = [];

    public List<int> Counts { get; init; } = [];

    public Dictionary<string, int> Beds { get; init; } = [];

    public Dictionary<int, string> Shifts { get; init; } = [];

    public Dictionary<WardRoom, int> Rooms { get; init; } = [];

    public Dictionary<bool, int> Settled { get; init; } = [];

    public Dictionary<DateTime, int> Rounds { get; init; } = [];

    public Dictionary<RequestShift, int> ByShift { get; init; } = [];
}

public sealed class OrderedItem
{
    public string Name { get; init; } = "";

    public RequestShift Shift { get; init; }
}

public sealed record WardRoom(string Ward, int Room);
