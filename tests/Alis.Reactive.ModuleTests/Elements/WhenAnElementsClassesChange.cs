using System;
using Alis.Reactive.ArrayLambdas;
using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Html;
using NUnit.Framework;
using static Alis.Reactive.ModuleTests.Elements.SeatPlans;

namespace Alis.Reactive.ModuleTests.Elements;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAnElementsClassesChange
{
    private const string Null = """{"kind":"lit","value":null}""";

    [Test]
    public void a_class_the_view_names_is_added_removed_or_toggled_where_its_name_holds_a_value()
    {
        var html = Views.Of(new Seating("4"));
        var plan = html.ReactivePlan<Seating>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            pipeline.Element("table-4").Classes.Add("risk-fall");
            pipeline.Element("table-4").Classes.Remove("risk-low");
            pipeline.Element("table-4").Classes.Toggle("seated");
        }));
        var expected = Loaded(
            Element("table-4"),
            Changed("1", Text("risk-fall"), "add") + "," + Changed("2", Text("risk-low"), "remove") + "," + Changed("3", Text("seated"), "toggle"));

        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_null_name_is_bound_as_null_so_it_changes_no_class()
    {
        var html = Views.Of(new Seating("4"));
        var plan = html.ReactivePlan<Seating>();
        string? noRisk = null;
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Element("table-4").Classes.Add(noRisk)));
        var expected = Loaded(Element("table-4"), Changed("1", Null, "add"));

        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_class_a_source_names_is_read_once_and_changes_no_class_when_it_reads_nothing()
    {
        var html = Views.Of(new Seating("4"));
        var plan = html.ReactivePlan<Seating>();
        plan.Render<SeatBoard>("board", HtmlString.Empty)
            .Reactive(board => board.Seated, (seated, pipeline) =>
            {
                pipeline.Element("table-4").Classes.Add(seated.Flag);
                pipeline.Element("table-4").Classes.Toggle(seated.Type);
            });
        var expected = Seated(
            Element("board") + "," + Element("table-4"),
            Changed("1", """{"kind":"get","path":["flag"],"members":"optionalMember","target":""" + Payload + "}", "add") + "," +
            Changed("2", """{"kind":"get","path":["type"],"members":"required","target":""" + Payload + "}", "toggle"));

        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void the_classes_are_a_list_of_text_their_iterator_yields_when_the_count_runs()
    {
        var html = Views.Of(new Seating("4"));
        var plan = html.ReactivePlan<Seating>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
            pipeline.Element("table-4-risks").Text.Set(pipeline.Element("table-4").Classes.Items.Count(flag => flag.StartsWith("risk-", StringComparison.Ordinal)))));
        var classes = """{"kind":"op","op":"toArray","args":[{"kind":"get","path":["classList"],"members":"required","target":""" + Obj("table-4") + "}]}";
        var risks = """{"kind":"get","path":["length"],"members":"requiredIfHeld","target":{"kind":"call","path":["filter"],"members":"requiredIfHeld","target":""" + classes +
            ""","args":[{"kind":"fn","params":["item1"],"body":{"kind":"op","op":"startsWith","args":[""" + Ref("item1") + "," + Text("risk-") + "]}}]}}";
        var expected = Loaded(
            Element("table-4") + "," + Element("table-4-risks"),
            """{"kind":"set","path":["textContent"],"members":"required","content":true,"target":""" + Obj("table-4-risks") + ",\"value\":" + ShownAsText(risks) + "}");

        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    private static string Changed(string number, string name, string method)
        => "{\"kind\":\"let\",\"name\":\"className" + number + "\",\"await\":false,\"binds\":\"line\",\"expr\":" + name + "}," +
           "{\"kind\":\"if\",\"when\":{\"kind\":\"op\",\"op\":\"neq\",\"args\":[" + Ref("className" + number) + "," + Null + "]}," +
           "\"then\":[{\"kind\":\"do\",\"await\":false,\"expr\":{\"kind\":\"call\",\"path\":[\"" + method + "\"],\"members\":\"required\"," +
           "\"target\":{\"kind\":\"get\",\"path\":[\"classList\"],\"members\":\"required\",\"target\":" + Obj("table-4") + "}," +
           "\"args\":[" + Ref("className" + number) + "]}}],\"else\":[]}";
}
