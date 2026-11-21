using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Plugins;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAPluginIsNamed
{
    [Test]
    public void a_declaration_keeps_the_name_it_is_given_where_it_is_made()
    {
        Assert.That(new DottedLog().Name, Is.EqualTo("alis.log"));
    }

    [Test]
    public void a_step_reaching_a_plugin_named_with_a_dot_is_refused_where_the_view_renders()
    {
        var html = Views.Of(new NightRound("North"));
        var plan = html.ReactivePlan<NightRound>();
        Assert.That(
            () => html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Plugin<DottedLog>().Count.Set(1))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "Plugin name 'alis.log' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen. A name with a dot is the framework's own."));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_handler_of_a_plugin_named_with_a_dot_is_refused_where_the_view_renders()
    {
        var html = Views.Of(new NightRound("North"));
        var plan = html.ReactivePlan<NightRound>();
        Assert.That(
            () => plan.Plugin<DottedLog>(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "Plugin name 'alis.log' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen. A name with a dot is the framework's own."));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_name_that_starts_with_a_digit_is_refused_where_the_view_renders()
    {
        var html = Views.Of(new NightRound("North"));
        var plan = html.ReactivePlan<NightRound>();
        Assert.That(
            () => html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Plugin<NumberedLog>().Count.Set(1))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "Plugin name '2nd-log' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen. A name with a dot is the framework's own."));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_name_of_letters_digits_underscores_and_hyphens_names_its_object()
    {
        var html = Views.Of(new NightRound("North"));
        var plan = html.ReactivePlan<NightRound>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Plugin<WardDesk>().Note.Set("open")));
        var json = RoundPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Plugins.NightRound","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{},"objects":{"$document":{"kind":"plugin","name":"alis.document","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}},"$plugin:ward-desk_2":{"kind":"plugin","name":"ward-desk_2","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}}},"handlers":[{"on":"$document","path":[],"event":"ready","payload":[],"members":"required","policy":"overlap","do":[{"kind":"set","path":["note"],"members":"required","content":false,"target":{"kind":"obj","id":"$plugin:ward-desk_2"},"value":{"kind":"lit","value":"open"}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }
}
