using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Html;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Vendors;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenARenderedHandlerNamesItsPolicy
{
    [Test]
    public void a_payloadless_handler_keeps_each_explicit_policy()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<NativeBell>("bell", HtmlString.Empty)
            .Reactive(bell => bell.Silenced, HandlerPolicy.Overlap, pipeline => pipeline.Component<NativeBell>("bell").Text.Set("overlapping"))
            .Reactive(bell => bell.Cleared, HandlerPolicy.Replace, pipeline => pipeline.Component<NativeBell>("bell").Text.Set("replacing"))
            .Reactive(bell => bell.Silenced, HandlerPolicy.Skip, pipeline => pipeline.Component<NativeBell>("bell").Text.Set("skipping"));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"bell":{"kind":"element","id":"bell","vendor":"native"}},"handlers":[{"on":"bell","path":[],"event":"silenced","payload":[],"members":"required","policy":"overlap","do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"obj","id":"bell"},"value":{"kind":"lit","value":"overlapping"}}]},{"on":"bell","path":[],"event":"clear-all","payload":[],"members":"required","policy":"replace","do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"obj","id":"bell"},"value":{"kind":"lit","value":"replacing"}}]},{"on":"bell","path":[],"event":"silenced","payload":[],"members":"required","policy":"skip","do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"obj","id":"bell"},"value":{"kind":"lit","value":"skipping"}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_payload_handler_keeps_each_explicit_policy_and_its_bindings()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<NativeBell>("bell", HtmlString.Empty)
            .Reactive(bell => bell.Rang, HandlerPolicy.Overlap, (ring, pipeline) => ring.Cancel.Set(true))
            .Reactive(bell => bell.Urgent, HandlerPolicy.Replace, (ring, pipeline) => pipeline.Component<NativeBell>("bell").Text.Set(ring.Room))
            .Reactive(bell => bell.Searching, HandlerPolicy.Skip, (search, pipeline) => search.Show(search.Context, 3));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"bell":{"kind":"element","id":"bell","vendor":"native"}},"handlers":[{"on":"bell","path":[],"event":"rang","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"set","path":["cancel"],"members":"required","content":false,"target":{"kind":"ref","name":"e"},"value":{"kind":"lit","value":true}}]},{"on":"bell","path":[],"event":"ring-now","payload":[0],"members":"required","policy":"replace","do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"obj","id":"bell"},"value":{"kind":"get","path":["room"],"members":"required","target":{"kind":"ref","name":"e"}}}]},{"on":"bell","path":[],"event":"searching","payload":[0],"members":"required","policy":"skip","do":[{"kind":"do","await":false,"expr":{"kind":"call","path":[1],"members":"required","target":{"kind":"ref","name":"arguments"},"args":[{"kind":"get","path":[2],"members":"required","target":{"kind":"ref","name":"arguments"}},{"kind":"lit","value":3}]}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }
}
