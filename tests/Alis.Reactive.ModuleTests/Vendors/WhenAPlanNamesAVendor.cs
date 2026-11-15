using System;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Html;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Vendors;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAPlanNamesAVendor
{

    [Test]
    public void the_object_path_is_part_of_a_vendors_definition()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<PathBell>("first", HtmlString.Empty);
        Assert.That(() => plan.Render<OtherPathBell>("second", HtmlString.Empty),
            Throws.InvalidOperationException.With.Message.EqualTo("Two different vendors are named 'bell'. Give each vendor its own name."));
        html.RenderPlan(plan);
    }

    [Test]
    public void the_subscribe_method_is_part_of_a_vendors_definition()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<PathBell>("first", HtmlString.Empty);
        Assert.That(() => plan.Render<OtherSubscribeBell>("second", HtmlString.Empty),
            Throws.InvalidOperationException.With.Message.EqualTo("Two different vendors are named 'bell'. Give each vendor its own name."));
        html.RenderPlan(plan);
    }

    [Test]
    public void the_unsubscribe_method_is_part_of_a_vendors_definition()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<PathBell>("first", HtmlString.Empty);
        Assert.That(() => plan.Render<OtherUnsubscribeBell>("second", HtmlString.Empty),
            Throws.InvalidOperationException.With.Message.EqualTo("Two different vendors are named 'bell'. Give each vendor its own name."));
        html.RenderPlan(plan);
    }

    [Test]
    public void the_listening_kind_is_part_of_a_vendors_definition()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<PathBell>("first", HtmlString.Empty);
        Assert.That(() => plan.Render<CallbackBell>("second", HtmlString.Empty),
            Throws.InvalidOperationException.With.Message.EqualTo("Two different vendors are named 'bell'. Give each vendor its own name."));
        html.RenderPlan(plan);
    }

    [Test]
    public void the_element_path_is_part_of_a_vendors_definition()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<PathBell>("first", HtmlString.Empty);
        Assert.That(() => plan.Render<OtherElementBell>("second", HtmlString.Empty),
            Throws.InvalidOperationException.With.Message.EqualTo("Two different vendors are named 'bell'. Give each vendor its own name."));
        html.RenderPlan(plan);
    }

    [Test]
    public void an_omitted_element_path_differs_from_a_named_path()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<PathBell>("first", HtmlString.Empty);
        Assert.That(() => plan.Render<NoElementBell>("second", HtmlString.Empty),
            Throws.InvalidOperationException.With.Message.EqualTo("Two different vendors are named 'bell'. Give each vendor its own name."));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_lookup_differs_from_an_object_path()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<PathBell>("first", HtmlString.Empty);
        Assert.That(() => plan.Render<PluginBell>("second", HtmlString.Empty),
            Throws.InvalidOperationException.With.Message.EqualTo("Two different vendors are named 'bell'. Give each vendor its own name."));
        html.RenderPlan(plan);
    }

    [Test]
    public void the_lookup_name_is_part_of_a_vendors_definition()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<PluginBell>("first", HtmlString.Empty);
        Assert.That(() => plan.Render<OtherPluginBell>("second", HtmlString.Empty),
            Throws.InvalidOperationException.With.Message.EqualTo("Two different vendors are named 'bell'. Give each vendor its own name."));
        html.RenderPlan(plan);
    }

    [Test]
    public void an_omitted_element_path_differs_from_the_element_itself()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<NoElementBell>("first", HtmlString.Empty);
        Assert.That(() => plan.Render<EmptyElementBell>("second", HtmlString.Empty),
            Throws.InvalidOperationException.With.Message.EqualTo("Two different vendors are named 'bell'. Give each vendor its own name."));
        html.RenderPlan(plan);
    }

    [Test]
    public void separate_equal_definitions_share_one_vendor()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<PathBell>("first", HtmlString.Empty);
        plan.Render<PathBell>("second", HtmlString.Empty);
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"bell":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"get","path":["devices",2,"bell"],"members":"required","target":{"kind":"ref","name":"element"}}},"listen":{"kind":"method","subscribe":"hear","unsubscribe":"forget"}}},"objects":{"first":{"kind":"element","id":"first","vendor":"bell"},"second":{"kind":"element","id":"second","vendor":"bell"}},"handlers":[]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_reach_compares_the_element_path_with_the_rendered_definition()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<PathBell>("bell", HtmlString.Empty);
        Assert.That(() => html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<OtherElementBell>("another"))),
            Throws.InvalidOperationException.With.Message.EqualTo("Two different vendors are named 'bell'. Give each vendor its own name."));
        html.RenderPlan(plan);
    }

    [Test]
    public void two_reaches_compare_their_whole_definitions()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        Assert.That(() => html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            pipeline.Component<PathBell>("first");
            pipeline.Component<OtherElementBell>("second");
        })), Throws.InvalidOperationException.With.Message.EqualTo("Two different vendors are named 'bell'. Give each vendor its own name."));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_reach_names_the_lookup_in_its_own_plan()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<PluginBell>("bell").Text.Set("ready")));
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"bell":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"call","path":[],"members":"required","target":{"kind":"obj","id":"$plugin:ward-bells"},"args":[{"kind":"ref","name":"element"}]}},"listen":{"kind":"method","subscribe":"hear","unsubscribe":"forget"}}},"objects":{"$document":{"kind":"plugin","name":"alis.document","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}},"$plugin:ward-bells":{"kind":"plugin","name":"ward-bells","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}},"bell":{"kind":"element","id":"bell","vendor":"bell"}},"handlers":[{"on":"$document","path":[],"event":"ready","payload":[],"members":"required","policy":"overlap","do":[{"kind":"set","path":["text"],"members":"required","content":false,"target":{"kind":"obj","id":"bell"},"value":{"kind":"lit","value":"ready"}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void an_element_cannot_be_reached_as_two_vendors()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<PathBell>("bell", HtmlString.Empty);
        Assert.That(() => html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<NativeBell>("bell"))),
            Throws.InvalidOperationException.With.Message.EqualTo("Element 'bell' is named as a component of vendor 'bell' and as one of vendor 'native', but an element holds one component: name it by the declaration of the component its view renders."));
        html.RenderPlan(plan);
    }

    [Test]
    public void an_element_cannot_be_rendered_twice()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        plan.Render<NativeBell>("bell", HtmlString.Empty);
        Assert.That(() => plan.Render<NativeBell>("bell", HtmlString.Empty),
            Throws.InvalidOperationException.With.Message.EqualTo("Element 'bell' renders a second time in this plan: render each component once."));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_reach_before_a_render_is_not_a_second_render()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<PathBell>("bell")));
        Assert.That(() => plan.Render<PathBell>("bell", HtmlString.Empty), Throws.Nothing);
        Assert.That(PlanSchema.ProblemsOf(BellPlans.JsonOf(html.RenderPlan(plan))), Is.Empty);
    }
}
