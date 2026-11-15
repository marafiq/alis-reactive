using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Html;
using NUnit.Framework;
using static Alis.Reactive.ModuleTests.Elements.SeatPlans;

namespace Alis.Reactive.ModuleTests.Elements;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAnElementsTextOrHtmlIsSet
{
    [Test]
    public void a_payloads_members_of_any_type_are_shown_as_the_string_table_writes_them()
    {
        var html = Views.Of(new Seating("4"));
        var plan = html.ReactivePlan<Seating>();
        plan.Render<SeatBoard>("board", HtmlString.Empty)
            .Reactive(board => board.Seated, (seated, pipeline) =>
            {
                pipeline.Element("seated-type").Text.Set(seated.Type);
                pipeline.Element("seated-room").Text.Set(seated.Detail.Read(detail => detail!.Room));
                pipeline.Element("seated-portion").Text.Set(seated.Portion);
            });
        var expected = Seated(
            Element("board") + "," + Element("seated-portion") + "," + Element("seated-room") + "," + Element("seated-type"),
            """{"kind":"set","path":["textContent"],"members":"required","content":true,"target":""" + Obj("seated-type") + ",\"value\":" +
            ShownAsText("""{"kind":"get","path":["type"],"members":"required","target":""" + Payload + "}") + "}," +
            """{"kind":"set","path":["textContent"],"members":"required","content":true,"target":""" + Obj("seated-room") + ",\"value\":" +
            ShownAsText("""{"kind":"data","path":[{"name":"room","requires":[],"absent":{"kind":"start","value":""}}],"target":{"kind":"get","path":["detail"],"members":"required","target":""" + Payload + "}}") + "}," +
            """{"kind":"set","path":["textContent"],"members":"required","content":true,"target":""" + Obj("seated-portion") + ",\"value\":" +
            ShownAsText("""{"kind":"get","path":["portion"],"members":"required","target":""" + Payload + "}") + "}");

        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void html_is_a_content_write_of_inner_html_from_a_text_or_a_source()
    {
        var html = Views.Of(new Seating("4"));
        var plan = html.ReactivePlan<Seating>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            pipeline.Element("menu").Html.Set("Soup today");
            pipeline.Element("menu").Html.Set(pipeline.Element("menu-draft").Text);
        }));
        var expected = Loaded(
            Element("menu") + "," + Element("menu-draft"),
            """{"kind":"set","path":["innerHTML"],"members":"required","content":true,"target":""" + Obj("menu") + ",\"value\":" + Text("Soup today") + "}," +
            """{"kind":"set","path":["innerHTML"],"members":"required","content":true,"target":""" + Obj("menu") + ",\"value\":" +
            ShownAsText("""{"kind":"get","path":["textContent"],"members":"required","target":""" + Obj("menu-draft") + "}") + "}");

        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_methods_result_set_as_text_is_called_at_its_line_and_shown_as_text()
    {
        var html = Views.Of(new Seating("4"));
        var plan = html.ReactivePlan<Seating>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Element("note").Text.Set(pipeline.Component<SeatBoard>("board").Note())));
        var expected = Loaded(
            Element("board") + "," + Element("note"),
            """{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["note"],"members":"required","target":""" + Obj("board") + ",\"args\":[]}}," +
            """{"kind":"set","path":["textContent"],"members":"required","content":true,"target":""" + Obj("note") + ",\"value\":" + ShownAsText(Ref("result1")) + "}");

        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }
}
