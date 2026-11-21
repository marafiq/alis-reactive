using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Plugins;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAStepReachesAPlugin
{
    [Test]
    public void a_step_reads_sets_and_calls_a_plugins_members_on_the_object_its_name_names()
    {
        var html = Views.Of(new NightRound("North"));
        var plan = html.ReactivePlan<NightRound>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var log = pipeline.Plugin<ShiftLog>();
            log.Count.Set(log.Entries);
            log.Add("night");
            log.Count.Set(log.Total());
        }));
        var json = RoundPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Plugins.NightRound","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{},"objects":{"$document":{"kind":"plugin","name":"alis.document","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}},"$plugin:shiftlog":{"kind":"plugin","name":"shiftlog","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}}},"handlers":[{"on":"$document","path":[],"event":"ready","payload":[],"members":"required","policy":"overlap","do":[{"kind":"set","path":["count"],"members":"required","content":false,"target":{"kind":"obj","id":"$plugin:shiftlog"},"value":{"kind":"get","path":["entries"],"members":"required","target":{"kind":"obj","id":"$plugin:shiftlog"}}},{"kind":"do","await":false,"expr":{"kind":"call","path":["add"],"members":"required","target":{"kind":"obj","id":"$plugin:shiftlog"},"args":[{"kind":"lit","value":"night"}]}},{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["total"],"members":"required","target":{"kind":"obj","id":"$plugin:shiftlog"},"args":[]}},{"kind":"set","path":["count"],"members":"required","content":false,"target":{"kind":"obj","id":"$plugin:shiftlog"},"value":{"kind":"ref","name":"result1"}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void two_declarations_of_one_name_reach_one_object_heard_as_an_event_target_while_no_handler_hears_it()
    {
        var html = Views.Of(new NightRound("North"));
        var plan = html.ReactivePlan<NightRound>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var clock = pipeline.Plugin<NightClock>();
            clock.Shown.Set(pipeline.Plugin<NightClockFace>().Hour);
        }));
        var json = RoundPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Plugins.NightRound","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{},"objects":{"$document":{"kind":"plugin","name":"alis.document","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}},"$plugin:nightclock":{"kind":"plugin","name":"nightclock","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}}},"handlers":[{"on":"$document","path":[],"event":"ready","payload":[],"members":"required","policy":"overlap","do":[{"kind":"set","path":["shown"],"members":"required","content":false,"target":{"kind":"obj","id":"$plugin:nightclock"},"value":{"kind":"get","path":["hour"],"members":"required","target":{"kind":"obj","id":"$plugin:nightclock"}}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_step_after_the_handler_of_another_declaration_of_its_name_keeps_the_handlers_listening()
    {
        var html = Views.Of(new NightRound("North"));
        var plan = html.ReactivePlan<NightRound>();
        plan.Plugin<NightClock>().Reactive(clock => clock.Struck, pipeline => pipeline.Plugin<NightClock>().Shown.Set("midnight"));
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Plugin<NightClock>().Shown.Set(pipeline.Plugin<NightClockFace>().Hour)));
        var json = RoundPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Plugins.NightRound","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{},"objects":{"$document":{"kind":"plugin","name":"alis.document","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}},"$plugin:nightclock":{"kind":"plugin","name":"nightclock","key":"","listen":{"kind":"assignment"},"data":{"kind":"none"}}},"handlers":[{"on":"$plugin:nightclock","path":[],"event":"onStruck","payload":[],"members":"required","policy":"overlap","do":[{"kind":"set","path":["shown"],"members":"required","content":false,"target":{"kind":"obj","id":"$plugin:nightclock"},"value":{"kind":"lit","value":"midnight"}}]},{"on":"$document","path":[],"event":"ready","payload":[],"members":"required","policy":"overlap","do":[{"kind":"set","path":["shown"],"members":"required","content":false,"target":{"kind":"obj","id":"$plugin:nightclock"},"value":{"kind":"get","path":["hour"],"members":"required","target":{"kind":"obj","id":"$plugin:nightclock"}}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_step_before_the_handler_of_another_declaration_of_its_name_takes_the_handlers_listening()
    {
        var html = Views.Of(new NightRound("North"));
        var plan = html.ReactivePlan<NightRound>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Plugin<NightClock>().Shown.Set(pipeline.Plugin<NightClockFace>().Hour)));
        plan.Plugin<NightClock>().Reactive(clock => clock.Struck, pipeline => pipeline.Plugin<NightClock>().Shown.Set("midnight"));
        var json = RoundPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Plugins.NightRound","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{},"objects":{"$document":{"kind":"plugin","name":"alis.document","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}},"$plugin:nightclock":{"kind":"plugin","name":"nightclock","key":"","listen":{"kind":"assignment"},"data":{"kind":"none"}}},"handlers":[{"on":"$document","path":[],"event":"ready","payload":[],"members":"required","policy":"overlap","do":[{"kind":"set","path":["shown"],"members":"required","content":false,"target":{"kind":"obj","id":"$plugin:nightclock"},"value":{"kind":"get","path":["hour"],"members":"required","target":{"kind":"obj","id":"$plugin:nightclock"}}}]},{"on":"$plugin:nightclock","path":[],"event":"onStruck","payload":[],"members":"required","policy":"overlap","do":[{"kind":"set","path":["shown"],"members":"required","content":false,"target":{"kind":"obj","id":"$plugin:nightclock"},"value":{"kind":"lit","value":"midnight"}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_vendors_lookup_by_the_name_after_its_handler_keeps_the_handlers_listening()
    {
        var html = Views.Of(new NightRound("North"));
        var plan = html.ReactivePlan<NightRound>();
        plan.Plugin<NightClock>().Reactive(clock => clock.Struck, pipeline => pipeline.Component<ClockFace>("face").Hour.Set("midnight"));
        var json = RoundPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Plugins.NightRound","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"clock-face":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"call","path":[],"members":"required","target":{"kind":"obj","id":"$plugin:nightclock"},"args":[{"kind":"ref","name":"element"}]}},"listen":{"kind":"method","subscribe":"on","unsubscribe":"off"}}},"objects":{"$plugin:nightclock":{"kind":"plugin","name":"nightclock","key":"","listen":{"kind":"assignment"},"data":{"kind":"none"}},"face":{"kind":"element","id":"face","vendor":"clock-face"}},"handlers":[{"on":"$plugin:nightclock","path":[],"event":"onStruck","payload":[],"members":"required","policy":"overlap","do":[{"kind":"set","path":["hour"],"members":"required","content":false,"target":{"kind":"obj","id":"face"},"value":{"kind":"lit","value":"midnight"}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void two_declarations_of_one_name_heard_by_different_listenings_are_refused_naming_both()
    {
        var html = Views.Of(new NightRound("North"));
        var plan = html.ReactivePlan<NightRound>();
        plan.Plugin<NightClock>().Reactive(clock => clock.Struck, pipeline => pipeline.Plugin<NightClock>().Shown.Set("midnight"));
        Assert.That(
            () => plan.Plugin<NightChime>().Reactive(chime => chime.Chimed, pipeline => pipeline.Plugin<NightClock>().Shown.Set("chimed")),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "Two declarations of the plugin 'nightclock' are heard two ways in one plan: the one whose event is 'onStruck' by EventListening.CallbackProperties, and the one whose event is 'chimed' by EventListening.EventTarget. A plugin is one object: give every declaration of 'nightclock' that declares an event one EventListening."));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_listen_on_a_declaration_heard_another_way_than_a_handler_of_its_name_is_refused_naming_both()
    {
        var html = Views.Of(new NightRound("North"));
        var plan = html.ReactivePlan<NightRound>();
        Assert.That(
            () => plan.Plugin<NightClock>().Reactive(clock => clock.Struck, pipeline =>
                pipeline.Listen(pipeline.Plugin<NightChime>(), chime => chime.Chimed, (_, pipeline) => pipeline.Plugin<NightClock>().Shown.Set("chimed"))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "Two declarations of the plugin 'nightclock' are heard two ways in one plan: the one whose event is 'onStruck' by EventListening.CallbackProperties, and the one whose event is 'chimed' by EventListening.EventTarget. A plugin is one object: give every declaration of 'nightclock' that declares an event one EventListening."));
        html.RenderPlan(plan);
    }

    [Test]
    public void two_declarations_of_one_name_heard_by_one_listening_share_its_locator()
    {
        var html = Views.Of(new NightRound("North"));
        var plan = html.ReactivePlan<NightRound>();
        plan.Plugin<WardDesk>().Reactive(desk => desk.Called, pipeline => pipeline.Plugin<WardDesk>().Note.Set("called"));
        plan.Plugin<WardDeskBell>().Reactive(bell => bell.Rang, pipeline => pipeline.Plugin<WardDesk>().Note.Set("rang"));
        var json = RoundPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Plugins.NightRound","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{},"objects":{"$plugin:ward-desk_2":{"kind":"plugin","name":"ward-desk_2","key":"","listen":{"kind":"method","subscribe":"hear","unsubscribe":"forget"},"data":{"kind":"none"}}},"handlers":[{"on":"$plugin:ward-desk_2","path":[],"event":"called","payload":[],"members":"required","policy":"overlap","do":[{"kind":"set","path":["note"],"members":"required","content":false,"target":{"kind":"obj","id":"$plugin:ward-desk_2"},"value":{"kind":"lit","value":"called"}}]},{"on":"$plugin:ward-desk_2","path":[],"event":"rang","payload":[],"members":"required","policy":"overlap","do":[{"kind":"set","path":["note"],"members":"required","content":false,"target":{"kind":"obj","id":"$plugin:ward-desk_2"},"value":{"kind":"lit","value":"rang"}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }
}
