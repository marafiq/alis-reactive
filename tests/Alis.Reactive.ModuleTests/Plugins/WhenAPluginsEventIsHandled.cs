using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Plugins;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAPluginsEventIsHandled
{
    [Test]
    public void a_handler_with_no_payload_and_one_with_a_payload_run_under_overlap_by_default()
    {
        var html = Views.Of(new NightRound("North"));
        var plan = html.ReactivePlan<NightRound>();
        plan.Plugin<VisitorBook>()
            .Reactive(book => book.Signed, pipeline => pipeline.Plugin<VisitorBook>().Note.Set("signed"))
            .Reactive(book => book.Visited, (visit, pipeline) => pipeline.Plugin<VisitorBook>().Note.Set(visit.Room));
        var json = RoundPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Plugins.NightRound","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{},"objects":{"$plugin:visitorbook":{"kind":"plugin","name":"visitorbook","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}}},"handlers":[{"on":"$plugin:visitorbook","path":[],"event":"signed","payload":[],"members":"required","policy":"overlap","do":[{"kind":"set","path":["note"],"members":"required","content":false,"target":{"kind":"obj","id":"$plugin:visitorbook"},"value":{"kind":"lit","value":"signed"}}]},{"on":"$plugin:visitorbook","path":[],"event":"visited","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"set","path":["note"],"members":"required","content":false,"target":{"kind":"obj","id":"$plugin:visitorbook"},"value":{"kind":"get","path":["room"],"members":"required","target":{"kind":"ref","name":"e"}}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_handler_with_no_payload_and_one_with_a_payload_keep_each_explicit_policy()
    {
        var html = Views.Of(new NightRound("North"));
        var plan = html.ReactivePlan<NightRound>();
        plan.Plugin<VisitorBook>()
            .Reactive(book => book.Signed, HandlerPolicy.Replace, pipeline => pipeline.Plugin<VisitorBook>().Note.Set("signed"))
            .Reactive(book => book.Visited, HandlerPolicy.Skip, (visit, pipeline) => pipeline.Plugin<VisitorBook>().Note.Set(visit.Room));
        var json = RoundPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Plugins.NightRound","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{},"objects":{"$plugin:visitorbook":{"kind":"plugin","name":"visitorbook","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}}},"handlers":[{"on":"$plugin:visitorbook","path":[],"event":"signed","payload":[],"members":"required","policy":"replace","do":[{"kind":"set","path":["note"],"members":"required","content":false,"target":{"kind":"obj","id":"$plugin:visitorbook"},"value":{"kind":"lit","value":"signed"}}]},{"on":"$plugin:visitorbook","path":[],"event":"visited","payload":[0],"members":"required","policy":"skip","do":[{"kind":"set","path":["note"],"members":"required","content":false,"target":{"kind":"obj","id":"$plugin:visitorbook"},"value":{"kind":"get","path":["room"],"members":"required","target":{"kind":"ref","name":"e"}}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_handled_plugin_is_heard_by_the_listening_its_declaration_names()
    {
        var html = Views.Of(new NightRound("North"));
        var plan = html.ReactivePlan<NightRound>();
        plan.Plugin<BedStore>().Reactive(store => store.Changed, (change, pipeline) => pipeline.Plugin<BedStore>().Free.Set(change.Bed));
        var json = RoundPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Plugins.NightRound","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{},"objects":{"$plugin:bedstore":{"kind":"plugin","name":"bedstore","key":"","listen":{"kind":"returned","subscribe":"subscribe"},"data":{"kind":"none"}}},"handlers":[{"on":"$plugin:bedstore","path":[],"event":"changed","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"set","path":["free"],"members":"required","content":false,"target":{"kind":"obj","id":"$plugin:bedstore"},"value":{"kind":"get","path":["bed"],"members":"required","target":{"kind":"ref","name":"e"}}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }
}
