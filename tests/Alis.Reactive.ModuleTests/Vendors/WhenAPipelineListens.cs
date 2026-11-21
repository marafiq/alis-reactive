using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Html;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Vendors;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAPipelineListens
{
    [Test]
    public void an_objects_own_listening_and_default_overlap_leave_while_it_lives()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<ListenHost>("host", HtmlString.Empty).Reactive(host => host.Changed, (outer, pipeline) =>
        {
            pipeline.Listen(outer.Feed, feed => feed.Updated, (inner, pipeline) => inner.Text.Set("heard"));
        });
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"host":{"kind":"element","id":"host","vendor":"native"}},"handlers":[{"on":"host","path":[],"event":"changed","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"listen","event":"feed-updated","listen":{"kind":"method","subscribe":"on","unsubscribe":"off"},"release":{"kind":"living"},"payload":[0],"members":"required","policy":"overlap","payloadName":"listenPayload1","argumentsName":"listenArguments1","object":{"kind":"get","path":["feed"],"members":"required","target":{"kind":"ref","name":"e"}},"do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"ref","name":"listenPayload1"},"value":{"kind":"lit","value":"heard"}}]}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void the_explicit_overlap_policy_belongs_to_the_listened_event()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<ListenHost>("host", HtmlString.Empty).Reactive(host => host.Changed, (outer, pipeline) =>
        {
            pipeline.Listen(outer.Feed, feed => feed.Updated, HandlerPolicy.Overlap, (inner, pipeline) => inner.Text.Set("heard"));
        });
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"host":{"kind":"element","id":"host","vendor":"native"}},"handlers":[{"on":"host","path":[],"event":"changed","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"listen","event":"feed-updated","listen":{"kind":"method","subscribe":"on","unsubscribe":"off"},"release":{"kind":"living"},"payload":[0],"members":"required","policy":"overlap","payloadName":"listenPayload1","argumentsName":"listenArguments1","object":{"kind":"get","path":["feed"],"members":"required","target":{"kind":"ref","name":"e"}},"do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"ref","name":"listenPayload1"},"value":{"kind":"lit","value":"heard"}}]}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void the_explicit_replace_policy_belongs_to_the_listened_event()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<ListenHost>("host", HtmlString.Empty).Reactive(host => host.Changed, (outer, pipeline) =>
        {
            pipeline.Listen(outer.Feed, feed => feed.Updated, HandlerPolicy.Replace, (inner, pipeline) => inner.Text.Set("heard"));
        });
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"host":{"kind":"element","id":"host","vendor":"native"}},"handlers":[{"on":"host","path":[],"event":"changed","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"listen","event":"feed-updated","listen":{"kind":"method","subscribe":"on","unsubscribe":"off"},"release":{"kind":"living"},"payload":[0],"members":"required","policy":"replace","payloadName":"listenPayload1","argumentsName":"listenArguments1","object":{"kind":"get","path":["feed"],"members":"required","target":{"kind":"ref","name":"e"}},"do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"ref","name":"listenPayload1"},"value":{"kind":"lit","value":"heard"}}]}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void the_explicit_skip_policy_belongs_to_the_listened_event()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<ListenHost>("host", HtmlString.Empty).Reactive(host => host.Changed, (outer, pipeline) =>
        {
            pipeline.Listen(outer.Feed, feed => feed.Updated, HandlerPolicy.Skip, (inner, pipeline) => inner.Text.Set("heard"));
        });
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"host":{"kind":"element","id":"host","vendor":"native"}},"handlers":[{"on":"host","path":[],"event":"changed","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"listen","event":"feed-updated","listen":{"kind":"method","subscribe":"on","unsubscribe":"off"},"release":{"kind":"living"},"payload":[0],"members":"required","policy":"skip","payloadName":"listenPayload1","argumentsName":"listenArguments1","object":{"kind":"get","path":["feed"],"members":"required","target":{"kind":"ref","name":"e"}},"do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"ref","name":"listenPayload1"},"value":{"kind":"lit","value":"heard"}}]}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_component_reached_by_id_needs_no_element_path()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<ListenHost>("host", HtmlString.Empty).Reactive(host => host.Changed, (outer, pipeline) =>
        {
            pipeline.Listen(pipeline.Component<NoElementPeer>("peer"), peer => peer.Updated, (inner, pipeline) => inner.Text.Set("heard"));
        });
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}},"unlocated":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"get","path":["instances",0],"members":"required","target":{"kind":"ref","name":"element"}}},"listen":{"kind":"method","subscribe":"hear","unsubscribe":"forget"}}},"objects":{"host":{"kind":"element","id":"host","vendor":"native"},"peer":{"kind":"element","id":"peer","vendor":"unlocated"}},"handlers":[{"on":"host","path":[],"event":"changed","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"listen","event":"updated","listen":{"kind":"method","subscribe":"hear","unsubscribe":"forget"},"release":{"kind":"located","id":"peer"},"payload":[0],"members":"required","policy":"overlap","payloadName":"listenPayload1","argumentsName":"listenArguments1","object":{"kind":"obj","id":"peer"},"do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"ref","name":"listenPayload1"},"value":{"kind":"lit","value":"heard"}}]}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_component_the_payload_holds_carries_its_vendor_and_element_path()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<ListenHost>("host", HtmlString.Empty).Reactive(host => host.Changed, (outer, pipeline) =>
        {
            pipeline.Listen(outer.Peer, peer => peer.Updated, (inner, pipeline) => inner.Text.Set("heard"));
        });
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}},"peer":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"get","path":["instances",0],"members":"required","target":{"kind":"ref","name":"element"}}},"listen":{"kind":"method","subscribe":"hear","unsubscribe":"forget"}}},"objects":{"host":{"kind":"element","id":"host","vendor":"native"}},"handlers":[{"on":"host","path":[],"event":"changed","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"listen","event":"updated","listen":{"kind":"method","subscribe":"hear","unsubscribe":"forget"},"release":{"kind":"held","vendor":"peer","element":["host",0]},"payload":[0],"members":"required","policy":"overlap","payloadName":"listenPayload1","argumentsName":"listenArguments1","object":{"kind":"get","path":["peer"],"members":"required","target":{"kind":"ref","name":"e"}},"do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"ref","name":"listenPayload1"},"value":{"kind":"lit","value":"heard"}}]}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_payload_that_is_a_component_carries_its_element_and_takes_a_later_write()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<ListenHost>("host", HtmlString.Empty).Reactive(host => host.PeerChanged, (outer, pipeline) =>
        {
            pipeline.Listen(outer, peer => peer.Updated, (inner, pipeline) => outer.Text.Set(inner.Text));
        });
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}},"peer":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"get","path":["instances",0],"members":"required","target":{"kind":"ref","name":"element"}}},"listen":{"kind":"method","subscribe":"hear","unsubscribe":"forget"}}},"objects":{"host":{"kind":"element","id":"host","vendor":"native"}},"handlers":[{"on":"host","path":[],"event":"peerChanged","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"listen","event":"updated","listen":{"kind":"method","subscribe":"hear","unsubscribe":"forget"},"release":{"kind":"held","vendor":"peer","element":["host",0]},"payload":[0],"members":"required","policy":"overlap","payloadName":"listenPayload1","argumentsName":"listenArguments1","object":{"kind":"ref","name":"e"},"do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"ref","name":"e"},"value":{"kind":"get","path":["text"],"members":"required","target":{"kind":"ref","name":"listenPayload1"}}}]}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_payload_that_is_an_evented_object_keeps_living_release()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<ListenHost>("host", HtmlString.Empty).Reactive(host => host.FeedChanged, (outer, pipeline) =>
        {
            pipeline.Listen(outer, feed => feed.Updated, (inner, pipeline) => inner.Text.Set("heard"));
        });
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"host":{"kind":"element","id":"host","vendor":"native"}},"handlers":[{"on":"host","path":[],"event":"feedChanged","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"listen","event":"feed-updated","listen":{"kind":"method","subscribe":"on","unsubscribe":"off"},"release":{"kind":"living"},"payload":[0],"members":"required","policy":"overlap","payloadName":"listenPayload1","argumentsName":"listenArguments1","object":{"kind":"ref","name":"e"},"do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"ref","name":"listenPayload1"},"value":{"kind":"lit","value":"heard"}}]}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void an_event_without_payload_keeps_the_empty_path()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<ListenHost>("host", HtmlString.Empty).Reactive(host => host.Changed, (outer, pipeline) =>
        {
            pipeline.Listen(outer.Feed, feed => feed.Quiet, (nothing, pipeline) => outer.Feed.Text.Set("quiet"));
        });
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"host":{"kind":"element","id":"host","vendor":"native"}},"handlers":[{"on":"host","path":[],"event":"changed","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"listen","event":"quiet","listen":{"kind":"method","subscribe":"on","unsubscribe":"off"},"release":{"kind":"living"},"payload":[],"members":"required","policy":"overlap","payloadName":"listenPayload1","argumentsName":"listenArguments1","object":{"kind":"get","path":["feed"],"members":"required","target":{"kind":"ref","name":"e"}},"do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"get","path":["feed"],"members":"required","target":{"kind":"ref","name":"e"}},"value":{"kind":"lit","value":"quiet"}}]}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_nested_listener_keeps_each_payload_and_arguments_binding()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<ListenHost>("host", HtmlString.Empty).Reactive(host => host.Changed, (outer, pipeline) =>
        {
            pipeline.Listen(outer.Feed, feed => feed.Updated, (inner, pipeline) =>
            {
                pipeline.Listen(inner.Feed, feed => feed.Updated, (last, pipeline) =>
                {
                    last.Text.Set(outer.Text);
                    last.Text.Set(inner.Text);
                    last.Text.Set(outer.Position);
                    last.Text.Set(inner.Position);
                    last.Text.Set(last.Position);
                });
                inner.Text.Set(outer.Text);
            });
            outer.Text.Set("returned");
        });
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"host":{"kind":"element","id":"host","vendor":"native"}},"handlers":[{"on":"host","path":[],"event":"changed","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"listen","event":"feed-updated","listen":{"kind":"method","subscribe":"on","unsubscribe":"off"},"release":{"kind":"living"},"payload":[0],"members":"required","policy":"overlap","payloadName":"listenPayload1","argumentsName":"listenArguments1","object":{"kind":"get","path":["feed"],"members":"required","target":{"kind":"ref","name":"e"}},"do":[{"kind":"listen","event":"feed-updated","listen":{"kind":"method","subscribe":"on","unsubscribe":"off"},"release":{"kind":"living"},"payload":[0],"members":"required","policy":"overlap","payloadName":"listenPayload2","argumentsName":"listenArguments2","object":{"kind":"get","path":["feed"],"members":"required","target":{"kind":"ref","name":"listenPayload1"}},"do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"ref","name":"listenPayload2"},"value":{"kind":"get","path":["text"],"members":"required","target":{"kind":"ref","name":"e"}}},{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"ref","name":"listenPayload2"},"value":{"kind":"get","path":["text"],"members":"required","target":{"kind":"ref","name":"listenPayload1"}}},{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"ref","name":"listenPayload2"},"value":{"kind":"get","path":[2],"members":"required","target":{"kind":"ref","name":"arguments"}}},{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"ref","name":"listenPayload2"},"value":{"kind":"get","path":[2],"members":"required","target":{"kind":"ref","name":"listenArguments1"}}},{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"ref","name":"listenPayload2"},"value":{"kind":"get","path":[2],"members":"required","target":{"kind":"ref","name":"listenArguments2"}}}]},{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"ref","name":"listenPayload1"},"value":{"kind":"get","path":["text"],"members":"required","target":{"kind":"ref","name":"e"}}}]},{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"ref","name":"e"},"value":{"kind":"lit","value":"returned"}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void the_listened_payload_starts_in_its_own_turn_after_the_outer_wait()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<ListenHost>("host", HtmlString.Empty).Reactive(host => host.Changed, (outer, pipeline) =>
        {
            outer.Wait();
            pipeline.Listen(outer.Feed, feed => feed.Updated, (inner, pipeline) => inner.Text.Set("heard"));
        });
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"host":{"kind":"element","id":"host","vendor":"native"}},"handlers":[{"on":"host","path":[],"event":"changed","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"do","await":true,"expr":{"kind":"call","path":["wait"],"members":"required","target":{"kind":"ref","name":"e"},"args":[]}},{"kind":"listen","event":"feed-updated","listen":{"kind":"method","subscribe":"on","unsubscribe":"off"},"release":{"kind":"living"},"payload":[0],"members":"required","policy":"overlap","payloadName":"listenPayload1","argumentsName":"listenArguments1","object":{"kind":"get","path":["feed"],"members":"required","target":{"kind":"ref","name":"e"}},"do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"ref","name":"listenPayload1"},"value":{"kind":"lit","value":"heard"}}]}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_listen_reads_an_enclosing_result_and_binds_its_own_result()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<ListenHost>("host", HtmlString.Empty).Reactive(host => host.Changed, (outer, pipeline) =>
        {
            var text = outer.ReadText();
            pipeline.Listen(outer.Feed, feed => feed.Updated, (inner, pipeline) =>
            {
                inner.Text.Set(text);
                inner.Text.Set(inner.ReadText());
            });
        });
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"host":{"kind":"element","id":"host","vendor":"native"}},"handlers":[{"on":"host","path":[],"event":"changed","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["readText"],"members":"required","target":{"kind":"ref","name":"e"},"args":[]}},{"kind":"listen","event":"feed-updated","listen":{"kind":"method","subscribe":"on","unsubscribe":"off"},"release":{"kind":"living"},"payload":[0],"members":"required","policy":"overlap","payloadName":"listenPayload1","argumentsName":"listenArguments1","object":{"kind":"get","path":["feed"],"members":"required","target":{"kind":"ref","name":"e"}},"do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"ref","name":"listenPayload1"},"value":{"kind":"ref","name":"result1"}},{"kind":"let","name":"result2","await":false,"binds":"line","expr":{"kind":"call","path":["readText"],"members":"required","target":{"kind":"ref","name":"listenPayload1"},"args":[]}},{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"ref","name":"listenPayload1"},"value":{"kind":"ref","name":"result2"}}]}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_listen_refuses_an_enclosing_payloads_property()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        Assert.That(() => plan.Render<ListenHost>("host", HtmlString.Empty).Reactive(host => host.Changed, (outer, pipeline) =>
        {
            pipeline.Listen(outer.Feed, feed => feed.Updated, (inner, pipeline) => outer.Text.Set("late"));
        }), Throws.InvalidOperationException.With.Message.EqualTo("A listen cannot write the payload of the event around it: the vendor read that payload when its listener returned. Write it before the listen step."));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_listen_refuses_an_enclosing_payloads_write_only_property()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        Assert.That(() => plan.Render<ListenHost>("host", HtmlString.Empty).Reactive(host => host.Changed, (outer, pipeline) =>
        {
            pipeline.Listen(outer.Feed, feed => feed.Updated, (inner, pipeline) => outer.Secret.Set("late"));
        }), Throws.InvalidOperationException.With.Message.EqualTo("A listen cannot write the payload of the event around it: the vendor read that payload when its listener returned. Write it before the listen step."));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_listen_refuses_an_enclosing_payloads_callback()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        Assert.That(() => plan.Render<ListenHost>("host", HtmlString.Empty).Reactive(host => host.Changed, (outer, pipeline) =>
        {
            pipeline.Listen(outer.Feed, feed => feed.Updated, (inner, pipeline) => outer.Format.Set(search => search.Query));
        }), Throws.InvalidOperationException.With.Message.EqualTo("A listen cannot write the payload of the event around it: the vendor read that payload when its listener returned. Write it before the listen step."));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_listen_refuses_an_object_declared_optional()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        Assert.That(() => plan.Render<ListenHost>("host", HtmlString.Empty).Reactive(host => host.Changed, (outer, pipeline) =>
        {
            pipeline.Listen(outer.OptionalFeed, feed => feed.Updated, (inner, pipeline) => inner.Text.Set("heard"));
        }), Throws.InvalidOperationException.With.Message.EqualTo("A listen needs its object when the step runs: an object declared optional or nullable cannot be listened to."));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_listen_refuses_an_object_declared_nullable()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        Assert.That(() => plan.Render<ListenHost>("host", HtmlString.Empty).Reactive(host => host.Changed, (outer, pipeline) =>
        {
            pipeline.Listen(outer.NullableFeed, feed => feed.Updated, (inner, pipeline) => inner.Text.Set("heard"));
        }), Throws.InvalidOperationException.With.Message.EqualTo("A listen needs its object when the step runs: an object declared optional or nullable cannot be listened to."));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_component_held_by_a_payload_needs_an_element_path()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        Assert.That(() => plan.Render<ListenHost>("host", HtmlString.Empty).Reactive(host => host.Changed, (outer, pipeline) =>
        {
            pipeline.Listen(outer.Unlocated, peer => peer.Updated, (inner, pipeline) => inner.Text.Set("heard"));
        }), Throws.InvalidOperationException.With.Message.EqualTo("A listen on a component handed over by a payload or a returned object needs its vendor to name an element path, so release can tell a destroyed component from a living one."));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_payload_that_is_a_component_needs_an_element_path()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        Assert.That(() => plan.Render<ListenHost>("host", HtmlString.Empty).Reactive(host => host.Unlocated, (outer, pipeline) =>
        {
            pipeline.Listen(outer, peer => peer.Updated, (inner, pipeline) => inner.Text.Set("heard"));
        }), Throws.InvalidOperationException.With.Message.EqualTo("A listen on a component handed over by a payload or a returned object needs its vendor to name an element path, so release can tell a destroyed component from a living one."));
        html.RenderPlan(plan);
    }
    [Test]
    public void a_returned_component_keeps_its_result_and_held_release()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<ListenHost>("host", HtmlString.Empty).Reactive(host => host.Changed, (outer, pipeline) =>
        {
            pipeline.Listen(outer.ReturnedPeer(), peer => peer.Updated, (inner, pipeline) => inner.Text.Set("heard"));
        });
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}},"peer":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"get","path":["instances",0],"members":"required","target":{"kind":"ref","name":"element"}}},"listen":{"kind":"method","subscribe":"hear","unsubscribe":"forget"}}},"objects":{"host":{"kind":"element","id":"host","vendor":"native"}},"handlers":[{"on":"host","path":[],"event":"changed","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["returnedPeer"],"members":"required","target":{"kind":"ref","name":"e"},"args":[]}},{"kind":"listen","event":"updated","listen":{"kind":"method","subscribe":"hear","unsubscribe":"forget"},"release":{"kind":"held","vendor":"peer","element":["host",0]},"payload":[0],"members":"required","policy":"overlap","payloadName":"listenPayload1","argumentsName":"listenArguments1","object":{"kind":"ref","name":"result1"},"do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"ref","name":"listenPayload1"},"value":{"kind":"lit","value":"heard"}}]}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_returned_component_needs_an_element_path()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        Assert.That(() => plan.Render<ListenHost>("host", HtmlString.Empty).Reactive(host => host.Changed, (outer, pipeline) =>
        {
            pipeline.Listen(outer.ReturnedUnlocated(), peer => peer.Updated, (inner, pipeline) => inner.Text.Set("heard"));
        }), Throws.InvalidOperationException.With.Message.EqualTo("A listen on a component handed over by a payload or a returned object needs its vendor to name an element path, so release can tell a destroyed component from a living one."));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_nested_listen_refuses_the_payload_of_the_listener_around_it()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        Assert.That(() => plan.Render<ListenHost>("host", HtmlString.Empty).Reactive(host => host.Changed, (outer, pipeline) =>
        {
            pipeline.Listen(outer.Feed, feed => feed.Updated, (inner, pipeline) =>
                pipeline.Listen(inner.Feed, feed => feed.Updated, (last, pipeline) => inner.Text.Set("late")));
        }), Throws.InvalidOperationException.With.Message.EqualTo("A listen cannot write the payload of the event around it: the vendor read that payload when its listener returned. Write it before the listen step."));
        html.RenderPlan(plan);
    }
}
