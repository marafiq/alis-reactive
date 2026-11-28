using Alis.Reactive.Conditions;
using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;
using static Alis.Reactive.ModuleTests.Elements.SeatPlans;

namespace Alis.Reactive.ModuleTests.Elements;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAnElementsChildrenAreCounted
{
    private const string Seats = """{"kind":"op","op":"toArray","args":[{"kind":"get","path":["children"],"members":"required","target":{"kind":"obj","id":"table-seats"}}]}""";

    [Test]
    public void a_count_of_children_is_the_length_of_the_array_their_iterator_yields()
    {
        var html = Views.Of(new Seating("4"));
        var plan = html.ReactivePlan<Seating>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Element("seated").Text.Set(pipeline.Element("table-seats").Children.Items.Count())));
        var expected = Loaded(
            Element("seated") + "," + Element("table-seats"),
            SetTo("seated", """{"kind":"get","path":["length"],"members":"requiredIfHeld","target":""" + Seats + "}"));

        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_lambda_over_the_children_reads_each_seats_attributes_by_its_own_methods()
    {
        var html = Views.Of(new Seating("4"));
        var plan = html.ReactivePlan<Seating>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            pipeline.Element("close-supervision").Text.Set(pipeline.Element("table-seats").Children.Items.Count(seat => seat.GetAttribute("data-supervision").Eq("close")));
            pipeline.Element("not-close").Text.Set(pipeline.Element("table-seats").Children.Items.Count(seat => seat.GetAttribute("data-supervision").NotEq("close")));
            pipeline.Element("wheelchair-seats").Text.Set(pipeline.Element("table-seats").Children.Items.Count(seat => seat.HasAttribute("data-wheelchair")));
            pipeline.Element("hoist-seats").Text.Set(pipeline.Element("table-seats").Children.Items.Count(seat => seat.HasAttribute("data-hoist")));
        }));
        var expected = Loaded(
            Element("close-supervision") + "," + Element("hoist-seats") + "," + Element("not-close") + "," + Element("table-seats") + "," + Element("wheelchair-seats"),
            SetTo("close-supervision", Counted("liveItem1", """{"kind":"op","op":"eq","args":[""" + Attribute("liveItem1", "getAttribute", "data-supervision") + "," + Text("close") + "]}")) + "," +
            SetTo("not-close", Counted("liveItem2", """{"kind":"op","op":"neq","args":[""" + Attribute("liveItem2", "getAttribute", "data-supervision") + "," + Text("close") + "]}")) + "," +
            SetTo("wheelchair-seats", Counted("liveItem3", Attribute("liveItem3", "hasAttribute", "data-wheelchair"))) + "," +
            SetTo("hoist-seats", Counted("liveItem4", Attribute("liveItem4", "hasAttribute", "data-hoist"))));

        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    private static string SetTo(string id, string value)
        => "{\"kind\":\"set\",\"path\":[\"textContent\"],\"members\":\"required\",\"content\":true,\"target\":" + Obj(id) + ",\"value\":" + ShownAsText(value) + "}";

    private static string Counted(string item, string body)
        => "{\"kind\":\"get\",\"path\":[\"length\"],\"members\":\"requiredIfHeld\",\"target\":{\"kind\":\"call\",\"path\":[\"filter\"],\"members\":\"requiredIfHeld\",\"target\":" + Seats +
           ",\"args\":[{\"kind\":\"fn\",\"params\":[\"" + item + "\"],\"body\":" + body + "}]}}";

    private static string Attribute(string item, string method, string name)
        => "{\"kind\":\"call\",\"path\":[\"" + method + "\"],\"members\":\"required\",\"target\":" + Ref(item) + ",\"args\":[" + Text(name) + "]}";
}
