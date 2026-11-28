using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Vendors;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAPlanReachesAnAppLevelObject
{
    [Test]
    public void a_handler_of_an_event_with_no_payload_listens_on_the_element_its_declaration_fixes_under_overlap()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Component<RoundDrawer>().Reactive(drawer => drawer.Closed, pipeline => pipeline.Component<RoundDrawer>().Heading.Set("closed"));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"round-drawer":{"kind":"element","id":"round-drawer","vendor":"native"}},"handlers":[{"on":"round-drawer","path":[],"event":"closed","payload":[],"members":"required","policy":"overlap","do":[{"kind":"set","path":["heading"],"members":"required","content":false,"target":{"kind":"obj","id":"round-drawer"},"value":{"kind":"lit","value":"closed"}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_handler_of_an_event_with_a_payload_writes_and_reads_what_its_listener_receives_under_overlap()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Component<RoundDrawer>().Reactive(drawer => drawer.BeforeClose, (closing, pipeline) =>
        {
            closing.Cancel.Set(true);
            pipeline.Component<RoundDrawer>().Heading.Set(closing.ClosedBy);
        });
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"round-drawer":{"kind":"element","id":"round-drawer","vendor":"native"}},"handlers":[{"on":"round-drawer","path":[],"event":"beforeClose","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"set","path":["cancel"],"members":"required","content":false,"target":{"kind":"ref","name":"e"},"value":{"kind":"lit","value":true}},{"kind":"set","path":["heading"],"members":"required","content":false,"target":{"kind":"obj","id":"round-drawer"},"value":{"kind":"get","path":["closedBy"],"members":"required","target":{"kind":"ref","name":"e"}}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_payloadless_handler_keeps_each_explicit_policy()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Component<RoundDrawer>()
            .Reactive(drawer => drawer.Closed, HandlerPolicy.Overlap, pipeline => pipeline.Component<RoundDrawer>().Heading.Set("overlapping"))
            .Reactive(drawer => drawer.Closed, HandlerPolicy.Replace, pipeline => pipeline.Component<RoundDrawer>().Heading.Set("replacing"))
            .Reactive(drawer => drawer.Closed, HandlerPolicy.Skip, pipeline => pipeline.Component<RoundDrawer>().Heading.Set("skipping"));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"round-drawer":{"kind":"element","id":"round-drawer","vendor":"native"}},"handlers":[{"on":"round-drawer","path":[],"event":"closed","payload":[],"members":"required","policy":"overlap","do":[{"kind":"set","path":["heading"],"members":"required","content":false,"target":{"kind":"obj","id":"round-drawer"},"value":{"kind":"lit","value":"overlapping"}}]},{"on":"round-drawer","path":[],"event":"closed","payload":[],"members":"required","policy":"replace","do":[{"kind":"set","path":["heading"],"members":"required","content":false,"target":{"kind":"obj","id":"round-drawer"},"value":{"kind":"lit","value":"replacing"}}]},{"on":"round-drawer","path":[],"event":"closed","payload":[],"members":"required","policy":"skip","do":[{"kind":"set","path":["heading"],"members":"required","content":false,"target":{"kind":"obj","id":"round-drawer"},"value":{"kind":"lit","value":"skipping"}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_payload_handler_keeps_each_explicit_policy_and_its_bindings()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Component<RoundDrawer>()
            .Reactive(drawer => drawer.BeforeClose, HandlerPolicy.Overlap, (closing, pipeline) => closing.Cancel.Set(true))
            .Reactive(drawer => drawer.BeforeClose, HandlerPolicy.Replace, (closing, pipeline) => pipeline.Component<RoundDrawer>().Heading.Set(closing.ClosedBy))
            .Reactive(drawer => drawer.BeforeClose, HandlerPolicy.Skip, (closing, pipeline) => closing.Cancel.Set(false));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"round-drawer":{"kind":"element","id":"round-drawer","vendor":"native"}},"handlers":[{"on":"round-drawer","path":[],"event":"beforeClose","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"set","path":["cancel"],"members":"required","content":false,"target":{"kind":"ref","name":"e"},"value":{"kind":"lit","value":true}}]},{"on":"round-drawer","path":[],"event":"beforeClose","payload":[0],"members":"required","policy":"replace","do":[{"kind":"set","path":["heading"],"members":"required","content":false,"target":{"kind":"obj","id":"round-drawer"},"value":{"kind":"get","path":["closedBy"],"members":"required","target":{"kind":"ref","name":"e"}}}]},{"on":"round-drawer","path":[],"event":"beforeClose","payload":[0],"members":"required","policy":"skip","do":[{"kind":"set","path":["cancel"],"members":"required","content":false,"target":{"kind":"ref","name":"e"},"value":{"kind":"lit","value":false}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void two_reaches_in_one_plan_name_one_object_and_keep_each_handler()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Component<RoundDrawer>().Reactive(drawer => drawer.Closed, pipeline => pipeline.Component<RoundDrawer>().Heading.Set("closed"));
        plan.Component<RoundDrawer>().Reactive(drawer => drawer.BeforeClose, (closing, pipeline) => closing.Cancel.Set(true));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"round-drawer":{"kind":"element","id":"round-drawer","vendor":"native"}},"handlers":[{"on":"round-drawer","path":[],"event":"closed","payload":[],"members":"required","policy":"overlap","do":[{"kind":"set","path":["heading"],"members":"required","content":false,"target":{"kind":"obj","id":"round-drawer"},"value":{"kind":"lit","value":"closed"}}]},{"on":"round-drawer","path":[],"event":"beforeClose","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"set","path":["cancel"],"members":"required","content":false,"target":{"kind":"ref","name":"e"},"value":{"kind":"lit","value":true}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_step_reads_sets_and_calls_the_objects_members_on_the_element_its_declaration_fixes()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var drawer = pipeline.Component<RoundDrawer>();
            drawer.Heading.Set(drawer.Section);
            drawer.Open();
        }));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"$document":{"kind":"plugin","name":"alis.document","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}},"round-drawer":{"kind":"element","id":"round-drawer","vendor":"native"}},"handlers":[{"on":"$document","path":[],"event":"ready","payload":[],"members":"required","policy":"overlap","do":[{"kind":"set","path":["heading"],"members":"required","content":false,"target":{"kind":"obj","id":"round-drawer"},"value":{"kind":"get","path":["section"],"members":"required","target":{"kind":"obj","id":"round-drawer"}}},{"kind":"do","await":false,"expr":{"kind":"call","path":["open"],"members":"required","target":{"kind":"obj","id":"round-drawer"},"args":[]}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }
}
