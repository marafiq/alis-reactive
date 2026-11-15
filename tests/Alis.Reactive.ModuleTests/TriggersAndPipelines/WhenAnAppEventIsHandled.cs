using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;
using static Alis.Reactive.ModuleTests.TriggersAndPipelines.CarePlans;

namespace Alis.Reactive.ModuleTests.TriggersAndPipelines;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAnAppEventIsHandled
{
    [Test]
    public void an_event_with_no_payload_is_heard_on_the_document_by_its_members_name_in_camel_case_under_overlap()
    {
        var html = Views.Of(new CareCall("North"));
        var plan = html.ReactivePlan<CareCall>();
        html.On(plan, trigger => trigger.CustomEvent(CareEvents.RoundStarted, pipeline => pipeline.Plugin<CareDesk>().Reason.Set("started")));
        var json = JsonOf(html.RenderPlan(plan));
        var expected = Plan(Document + "," + Desk,
            Handler("roundStarted", "[]", "overlap", SetsTheDesk("reason", """{"kind":"lit","value":"started"}""")));
        Assert.That(json, Is.EqualTo(expected));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void each_handler_keeps_the_policy_it_names()
    {
        var html = Views.Of(new CareCall("North"));
        var plan = html.ReactivePlan<CareCall>();
        html.On(plan, trigger => trigger
            .CustomEvent(CareEvents.RoundStarted, HandlerPolicy.Skip, pipeline => pipeline.Plugin<CareDesk>().Reason.Set("started"))
            .CustomEvent(CareEvents.Flagged, HandlerPolicy.Replace, (flagged, pipeline) => pipeline.Plugin<CareDesk>().Reason.Set("flagged")));
        var json = JsonOf(html.RenderPlan(plan));
        var expected = Plan(Document + "," + Desk,
            Handler("roundStarted", "[]", "skip", SetsTheDesk("reason", """{"kind":"lit","value":"started"}""")) + "," +
            Handler("flagged", """[0,"detail"]""", "replace", SetsTheDesk("reason", """{"kind":"lit","value":"flagged"}""")));
        Assert.That(json, Is.EqualTo(expected));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_payload_is_read_at_the_events_detail_with_each_number_through_its_shape_and_text_as_it_is()
    {
        var html = Views.Of(new CareCall("North"));
        var plan = html.ReactivePlan<CareCall>();
        html.On(plan, trigger => trigger.CustomEvent(CareEvents.Flagged, (flagged, pipeline) =>
        {
            pipeline.Plugin<CareDesk>().Room.Set(flagged.Read(x => x.Room));
            pipeline.Plugin<CareDesk>().Reason.Set(flagged.Read(x => x.Reason));
        }));
        var json = JsonOf(html.RenderPlan(plan));
        var expected = Plan(Document + "," + Desk,
            Handler("flagged", """[0,"detail"]""", "overlap",
                SetsTheDesk("room", ReadAs(PayloadMember("room", "0"), AnInteger)) + "," +
                SetsTheDesk("reason", PayloadMember("reason", "\"unknown\""))));
        Assert.That(json, Is.EqualTo(expected));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_boolean_a_nullable_number_a_date_an_enum_and_a_fraction_each_read_through_their_shape_and_a_name_given_is_the_events()
    {
        var html = Views.Of(new CareCall("North"));
        var plan = html.ReactivePlan<CareCall>();
        html.On(plan, trigger => trigger.CustomEvent(CareEvents.Escalated, (escalation, pipeline) =>
        {
            pipeline.Plugin<CareDesk>().Paged.Set(escalation.Read(x => x.Paged));
            pipeline.Plugin<CareDesk>().Bed.Set(escalation.Read(x => x.Bed));
            pipeline.Plugin<CareDesk>().Due.Set(escalation.Read(x => x.Due));
            pipeline.Plugin<CareDesk>().Level.Set(escalation.Read(x => x.Level));
            pipeline.Plugin<CareDesk>().Dose.Set(escalation.Read(x => x.Dose));
            pipeline.Plugin<CareDesk>().Note.Set(escalation.Read(x => x.Note));
        }));
        var json = JsonOf(html.RenderPlan(plan));
        var expected = Plan(Document + "," + Desk,
            Handler("care-escalated", """[0,"detail"]""", "overlap",
                SetsTheDesk("paged", ReadAs(PayloadMember("paged", "false"), """{"kind":"boolean"}""")) + "," +
                SetsTheDesk("bed", ReadAs(PayloadMember("bed", "null"), "{\"kind\":\"nullable\",\"inner\":" + AnInteger + "}")) + "," +
                SetsTheDesk("due", ReadAs(PayloadMember("due", "\"0001-01-01T00:00:00\""), """{"kind":"date"}""")) + "," +
                SetsTheDesk("level", ReadAs(PayloadMember("level", "0"), AnInteger)) + "," +
                SetsTheDesk("dose", ReadAs(PayloadMember("dose", "0"), AFraction)) + "," +
                SetsTheDesk("note", PayloadMember("note", "null"))));
        Assert.That(json, Is.EqualTo(expected));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }
}
