using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Plugins;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAPluginCallsItself
{
    [Test]
    public void a_plugin_that_is_a_function_is_called_with_no_path_at_its_line()
    {
        var html = Views.Of(new NightRound("North"));
        var plan = html.ReactivePlan<NightRound>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var initials = pipeline.Plugin<Initials>();
            initials.Refresh();
            initials.Refresh("Ada Hart");
        }));
        var json = RoundPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Plugins.NightRound","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{},"objects":{"$document":{"kind":"plugin","name":"alis.document","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}},"$plugin:initials":{"kind":"plugin","name":"initials","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}}},"handlers":[{"on":"$document","path":[],"event":"ready","payload":[],"members":"required","policy":"overlap","do":[{"kind":"do","await":false,"expr":{"kind":"call","path":[],"members":"required","target":{"kind":"obj","id":"$plugin:initials"},"args":[]}},{"kind":"do","await":false,"expr":{"kind":"call","path":[],"members":"required","target":{"kind":"obj","id":"$plugin:initials"},"args":[{"kind":"lit","value":"Ada Hart"}]}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_call_of_the_plugin_itself_binds_what_it_returned_at_its_line()
    {
        var html = Views.Of(new NightRound("North"));
        var plan = html.ReactivePlan<NightRound>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var log = pipeline.Plugin<ShiftLog>();
            var initials = pipeline.Plugin<Initials>().Of("Ada Hart");
            log.Add("night");
            log.Note.Set(initials);
        }));
        var json = RoundPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Plugins.NightRound","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{},"objects":{"$document":{"kind":"plugin","name":"alis.document","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}},"$plugin:initials":{"kind":"plugin","name":"initials","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}},"$plugin:shiftlog":{"kind":"plugin","name":"shiftlog","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}}},"handlers":[{"on":"$document","path":[],"event":"ready","payload":[],"members":"required","policy":"overlap","do":[{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":[],"members":"required","target":{"kind":"obj","id":"$plugin:initials"},"args":[{"kind":"lit","value":"Ada Hart"}]}},{"kind":"do","await":false,"expr":{"kind":"call","path":["add"],"members":"required","target":{"kind":"obj","id":"$plugin:shiftlog"},"args":[{"kind":"lit","value":"night"}]}},{"kind":"set","path":["note"],"members":"required","content":false,"target":{"kind":"obj","id":"$plugin:shiftlog"},"value":{"kind":"ref","name":"result1"}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }
}
