using System;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Html;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.PartialsAndSlots;

[TestFixture]
public sealed class WhenAViewsPlanHasRendered
{
    [Test]
    public void a_second_render_a_section_runs_is_refused_since_every_handler_would_run_twice()
    {
        var html = Views.Of(new Round("Morning"));
        var plan = html.ReactivePlan<Round>();
        Func<IHtmlContent> aSectionsRender = () => html.RenderPlan(plan);
        html.RenderPlan(plan);

        Assert.That(
            () => aSectionsRender(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "The plan of 'Alis.Reactive.ModuleTests.PartialsAndSlots.Round' is rendered twice, so every handler it holds would run twice; " +
                "render it once, as the view's last line."));
    }

    [Test]
    public void a_door_a_section_runs_after_the_render_is_refused_since_the_page_never_runs_what_it_writes()
    {
        var html = Views.Of(new Round("Morning"));
        var plan = html.ReactivePlan<Round>();
        Action aSectionsDoor = () => html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Element("round-status").Text.Set("done")));
        html.RenderPlan(plan);

        Assert.That(
            () => aSectionsDoor(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A door takes the plan of 'Alis.Reactive.ModuleTests.PartialsAndSlots.Round' after it rendered, so the page never runs what the door writes; " +
                "write every door above Html.RenderPlan, and where a @section holds one, render the plan in a section the layout renders after it."));
    }

    [Test]
    public void a_door_a_section_runs_before_the_one_render_is_accepted()
    {
        var html = Views.Of(new Round("Morning"));
        var plan = html.ReactivePlan<Round>();
        Action aSectionsDoor = () => html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Element("round-status").Text.Set("done")));
        aSectionsDoor();

        Assert.That(() => html.RenderPlan(plan), Throws.Nothing);
    }
}

public sealed record Round(string Name);
