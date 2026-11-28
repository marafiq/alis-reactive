using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Html;
using NUnit.Framework;
using static Alis.Reactive.ModuleTests.Elements.SeatPlans;

namespace Alis.Reactive.ModuleTests.Elements;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAnElementsContentIsAppended
{
    [Test]
    public void a_text_is_added_after_the_content_by_insert_adjacent_text_where_its_joined_text_is_not_empty()
    {
        var html = Views.Of(new Seating("4"));
        var plan = html.ReactivePlan<Seating>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Element("hour-log").Text.Append("Noted at 02:00")));
        var expected = Loaded(Element("hour-log"), Added("1", Obj("hour-log"), Text("Noted at 02:00"), "insertAdjacentText"));

        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_source_is_added_to_the_html_by_insert_adjacent_html_as_its_text_and_one_that_reads_nothing_as_an_empty_text()
    {
        var html = Views.Of(new Seating("4"));
        var plan = html.ReactivePlan<Seating>();
        plan.Render<SeatBoard>("board", HtmlString.Empty)
            .Reactive(board => board.Seated, (seated, pipeline) => pipeline.Element("arrivals").Html.Append(seated.Flag));
        var flag = """{"kind":"get","path":["flag"],"members":"optionalMember","target":""" + Payload + "}";
        var expected = Seated(
            Element("arrivals") + "," + Element("board"),
            Added("1", Obj("arrivals"), """{"kind":"op","op":"coalesce","args":[""" + ShownAsText(flag) + "," + Text("") + "]}", "insertAdjacentHTML"));

        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_null_text_is_an_empty_part_so_its_append_adds_nothing_and_still_needs_its_element()
    {
        var html = Views.Of(new Seating("4"));
        var plan = html.ReactivePlan<Seating>();
        string? noEntry = null;
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Element("hour-log").Text.Append(noEntry)));
        var expected = Loaded(Element("hour-log"), Added("1", Obj("hour-log"), Text(""), "insertAdjacentText"));

        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_methods_result_appended_is_called_at_its_line_before_the_element_is_bound()
    {
        var html = Views.Of(new Seating("4"));
        var plan = html.ReactivePlan<Seating>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Element("hour-log").Text.Append(pipeline.Component<SeatBoard>("board").Note())));
        var expected = Loaded(
            Element("board") + "," + Element("hour-log"),
            """{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["note"],"members":"required","target":""" + Obj("board") + ",\"args\":[]}}," +
            Added("1", Obj("hour-log"), """{"kind":"op","op":"coalesce","args":[""" + ShownAsText(Ref("result1")) + "," + Text("") + "]}", "insertAdjacentText"));

        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void two_appends_in_one_pipeline_bind_names_of_their_own()
    {
        var html = Views.Of(new Seating("4"));
        var plan = html.ReactivePlan<Seating>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            pipeline.Element("hour-log").Text.Append("Turned at midnight");
            pipeline.Element("hour-log").Text.Append("Noted at 02:00");
        }));
        var expected = Loaded(
            Element("hour-log"),
            Added("1", Obj("hour-log"), Text("Turned at midnight"), "insertAdjacentText") + "," +
            Added("2", Obj("hour-log"), Text("Noted at 02:00"), "insertAdjacentText"));

        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    private static string Added(string number, string element, string part, string method)
        => "{\"kind\":\"let\",\"name\":\"element" + number + "\",\"await\":false,\"binds\":\"line\",\"expr\":" + element + "}," +
           "{\"kind\":\"let\",\"name\":\"text" + number + "\",\"await\":false,\"binds\":\"line\",\"expr\":" +
           "{\"kind\":\"call\",\"path\":[\"join\"],\"members\":\"requiredIfHeld\",\"target\":{\"kind\":\"list\",\"items\":[" + part + "]},\"args\":[" + Text("") + "]}}," +
           "{\"kind\":\"if\",\"when\":{\"kind\":\"op\",\"op\":\"neq\",\"args\":[" + Ref("text" + number) + "," + Text("") + "]}," +
           "\"then\":[{\"kind\":\"do\",\"await\":false,\"expr\":{\"kind\":\"call\",\"path\":[\"" + method + "\"],\"members\":\"required\",\"target\":" + Ref("element" + number) +
           ",\"args\":[" + Text("beforeend") + "," + Ref("text" + number) + "]}}],\"else\":[]}";
}
