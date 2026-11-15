using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;
using static Alis.Reactive.ModuleTests.Elements.SeatPlans;

namespace Alis.Reactive.ModuleTests.Elements;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAnElementShowsOrHides
{
    [Test]
    public void hide_and_show_set_hidden_to_true_and_false_with_no_content_write()
    {
        var html = Views.Of(new Seating("4"));
        var plan = html.ReactivePlan<Seating>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            pipeline.Element("memory-care").Hide();
            pipeline.Element("memory-care").Show();
        }));
        var expected = Loaded(
            Element("memory-care"),
            """{"kind":"set","path":["hidden"],"members":"required","content":false,"target":""" + Obj("memory-care") + ""","value":{"kind":"lit","value":true}},""" +
            """{"kind":"set","path":["hidden"],"members":"required","content":false,"target":""" + Obj("memory-care") + ""","value":{"kind":"lit","value":false}}""");

        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void hidden_is_a_source_of_whether_the_element_is_hidden()
    {
        var html = Views.Of(new Seating("4"));
        var plan = html.ReactivePlan<Seating>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Element("memory-care-hidden").Text.Set(pipeline.Element("memory-care").Hidden)));
        var expected = Loaded(
            Element("memory-care") + "," + Element("memory-care-hidden"),
            """{"kind":"set","path":["textContent"],"members":"required","content":true,"target":""" + Obj("memory-care-hidden") + ",\"value\":" +
            ShownAsText("""{"kind":"get","path":["hidden"],"members":"required","target":""" + Obj("memory-care") + "}") + "}");

        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }
}
