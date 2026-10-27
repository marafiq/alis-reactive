using System.Text.Json.Nodes;
using Alis.Reactive.PlanTests.Support;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.PlanTests.Triggers;

/// <summary>A trigger is a handler of one object's event; its pipeline is the statements the plan runs.</summary>
[TestFixture]
public class WhenAPageDeclaresTriggers
{
    [Test]
    public void a_ready_trigger_is_one_handler_of_the_document_run_at_boot()
    {
        var plan = Page.Plan<Admission>();
        plan.Triggers().DomReady(p => p.Element("status").Text.Set("Round open"));

        var rendered = plan.Rendered();

        var handler = rendered.Handlers().Single()!;
        Assert.That(handler["on"]!.GetValue<string>(), Is.EqualTo("$document"));
        Assert.That(handler["event"]!.GetValue<string>(), Is.EqualTo("ready"));
        Golden.Matches(rendered, "Triggers", nameof(a_ready_trigger_is_one_handler_of_the_document_run_at_boot));
    }

    [Test]
    public void a_click_on_a_declared_element_runs_the_pipeline_written_for_it()
    {
        var plan = Page.Plan<Admission>();
        plan.Render<Button>("save", HtmlString.Empty).Reactive(button => button.Click, p => p.Element("status").Text.Set("Saved"));

        var rendered = plan.Rendered();

        var handler = rendered.Handlers().Single()!;
        Assert.That(handler["on"]!.GetValue<string>(), Is.EqualTo("save"));
        Assert.That(handler["event"]!.GetValue<string>(), Is.EqualTo("click"));
        Assert.That(rendered.Objects()["save"]!["vendor"]!.GetValue<string>(), Is.EqualTo("native"));
        Golden.Matches(rendered, "Triggers", nameof(a_click_on_a_declared_element_runs_the_pipeline_written_for_it));
    }

    [Test]
    public void an_event_dispatched_with_a_payload_is_heard_with_that_payload()
    {
        var plan = Page.Plan<Admission>();
        plan.Triggers()
            .DomReady(p => p.Dispatch(AdmissionEvents.RoomChecked, new RoomCheck { Room = "214" }))
            .CustomEvent(AdmissionEvents.RoomChecked, (check, p) => p.Element("room").Text.Set(check.Get(x => x.Room)));

        var rendered = plan.Rendered();

        var hearing = rendered.Handlers()[1]!;
        Assert.That(hearing["on"]!.GetValue<string>(), Is.EqualTo("$document"));
        Assert.That(hearing["event"]!.GetValue<string>(), Is.EqualTo(AdmissionEvents.RoomChecked.Name));
        Golden.Matches(rendered, "Triggers", nameof(an_event_dispatched_with_a_payload_is_heard_with_that_payload));
    }
}
