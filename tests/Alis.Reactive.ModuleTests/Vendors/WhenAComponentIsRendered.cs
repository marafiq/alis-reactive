using System;
using System.Text.Json;
using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Html;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Vendors;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAComponentIsRendered
{

    [Test]
    public void an_event_without_a_payload_requires_no_argument()
    {
        var html = Views.Of(new Ward("North"), options => options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        var plan = html.ReactivePlan<Ward>();
        plan.Render<NativeBell>("bell", HtmlString.Empty).Reactive(bell => bell.Silenced, pipeline => pipeline.Component<NativeBell>("bell").Text.Set("quiet"));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"bell":{"kind":"element","id":"bell","vendor":"native"}},"handlers":[{"on":"bell","path":[],"event":"silenced","payload":[],"members":"required","policy":"overlap","do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"obj","id":"bell"},"value":{"kind":"lit","value":"quiet"}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void an_event_name_can_follow_its_javascript_api()
    {
        var html = Views.Of(new Ward("North"), options => options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        var plan = html.ReactivePlan<Ward>();
        plan.Render<NativeBell>("bell", HtmlString.Empty).Reactive(bell => bell.Cleared, pipeline => pipeline.Component<NativeBell>("bell").Text.Set("clear"));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"bell":{"kind":"element","id":"bell","vendor":"native"}},"handlers":[{"on":"bell","path":[],"event":"clear-all","payload":[],"members":"required","policy":"overlap","do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"obj","id":"bell"},"value":{"kind":"lit","value":"clear"}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_live_payload_reads_its_required_member()
    {
        var html = Views.Of(new Ward("North"), options => options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        var plan = html.ReactivePlan<Ward>();
        plan.Render<NativeBell>("bell", HtmlString.Empty).Reactive(bell => bell.Rang, (ring, pipeline) => pipeline.Component<NativeBell>("bell").Text.Set(ring.Room));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"bell":{"kind":"element","id":"bell","vendor":"native"}},"handlers":[{"on":"bell","path":[],"event":"rang","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"obj","id":"bell"},"value":{"kind":"get","path":["room"],"members":"required","target":{"kind":"ref","name":"e"}}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_named_event_keeps_its_payload()
    {
        var html = Views.Of(new Ward("North"), options => options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        var plan = html.ReactivePlan<Ward>();
        plan.Render<NativeBell>("bell", HtmlString.Empty).Reactive(bell => bell.Urgent, (ring, pipeline) => pipeline.Component<NativeBell>("bell").Text.Set(ring.Room));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"bell":{"kind":"element","id":"bell","vendor":"native"}},"handlers":[{"on":"bell","path":[],"event":"ring-now","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"obj","id":"bell"},"value":{"kind":"get","path":["room"],"members":"required","target":{"kind":"ref","name":"e"}}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_payloads_optional_member_keeps_its_presence()
    {
        var html = Views.Of(new Ward("North"), options => options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        var plan = html.ReactivePlan<Ward>();
        plan.Render<NativeBell>("bell", HtmlString.Empty).Reactive(bell => bell.Rang, (ring, pipeline) => pipeline.Component<NativeBell>("bell").Text.Set(ring.Note));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"bell":{"kind":"element","id":"bell","vendor":"native"}},"handlers":[{"on":"bell","path":[],"event":"rang","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"obj","id":"bell"},"value":{"kind":"get","path":["note"],"members":"optionalMember","target":{"kind":"ref","name":"e"}}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_payload_can_be_written_in_its_handler()
    {
        var html = Views.Of(new Ward("North"), options => options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        var plan = html.ReactivePlan<Ward>();
        plan.Render<NativeBell>("bell", HtmlString.Empty).Reactive(bell => bell.Rang, (ring, pipeline) => ring.Cancel.Set(true));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"bell":{"kind":"element","id":"bell","vendor":"native"}},"handlers":[{"on":"bell","path":[],"event":"rang","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"set","path":["cancel"],"members":"required","content":false,"target":{"kind":"ref","name":"e"},"value":{"kind":"lit","value":true}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void an_object_held_by_the_payload_keeps_its_own_boundary()
    {
        var html = Views.Of(new Ward("North"), options => options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        var plan = html.ReactivePlan<Ward>();
        plan.Render<NativeBell>("bell", HtmlString.Empty).Reactive(bell => bell.Rang, (ring, pipeline) => pipeline.Component<NativeBell>("bell").Text.Set(ring.Detail.Room));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"bell":{"kind":"element","id":"bell","vendor":"native"}},"handlers":[{"on":"bell","path":[],"event":"rang","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"obj","id":"bell"},"value":{"kind":"get","path":["room"],"members":"required","target":{"kind":"get","path":["detail"],"members":"required","target":{"kind":"ref","name":"e"}}}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_payloads_method_result_is_bound_by_its_handler()
    {
        var html = Views.Of(new Ward("North"), options => options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        var plan = html.ReactivePlan<Ward>();
        plan.Render<NativeBell>("bell", HtmlString.Empty).Reactive(bell => bell.Rang, (ring, pipeline) => pipeline.Component<NativeBell>("bell").Count.Set(ring.Number()));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"bell":{"kind":"element","id":"bell","vendor":"native"}},"handlers":[{"on":"bell","path":[],"event":"rang","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["number"],"members":"required","target":{"kind":"ref","name":"e"},"args":[]}},{"kind":"set","path":["count"],"members":"required","content":false,"target":{"kind":"obj","id":"bell"},"value":{"kind":"ref","name":"result1"}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void the_first_argument_is_read_by_its_position()
    {
        var html = Views.Of(new Ward("North"), options => options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        var plan = html.ReactivePlan<Ward>();
        plan.Render<NativeBell>("bell", HtmlString.Empty).Reactive(bell => bell.Searching, (search, pipeline) => pipeline.Component<NativeBell>("bell").Text.Set(search.Query));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"bell":{"kind":"element","id":"bell","vendor":"native"}},"handlers":[{"on":"bell","path":[],"event":"searching","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"obj","id":"bell"},"value":{"kind":"get","path":[0],"members":"required","target":{"kind":"ref","name":"arguments"}}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_later_argument_is_read_by_its_position()
    {
        var html = Views.Of(new Ward("North"), options => options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        var plan = html.ReactivePlan<Ward>();
        plan.Render<NativeBell>("bell", HtmlString.Empty).Reactive(bell => bell.Searching, (search, pipeline) => pipeline.Component<NativeBell>("bell").Text.Set(search.Context));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"bell":{"kind":"element","id":"bell","vendor":"native"}},"handlers":[{"on":"bell","path":[],"event":"searching","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"obj","id":"bell"},"value":{"kind":"get","path":[2],"members":"required","target":{"kind":"ref","name":"arguments"}}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_continuation_receives_its_arguments_in_order()
    {
        var html = Views.Of(new Ward("North"), options => options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        var plan = html.ReactivePlan<Ward>();
        plan.Render<NativeBell>("bell", HtmlString.Empty).Reactive(bell => bell.Searching, (search, pipeline) => search.Show(search.Query, 3));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"bell":{"kind":"element","id":"bell","vendor":"native"}},"handlers":[{"on":"bell","path":[],"event":"searching","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"do","await":false,"expr":{"kind":"call","path":[1],"members":"required","target":{"kind":"ref","name":"arguments"},"args":[{"kind":"get","path":[0],"members":"required","target":{"kind":"ref","name":"arguments"}},{"kind":"lit","value":3}]}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_continuation_can_receive_no_arguments()
    {
        var html = Views.Of(new Ward("North"), options => options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        var plan = html.ReactivePlan<Ward>();
        plan.Render<NativeBell>("bell", HtmlString.Empty).Reactive(bell => bell.Searching, (search, pipeline) => search.Finish());
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"bell":{"kind":"element","id":"bell","vendor":"native"}},"handlers":[{"on":"bell","path":[],"event":"searching","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"do","await":false,"expr":{"kind":"call","path":[3],"members":"required","target":{"kind":"ref","name":"arguments"},"args":[]}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_payloadless_event_stays_payloadless_through_the_generic_door()
    {
        var html = Views.Of(new Ward("North"), options => options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        var plan = html.ReactivePlan<Ward>();
        plan.Render<NativeBell>("bell", HtmlString.Empty).Reactive(bell => (Event<NoPayload>)bell.Silenced, (nothing, pipeline) => pipeline.Component<NativeBell>("bell").Text.Set("quiet"));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"bell":{"kind":"element","id":"bell","vendor":"native"}},"handlers":[{"on":"bell","path":[],"event":"silenced","payload":[],"members":"required","policy":"overlap","do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"obj","id":"bell"},"value":{"kind":"lit","value":"quiet"}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void each_handler_keeps_its_payload_and_its_place()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<NativeBell>("bell", HtmlString.Empty)
            .Reactive(bell => bell.Rang, (ring, pipeline) => ring.Cancel.Set(true))
            .Reactive(bell => bell.Silenced, pipeline => pipeline.Component<NativeBell>("bell").Text.Set("quiet"));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"bell":{"kind":"element","id":"bell","vendor":"native"}},"handlers":[{"on":"bell","path":[],"event":"rang","payload":[0],"members":"required","policy":"overlap","do":[{"kind":"set","path":["cancel"],"members":"required","content":false,"target":{"kind":"ref","name":"e"},"value":{"kind":"lit","value":true}}]},{"on":"bell","path":[],"event":"silenced","payload":[],"members":"required","policy":"overlap","do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"obj","id":"bell"},"value":{"kind":"lit","value":"quiet"}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void the_rendered_component_writes_the_supplied_markup()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        var rendered = plan.Render<NativeBell>("bell", new HtmlContentBuilder().AppendHtml("<bell>").Append("North & South").AppendHtml("</bell>"));
        Assert.That(Views.TextOf(rendered), Is.EqualTo("<bell>North &amp; South</bell>"));
        Assert.That(PlanSchema.ProblemsOf(BellPlans.JsonOf(html.RenderPlan(plan))), Is.Empty);
    }

    [Test]
    public void a_native_component_listens_as_an_event_target()
    {
        Assert.That(new NativeBell().HeardBy, Is.SameAs(EventListening.EventTarget));
    }

    [Test]
    public void a_callback_component_listens_through_callback_properties()
    {
        Assert.That(new CallbackBell().HeardBy, Is.SameAs(EventListening.CallbackProperties));
    }
}
