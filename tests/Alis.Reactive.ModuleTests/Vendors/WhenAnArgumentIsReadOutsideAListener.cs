using System;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Html;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Vendors;

[TestFixture]
public sealed class WhenAnArgumentIsReadOutsideAListener
{

    [Test]
    public void a_component_cannot_read_a_listener_argument()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        Assert.That(() => html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<NativeBell>("bell").Text.Set(pipeline.Component<NativeBell>("bell").MisplacedArgument))),
            Throws.InvalidOperationException.With.Message.EqualTo("An argument at a position is what a listener or a callback receives: declare it on an event's payload or a callback's arguments."));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_component_cannot_call_a_listener_argument()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        Assert.That(() => html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<NativeBell>("bell").MisplacedCall())),
            Throws.InvalidOperationException.With.Message.EqualTo("An argument at a position is what a listener or a callback receives: declare it on an event's payload or a callback's arguments."));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_component_cannot_pass_arguments_to_a_listener_argument()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        Assert.That(() => html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<NativeBell>("bell").MisplacedCallWith("Ada"))),
            Throws.InvalidOperationException.With.Message.EqualTo("An argument at a position is what a listener or a callback receives: declare it on an event's payload or a callback's arguments."));
        html.RenderPlan(plan);
    }

    [Test]
    public void an_object_held_by_a_payload_cannot_read_listener_arguments()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        Assert.That(() => plan.Render<NativeBell>("bell", HtmlString.Empty).Reactive(bell => bell.Rang, (ring, pipeline) => pipeline.Component<NativeBell>("bell").Text.Set(ring.Detail.MisplacedArgument)),
            Throws.InvalidOperationException.With.Message.EqualTo("An argument at a position is what a listener or a callback receives: declare it on an event's payload or a callback's arguments."));
        html.RenderPlan(plan);
    }

    [Test]
    public void an_object_held_by_a_payload_cannot_call_listener_arguments()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        Assert.That(() => plan.Render<NativeBell>("bell", HtmlString.Empty).Reactive(bell => bell.Rang, (ring, pipeline) => ring.Detail.MisplacedCall()),
            Throws.InvalidOperationException.With.Message.EqualTo("An argument at a position is what a listener or a callback receives: declare it on an event's payload or a callback's arguments."));
        html.RenderPlan(plan);
    }

    [Test]
    public void an_object_held_by_a_payload_cannot_pass_arguments_to_listener_arguments()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        Assert.That(() => plan.Render<NativeBell>("bell", HtmlString.Empty).Reactive(bell => bell.Rang, (ring, pipeline) => ring.Detail.MisplacedCallWith("Ada")),
            Throws.InvalidOperationException.With.Message.EqualTo("An argument at a position is what a listener or a callback receives: declare it on an event's payload or a callback's arguments."));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_read_requires_a_non_negative_position()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        Assert.That(() => plan.Render<NativeBell>("bell", HtmlString.Empty).Reactive(bell => bell.Searching, (search, pipeline) => pipeline.Component<NativeBell>("bell").Count.Set(search.BadPosition)),
            Throws.InvalidOperationException.With.Message.EqualTo("An argument's position must be a non-negative integer; the first argument is at position 0."));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_call_requires_a_non_negative_position()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        Assert.That(() => plan.Render<NativeBell>("bell", HtmlString.Empty).Reactive(bell => bell.Searching, (search, pipeline) => search.BadCall()),
            Throws.InvalidOperationException.With.Message.EqualTo("An argument's position must be a non-negative integer; the first argument is at position 0."));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_call_with_arguments_requires_a_non_negative_position()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        Assert.That(() => plan.Render<NativeBell>("bell", HtmlString.Empty).Reactive(bell => bell.Searching, (search, pipeline) => search.BadCallWith("Ada")),
            Throws.InvalidOperationException.With.Message.EqualTo("An argument's position must be a non-negative integer; the first argument is at position 0."));
        html.RenderPlan(plan);
    }
}
