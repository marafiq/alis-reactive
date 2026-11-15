using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.RequestsAndResponses;

[TestFixture]
public sealed class WhenARequestPartIsWritten
{
    [Test]
    public void query_names_are_csharp_paths_even_when_json_renames_or_ignores_them()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/residents", request => request
            .Query(r => r.Name, "Ada").Query(r => r.Address.Ward, "North").Query(r => r.Ignored, "public")),
            json => json.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        Assert.That(Fields(plan).GetRawText(), Is.EqualTo(
            """{"kind":"list","items":[{"kind":"record","fields":[{"name":"name","value":{"kind":"lit","value":"Name"}},{"name":"path","value":{"kind":"list","items":[{"kind":"lit","value":"Name"}]}},{"name":"value","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"lit","value":"Ada"}}]}},{"name":"form","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"text"}}]}},{"name":"whenNothing","value":{"kind":"lit","value":"empty"}}]},{"kind":"record","fields":[{"name":"name","value":{"kind":"lit","value":"Address.Ward"}},{"name":"path","value":{"kind":"list","items":[{"kind":"lit","value":"Address"},{"kind":"lit","value":"Ward"}]}},{"name":"value","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"lit","value":"North"}}]}},{"name":"form","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"text"}}]}},{"name":"whenNothing","value":{"kind":"lit","value":"empty"}}]},{"kind":"record","fields":[{"name":"name","value":{"kind":"lit","value":"Ignored"}},{"name":"path","value":{"kind":"list","items":[{"kind":"lit","value":"Ignored"}]}},{"name":"value","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"lit","value":"public"}}]}},{"name":"form","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"text"}}]}},{"name":"whenNothing","value":{"kind":"lit","value":"empty"}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void path_members_match_ignoring_case_and_keep_the_urls_query()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("https://care.example/residents/{id}/cards?mode=short", request => request.Path(r => r.Id, 7)));
        Assert.That(RequestPage.Send(plan, "url").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"parts","value":{"kind":"list","items":[{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"text"}},{"name":"text","value":{"kind":"lit","value":"https://care.example/residents/"}}]},{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"member"}},{"name":"name","value":{"kind":"lit","value":"Id"}},{"name":"value","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"lit","value":7}}]}},{"name":"form","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"number"}}]}}]},{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"text"}},{"name":"text","value":{"kind":"lit","value":"/cards"}}]}]}},{"name":"query","value":{"kind":"lit","value":"mode=short"}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_source_is_captured_once_and_preserves_nothing_apart_from_null()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/residents", request => request.Query(r => r.Name, pipeline.Component<SearchPanel>("search").Name)));
        Assert.That(RequestPage.Steps(plan)[0].GetRawText(), Is.EqualTo(
            """{"kind":"let","name":"requestValue1","await":false,"binds":"line","expr":{"kind":"get","path":["name"],"members":"required","target":{"kind":"obj","id":"search"}}}"""));
        Assert.That(RequestPage.Field(Fields(plan).GetProperty("items")[0], "value").GetRawText(), Is.EqualTo(
            """{"kind":"op","op":"cond","args":[{"kind":"op","op":"or","args":[{"kind":"op","op":"eq","args":[{"kind":"ref","name":"requestValue1"},{"kind":"lit","value":null}]},{"kind":"op","op":"neq","args":[{"kind":"ref","name":"requestValue1"},{"kind":"lit","value":null}]}]},{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"ref","name":"requestValue1"}}]},{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"nothing"}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_literal_null_is_held_and_the_destination_controls_absence()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/residents", request => request
            .Query(r => r.Name, (string?)null).Query(r => r.Count, pipeline.Component<SearchPanel>("search").Count)));
        Assert.That(RequestPage.Field(Fields(plan).GetProperty("items")[0], "value").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"lit","value":null}}]}"""));
        Assert.That(RequestPage.Field(Fields(plan).GetProperty("items")[1], "whenNothing").GetRawText(), Is.EqualTo("""{"kind":"lit","value":"empty"}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void lists_carry_the_items_form_and_their_own_absence_rule()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/residents", request => request
            .Query(r => r.Names, new string?[] { "Ada", null }).Query(r => r.Ids, new[] { 7, 9 })));
        Assert.That(RequestPage.Field(Fields(plan).GetProperty("items")[0], "form").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"list"}},{"name":"item","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"text"}}]}},{"name":"itemWhenNothing","value":{"kind":"lit","value":"empty"}}]}"""));
        Assert.That(RequestPage.Field(Fields(plan).GetProperty("items")[1], "form").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"list"}},{"name":"item","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"number"}}]}},{"name":"itemWhenNothing","value":{"kind":"lit","value":"stop"}}]}"""));
        Assert.That(RequestPage.Field(Fields(plan).GetProperty("items")[0], "whenNothing").GetRawText(), Is.EqualTo("""{"kind":"lit","value":"leave"}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_literal_enum_uses_its_csharp_name_under_an_apps_enum_converter()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/staff", request => request.Query(r => r.Shift, RequestShift.Both)),
            json => json.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));
        Assert.That(RequestPage.Field(Fields(plan).GetProperty("items")[0], "value").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"lit","value":"Both"}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void an_enum_source_maps_the_held_values_to_csharp_names()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/staff", request => request.Query(r => r.Shift, pipeline.Component<SearchPanel>("search").Shift)),
            json => json.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));
        Assert.That(RequestPage.Field(Fields(plan).GetProperty("items")[0], "form").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"enum"}},{"name":"values","value":{"kind":"list","items":[{"kind":"record","fields":[{"name":"held","value":{"kind":"lit","value":0}},{"name":"name","value":{"kind":"lit","value":"None"}}]},{"kind":"record","fields":[{"name":"held","value":{"kind":"lit","value":1}},{"name":"name","value":{"kind":"lit","value":"Day"}}]},{"kind":"record","fields":[{"name":"held","value":{"kind":"lit","value":2}},{"name":"name","value":{"kind":"lit","value":"Night"}}]},{"kind":"record","fields":[{"name":"held","value":{"kind":"lit","value":3}},{"name":"name","value":{"kind":"lit","value":"Both"}}]}]}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void fixed_and_source_headers_keep_their_scalar_forms()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Options<SearchRequest>("/staff", request => request
            .Header("X-Requested-With", "XMLHttpRequest").Header("X-Count", pipeline.Component<SearchPanel>("search").Count)));
        Assert.That(RequestPage.Send(plan, "headers").GetProperty("items")[0].GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"name","value":{"kind":"lit","value":"X-Requested-With"}},{"name":"value","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"lit","value":"XMLHttpRequest"}}]}},{"name":"form","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"text"}}]}}]}"""));
        Assert.That(RequestPage.Field(RequestPage.Send(plan, "headers").GetProperty("items")[1], "form").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"number"}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void query_cannot_fill_a_placeholder()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/staff/{Id}", request => request.Query(r => r.Id, 7))),
            Throws.InvalidOperationException.With.Message.EqualTo("The URL '/staff/{Id}' has the placeholder Id, and the request has no member named Id; add the member, or take the placeholder out of the URL."));

    [Test]
    public void path_requires_a_matching_top_level_member()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/staff/{Ward}", request => request.Path(r => r.Address.Ward, "North"))),
            Throws.InvalidOperationException.With.Message.EqualTo("Path member 'r => r.Address.Ward' names no top-level placeholder; use Query for this member, or name a top-level member that fills the URL."));

    [Test]
    public void a_computation_is_not_a_query_member()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/staff", request => request.Query(r => r.Id + 1, 7))),
            Throws.InvalidOperationException.With.Message.EqualTo("'r => (r.Id + 1)' names no request member; name a member path, such as r => r.Address.Ward."));

    [Test]
    public void a_member_cannot_be_supplied_twice()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/staff", request => request.Query(r => r.Name, "Ada").Query(r => r.Name, "Ben"))),
            Throws.InvalidOperationException.With.Message.EqualTo("Request member 'Name' overlaps a member already supplied; send each member once, and send either an object or its members."));

    [Test]
    public void a_date_cannot_fill_a_path()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/staff/{On}", request => request.Path(r => r.On, new DateTime(2026, 11, 15)))),
            Throws.InvalidOperationException.With.Message.EqualTo("'r => r.On' is a date, and a date cannot fill the placeholder On: IIS refuses a time in a path segment. Name the member so it goes in the query."));

    [TestCase("/staff#card", "The URL '/staff#card' is not a page path or an absolute http or https URI with paired path placeholders and no fragment; write a URL of that form.")]
    [TestCase("/staff/{Id", "The URL '/staff/{Id' is not a page path or an absolute http or https URI with paired path placeholders and no fragment; write a URL of that form.")]
    [TestCase("ftp://care.example/staff", "The URL 'ftp://care.example/staff' is not a page path or an absolute http or https URI with paired path placeholders and no fragment; write a URL of that form.")]
    [TestCase("//care.example/staff", "The URL '//care.example/staff' is not a page path or an absolute http or https URI with paired path placeholders and no fragment; write a URL of that form.")]
    [TestCase("/staff with space", "The URL '/staff with space' is not a page path or an absolute http or https URI with paired path placeholders and no fragment; write a URL of that form.")]
    public void a_url_outside_the_declared_shape_is_refused(string url, string message)
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get(url)),
            Throws.InvalidOperationException.With.Message.EqualTo(message));

    [Test]
    public void a_query_literal_past_javascripts_whole_numbers_is_refused_naming_its_member()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get<BillRequest>("/bills", request => request.Query(r => r.Amount, 9007199254740993m))),
            Throws.InvalidOperationException.With.Message.EqualTo("'r => r.Amount' holds a JavaScript number, and 9007199254740993 is a whole number past 9007199254740991, which JavaScript reads as another value; write a finite number of at most 15 significant digits."));

    [Test]
    public void a_query_lists_item_past_them_is_refused_naming_its_member()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get<BillRequest>("/bills", request => request.Query(r => r.Tickets, new[] { 7L, 9007199254740992L }))),
            Throws.InvalidOperationException.With.Message.EqualTo("'r => r.Tickets' holds a JavaScript number, and 9007199254740992 is a whole number past 9007199254740991, which JavaScript reads as another value; write a finite number of at most 15 significant digits."));

    [Test]
    public void an_unnamed_enum_number_is_refused()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/staff", request => request.Query(r => r.Shift, (RequestShift)8))),
            Throws.InvalidOperationException.With.Message.EqualTo("'r => r.Shift' is set to 8, which no value of its enum names: net10 refuses it and MVC 5 binds it, so the hosts bind it apart; set a value its enum names."));

    [TestCase("Content-Type", "Content-Type is set by the body door; write Json, UrlEncoded, Multipart, Text or File.")]
    [TestCase("Accept", "Accept is set by the success route; choose its Content kind.")]
    [TestCase("X Bad", "Header 'X Bad' is not an HTTP token; use a name made of HTTP token characters.")]
    [TestCase("Cookie", "Header 'Cookie' is written by the framework or the browser; use an app header name.")]
    [TestCase("sEc-Test", "Header 'sEc-Test' is written by the framework or the browser; use an app header name.")]
    public void a_header_name_the_app_cannot_send_is_refused(string name, string message)
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/staff", request => request.Header(name, "x"))),
            Throws.InvalidOperationException.With.Message.EqualTo(message));

    [Test]
    public void headers_cannot_repeat_in_another_case()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/staff", request => request.Header("X-Ward", "North").Header("x-ward", "South"))),
            Throws.InvalidOperationException.With.Message.EqualTo("Header 'x-ward' is supplied twice; send each header name once, ignoring case."));

    [Test]
    public void a_header_with_a_line_break_is_refused()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/staff", request => request.Header("X-Ward", "North\r\nInjected: yes"))),
            Throws.InvalidOperationException.With.Message.EqualTo("The header X-Ward holds a line break, which HTTP does not carry in a field value; send it in the body."));

    [Test]
    public void a_header_cannot_send_a_list()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/staff", request => request.Header("X-Wards", pipeline.Component<SearchPanel>("search").Names))),
            Throws.InvalidOperationException.With.Message.EqualTo("Header 'X-Wards' holds a list or an object; send a scalar source, or send the member in the query or body."));

    [Test]
    public void sources_are_captured_after_the_parts_methods_finish()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/staff", request => request
            .Query(r => r.Name, pipeline.Component<SearchPanel>("search").Name)
            .Query(r => r.Id, pipeline.Component<SearchPanel>("search").CountNowAsync())));
        Assert.That(RequestPage.Steps(plan)[0].GetRawText(), Is.EqualTo(
            """{"kind":"let","name":"result1","await":true,"binds":"line","expr":{"kind":"call","path":["countNowAsync"],"members":"required","target":{"kind":"obj","id":"search"},"args":[]}}"""));
        Assert.That(RequestPage.Steps(plan)[1].GetRawText(), Is.EqualTo(
            """{"kind":"let","name":"requestValue1","await":false,"binds":"line","expr":{"kind":"get","path":["name"],"members":"required","target":{"kind":"obj","id":"search"}}}"""));
        Assert.That(RequestPage.Steps(plan)[2].GetRawText(), Is.EqualTo(
            """{"kind":"let","name":"requestValue2","await":false,"binds":"line","expr":{"kind":"ref","name":"result1"}}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    private static JsonElement Fields(string plan) => RequestPage.Field(RequestPage.Send(plan, "query"), "fields");
}
