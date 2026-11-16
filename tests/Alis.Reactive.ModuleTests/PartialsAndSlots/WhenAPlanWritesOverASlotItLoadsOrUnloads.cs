using System;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;
using static Alis.Reactive.ModuleTests.PartialsAndSlots.CardPlans;

namespace Alis.Reactive.ModuleTests.PartialsAndSlots;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAPlanWritesOverASlotItLoadsOrUnloads
{
    private const string TheText =
        "The plan sets the text of element 'card', a slot this plan loads or unloads, so what the slot loaded would keep running under content it no longer shows. " +
        "Empty the slot with Unload(\"card\"), and write the text into an element beside it.";

    private const string TheHtml =
        "The plan sets the HTML of element 'card', a slot this plan loads or unloads, so what the slot loaded would keep running under content it no longer shows. " +
        "Empty the slot with Unload(\"card\"), and write the HTML into an element beside it.";

    private static readonly TestCaseData[] ASetOfTheSlotsTextOrHtml =
    [
        new TestCaseData((Action<Pipeline<CardDesk>>)(pipeline =>
        {
            pipeline.Element("card").Text.Set("Signed out");
            pipeline.Get("/cards").Into("card");
        }), TheText).SetArgDisplayNames("the text, then the load"),
        new TestCaseData((Action<Pipeline<CardDesk>>)(pipeline =>
        {
            pipeline.Get("/cards").Into("card");
            pipeline.Element("card").Text.Set("Signed out");
        }), TheText).SetArgDisplayNames("the load, then the text"),
        new TestCaseData((Action<Pipeline<CardDesk>>)(pipeline =>
        {
            pipeline.Element("card").Html.Set("<p>Signed out</p>");
            pipeline.Unload("card");
        }), TheHtml).SetArgDisplayNames("the HTML, then the unload"),
        new TestCaseData((Action<Pipeline<CardDesk>>)(pipeline =>
        {
            pipeline.Unload("card");
            pipeline.Element("card").Html.Set("<p>Signed out</p>");
        }), TheHtml).SetArgDisplayNames("the unload, then the HTML"),
        new TestCaseData((Action<Pipeline<CardDesk>>)(pipeline =>
        {
            pipeline.Element("card").Text.Set(pipeline.Element("card-note").Text);
            pipeline.Unload("card");
        }), TheText).SetArgDisplayNames("a source's text, then the unload"),
        new TestCaseData((Action<Pipeline<CardDesk>>)(pipeline => pipeline.Get("/cards").OnSuccess(Content.Html, (reply, pipeline) =>
        {
            pipeline.Into("card", reply.Body);
            pipeline.Element("card").Html.Set(reply.Status);
        })), TheHtml).SetArgDisplayNames("the load, then the HTML, in its route"),
    ];

    private static readonly TestCaseData[] AWriteTheSlotTakes =
    [
        new TestCaseData((Action<Pipeline<CardDesk>>)(pipeline =>
        {
            pipeline.Get("/cards").Into("card");
            pipeline.Element("card").Text.Append("Noted");
            pipeline.Element("card").Html.Append("<p>Noted</p>");
            pipeline.Unload("card");
        })).SetArgDisplayNames("an append to the slot"),
        new TestCaseData((Action<Pipeline<CardDesk>>)(pipeline =>
        {
            pipeline.Element("card-note").Text.Set("Signed out");
            pipeline.Get("/cards").Into("card");
            pipeline.Unload("card");
            pipeline.Element("card-note").Html.Set("<p>Signed out</p>");
        })).SetArgDisplayNames("a set of the element beside the slot"),
    ];

    [TestCaseSource(nameof(ASetOfTheSlotsTextOrHtml))]
    public void a_set_of_the_text_or_html_of_the_slot_is_refused_where_the_view_renders_in_either_order(Action<Pipeline<CardDesk>> write, string refusal)
        => Assert.That(() => Rendered(write), Throws.InvalidOperationException.With.Message.EqualTo(refusal));

    [Test]
    public void a_set_in_another_handler_of_the_plan_is_refused_as_one_in_the_handler_that_loads()
        => Assert.That(
            () =>
            {
                var html = Views.Of(new CardDesk("North"), _ => { }, app => app.AddReactiveRequests(requests => requests.NoAntiforgery()));
                var plan = html.ReactivePlan<CardDesk>();
                html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Get("/cards").Into("card")));
                html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Element("card").Text.Set("Signed out")));
                return Views.TextOf(html.RenderPlan(plan));
            },
            Throws.InvalidOperationException.With.Message.EqualTo(TheText));

    [TestCaseSource(nameof(AWriteTheSlotTakes))]
    public void an_append_to_the_slot_and_a_set_beside_it_are_accepted(Action<Pipeline<CardDesk>> write)
        => Assert.That(() => Rendered(write), Throws.Nothing);
}
