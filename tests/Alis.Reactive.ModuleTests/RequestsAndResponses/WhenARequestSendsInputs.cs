using System;
using System.Text.Json;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.RequestsAndResponses;

[TestFixture]
public sealed class WhenARequestSendsInputs
{
    private const string Model = "Alis.Reactive.ModuleTests.RequestsAndResponses.IntakeSend";
    private const string WardInput = "Alis_Reactive_ModuleTests_RequestsAndResponses_IntakeDesk__Ward";
    private const string BedsInput = "Alis_Reactive_ModuleTests_RequestsAndResponses_IntakeDesk__Beds";
    private const string NoInputs = """{"kind":"record","fields":[{"name":"container","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"none"}}]}},{"name":"individual","value":{"kind":"list","items":[]}}]}""";
    private const string NoBody = """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"none"}}]}""";
    private const string WardText = """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"text"}}]}""";

    [Test]
    public void a_get_sends_the_inputs_a_container_holds_in_its_query()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get<IntakeSend>("/intakes", request => request.Inputs(pipeline.Container<IntakeSend>("contact"))));
        Assert.That(RequestPage.Send(plan, "query").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"fields","value":{"kind":"list","items":[]}},{"name":"inputs","value":""" + ContainerInputs("sends", "contact") + "}]}"));
        Assert.That(RequestPage.Send(plan, "body").GetRawText(), Is.EqualTo(NoBody));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_post_sends_the_inputs_a_container_holds_as_multipart()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Post<IntakeSend>("/intakes", request => request.Inputs(pipeline.Container<IntakeSend>("contact"))));
        Assert.That(RequestPage.Send(plan, "body").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"multipart"}},{"name":"parts","value":{"kind":"list","items":[]}},{"name":"inputs","value":""" + ContainerInputs("sends", "contact") + "}]}"));
        Assert.That(Inputs(RequestPage.Send(plan, "query")), Is.EqualTo(NoInputs));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void each_body_that_names_a_container_sends_its_inputs_beside_the_members_it_sets()
    {
        var json = RequestPage.Rendered(pipeline => pipeline.Post<IntakeSend>("/intakes", request => request.Json(pipeline.Container<IntakeSend>("contact"), body => body.Set(r => r.Beds, 2))));
        Assert.That(RequestPage.Send(json, "body").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"json"}},{"name":"value","value":{"kind":"record","fields":[{"name":"beds","value":{"kind":"lit","value":2}}]}},{"name":"guarded","value":{"kind":"list","items":[]}},{"name":"enums","value":{"kind":"list","items":[]}},{"name":"inputs","value":""" + ContainerInputs("sends", "contact") + "}]}"));
        var encoded = RequestPage.Rendered(pipeline => pipeline.Put<IntakeSend>("/intakes", request => request.UrlEncoded(pipeline.Container<IntakeSend>("contact"), body => body.Set(r => r.Ward, "North"))));
        Assert.That(RequestPage.Send(encoded, "body").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"urlEncoded"}},{"name":"fields","value":{"kind":"list","items":[""" + WardNorth + """]}},{"name":"inputs","value":""" + ContainerInputs("sends", "contact") + "}]}"));
        var multipart = RequestPage.Rendered(pipeline => pipeline.Delete<IntakeSend>("/intakes", request => request.Multipart(pipeline.Container<IntakeSend>("contact"), body => body.Set(r => r.Ward, "North"))));
        Assert.That(RequestPage.Send(multipart, "body").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"multipart"}},{"name":"parts","value":{"kind":"list","items":[{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"field"}},{"name":"field","value":""" + WardNorth + """}]}]}},{"name":"inputs","value":""" + ContainerInputs("sends", "contact") + "}]}"));
        var block = ContainerPlan((pipeline, handle) => pipeline.Put<IntakeSend>("/intakes", request => request.Multipart(handle, body => { })));
        Assert.That(Inputs(RequestPage.Send(block, "body")), Is.EqualTo(ContainerInputs("sends", "intake")));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(encoded), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(multipart), Is.Empty);
    }

    [Test]
    public void after_a_container_a_get_fills_its_path_query_headers_and_set_members_as_before_it()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get<IntakeSend>("/wards/{ward}", request => request.Inputs(pipeline.Container<IntakeSend>("contact"))
            .Path(r => r.Ward, "North").Query(r => r.Beds, 2).Header("X-Desk", "front").Set(r => r.Room, "R4")));
        Assert.That(RequestPage.Field(RequestPage.Send(plan, "url"), "parts").GetProperty("items")[1].GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"member"}},{"name":"name","value":{"kind":"lit","value":"Ward"}},{"name":"value","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"lit","value":"North"}}]}},{"name":"form","value":""" + WardText + "}]}"));
        Assert.That(RequestPage.Send(plan, "query").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"fields","value":{"kind":"list","items":[""" + Field("Beds", Held("""{"kind":"lit","value":2}"""), """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"number"}}]}""", "leave") + "," +
            Field("Room", Held("""{"kind":"lit","value":"R4"}"""), WardText, "empty") + """]}},{"name":"inputs","value":""" + ContainerInputs("sends", "contact") + "}]}"));
        Assert.That(RequestPage.Send(plan, "headers").GetProperty("items")[0].GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"name","value":{"kind":"lit","value":"X-Desk"}},{"name":"value","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"lit","value":"front"}}]}},{"name":"form","value":""" + WardText + """},{"name":"when","value":{"kind":"lit","value":true}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void an_input_is_read_where_the_request_starts_and_sent_under_the_request_members_name_with_its_capture()
    {
        var plan = DeskPlan(pipeline => pipeline.Get<IntakeSend>("/rooms", request => request.Input(r => r.Ward, pipeline.Input(m => m.Ward))));
        Assert.That(RequestPage.Steps(plan)[0].GetRawText(), Is.EqualTo(
            $$$"""{"kind":"let","name":"requestValue1","await":false,"binds":"line","expr":{"kind":"call","path":["valueOf"],"members":"required","target":{"kind":"obj","id":"$model"},"args":[{"kind":"lit","value":"{{{WardInput}}}"}]}}"""));
        Assert.That(RequestPage.Send(plan, "query").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"fields","value":{"kind":"list","items":[""" + Field("Ward", Read("requestValue1"), WardText, "empty") +
            """]}},{"name":"inputs","value":{"kind":"record","fields":[{"name":"container","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"none"}}]}},{"name":"individual","value":{"kind":"list","items":[""" + Captured(WardInput, "Ward", "ward") + "]}}]}}]}"));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void an_input_in_a_json_body_is_a_member_of_its_value_read_in_the_live_form()
    {
        var plan = DeskPlan(pipeline => pipeline.Post<IntakeSend>("/intakes", request => request.Json(body => body.Input(r => r.Beds, pipeline.Input(m => m.Beds)))));
        Assert.That(RequestPage.Send(plan, "body").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"json"}},{"name":"value","value":{"kind":"as","expr":{"kind":"record","fields":[{"name":"beds","value":{"kind":"ref","name":"requestValue1"}}]},"shape":{"kind":"object","fields":{"beds":{"shape":{"kind":"number","of":"fraction","text":"invariant","overflow":"nothing"},"nothing":"omit"}}}}},{"name":"guarded","value":{"kind":"list","items":[]}},{"name":"enums","value":{"kind":"list","items":[]}},{"name":"inputs","value":{"kind":"record","fields":[{"name":"container","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"none"}}]}},{"name":"individual","value":{"kind":"list","items":[""" + Captured(BedsInput, "Beds", "beds") + "]}}]}}]}"));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void an_input_sends_its_value_in_place_of_a_member_set_under_its_name()
    {
        var plan = DeskPlan(pipeline => pipeline.Post<IntakeSend>("/intakes", request => request.Set(r => r.Ward, "North").Set(r => r.Beds, 2).Input(r => r.Ward, pipeline.Input(m => m.Ward))));
        Assert.That(RequestPage.Field(RequestPage.Send(plan, "body"), "parts").GetRawText(), Is.EqualTo(
            """{"kind":"list","items":[""" + Part(Field("Beds", Held("""{"kind":"lit","value":2}"""), """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"number"}}]}""", "leave")) + "," +
            Part(Field("Ward", Read("requestValue1"), WardText, "empty")) + "]}"));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void each_body_and_stage_sends_an_input_among_its_fields_and_records_its_capture()
    {
        var queried = DeskPlan(pipeline => pipeline.Get<IntakeSend>("/rooms", request => request.Inputs(pipeline.Container<IntakeSend>("contact")).Input(r => r.Ward, pipeline.Input(m => m.Ward))));
        Assert.That(RequestPage.Field(RequestPage.Field(RequestPage.Send(queried, "query"), "inputs"), "individual").GetRawText(), Is.EqualTo("""{"kind":"list","items":[""" + Captured(WardInput, "Ward", "ward") + "]}"));
        Assert.That(RequestPage.Field(RequestPage.Send(queried, "query"), "fields").GetRawText(), Is.EqualTo("""{"kind":"list","items":[""" + Field("Ward", Read("requestValue1"), WardText, "empty") + "]}"));
        var bodied = DeskPlan(pipeline => pipeline.Post<IntakeSend>("/intakes", request => request.Set(r => r.Room, "R4").Input(r => r.Ward, pipeline.Input(m => m.Ward))));
        Assert.That(RequestPage.Field(RequestPage.Field(RequestPage.Send(bodied, "body"), "inputs"), "individual").GetRawText(), Is.EqualTo("""{"kind":"list","items":[""" + Captured(WardInput, "Ward", "ward") + "]}"));
        var encoded = DeskPlan(pipeline => pipeline.Put<IntakeSend>("/intakes", request => request.UrlEncoded(body => body.Input(r => r.Ward, pipeline.Input(m => m.Ward)))));
        Assert.That(RequestPage.Field(RequestPage.Send(encoded, "body"), "fields").GetRawText(), Is.EqualTo("""{"kind":"list","items":[""" + Field("Ward", Read("requestValue1"), WardText, "empty") + "]}"));
        Assert.That(RequestPage.Field(RequestPage.Field(RequestPage.Send(encoded, "body"), "inputs"), "individual").GetRawText(), Is.EqualTo("""{"kind":"list","items":[""" + Captured(WardInput, "Ward", "ward") + "]}"));
        var multipart = DeskPlan(pipeline => pipeline.Delete<IntakeSend>("/intakes", request => request.Multipart(body => body.Input(r => r.Ward, pipeline.Input(m => m.Ward)))));
        Assert.That(RequestPage.Field(RequestPage.Send(multipart, "body"), "parts").GetRawText(), Is.EqualTo("""{"kind":"list","items":[""" + Part(Field("Ward", Read("requestValue1"), WardText, "empty")) + "]}"));
        Assert.That(RequestPage.Field(RequestPage.Field(RequestPage.Send(multipart, "body"), "inputs"), "individual").GetRawText(), Is.EqualTo("""{"kind":"list","items":[""" + Captured(WardInput, "Ward", "ward") + "]}"));
    }

    [Test]
    public void a_member_two_inputs_send_is_refused_where_the_view_renders()
        => Assert.That(() => DeskPlan(pipeline => pipeline.Post<IntakeSend>("/intakes", request => request.Input(r => r.Ward, pipeline.Input(m => m.Ward)).Input(r => r.Ward, pipeline.Input(m => m.Room)))),
            Throws.InvalidOperationException.With.Message.EqualTo("Request member 'Ward' overlaps a member already supplied; send each member once, and send either an object or its members."));

    private static string ContainerInputs(string method, string id)
        => $$$"""{"kind":"record","fields":[{"name":"container","value":{"kind":"call","path":["{{{method}}}"],"members":"required","target":{"kind":"obj","id":"$model"},"args":[{"kind":"lit","value":"{{{id}}}"},{"kind":"lit","value":"{{{Model}}}"}]}},{"name":"individual","value":{"kind":"list","items":[]}}]}""";

    private static string Captured(string id, string name, string path)
        => $$$"""{"kind":"call","path":["input"],"members":"required","target":{"kind":"obj","id":"$model"},"args":[{"kind":"lit","value":"{{{id}}}"},{"kind":"lit","value":"{{{name}}}"},{"kind":"list","items":[{"kind":"lit","value":"{{{path}}}"}]}]}""";

    private static string Read(string captured)
        => $$$"""{"kind":"op","op":"cond","args":[{"kind":"op","op":"or","args":[{"kind":"op","op":"eq","args":[{"kind":"ref","name":"{{{captured}}}"},{"kind":"lit","value":null}]},{"kind":"op","op":"neq","args":[{"kind":"ref","name":"{{{captured}}}"},{"kind":"lit","value":null}]}]},{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"ref","name":"{{{captured}}}"}}]},{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"nothing"}}]}]}""";

    private static string Field(string name, string value, string form, string whenNothing)
        => $$$"""{"kind":"record","fields":[{"name":"name","value":{"kind":"lit","value":"{{{name}}}"}},{"name":"path","value":{"kind":"list","items":[{"kind":"lit","value":"{{{name}}}"}]}},{"name":"value","value":{{{value}}}},{"name":"form","value":{{{form}}}},{"name":"whenNothing","value":{"kind":"lit","value":"{{{whenNothing}}}"}},{"name":"when","value":{"kind":"lit","value":true}}]}""";

    private static string Held(string value)
        => """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":""" + value + "}]}";

    private static string Part(string field)
        => """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"field"}},{"name":"field","value":""" + field + "}]}";

    private static string WardNorth => Field("Ward", Held("""{"kind":"lit","value":"North"}"""), WardText, "empty");

    private static string Inputs(JsonElement record) => RequestPage.Field(record, "inputs").GetRawText();

    private static string DeskPlan(Action<Pipeline<IntakeDesk>> write)
    {
        var html = Views.Of(new IntakeDesk(), _ => { }, app => app.AddReactiveRequests(requests => requests.NoAntiforgery()));
        var plan = html.ReactivePlan<IntakeDesk>();
        html.On(plan, trigger => trigger.DomReady(write));
        return Views.TextOf(html.RenderPlan(plan))["<script type=\"application/json\" data-reactive-plan>".Length..^"</script>".Length];
    }

    private static string ContainerPlan(Action<Pipeline<IntakeSend>, RequestContainer<IntakeSend>> write)
    {
        var html = Views.Of(new IntakeSend(), _ => { }, app => app.AddReactiveRequests(requests => requests.NoAntiforgery()));
        var plan = html.ReactivePlan<IntakeSend>();
        using (var block = html.BeginRequestContainer(plan, "intake"))
        {
            html.On(plan, trigger => trigger.DomReady(pipeline => write(pipeline, block.Container)));
        }

        return Views.TextOf(html.RenderPlan(plan))["<script type=\"application/json\" data-reactive-plan>".Length..^"</script>".Length];
    }
}

public sealed class IntakeDesk
{
    public string Ward { get; init; } = "";

    public string Room { get; init; } = "";

    public int Beds { get; init; }
}

public sealed class IntakeSend
{
    public string Ward { get; init; } = "";

    public string Room { get; init; } = "";

    public int Beds { get; init; }
}
