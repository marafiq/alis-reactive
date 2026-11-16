using System;
using System.Text.Json;
using Alis.Reactive.RenderingAndDelivery;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.TriggersAndPipelines;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenMarkupRaisesAnAppEvent
{
    [Test]
    public void an_event_with_no_payload_marks_the_element_with_its_name_alone()
    {
        var html = Views.Of(new CareCall("North"));
        var plan = html.ReactivePlan<CareCall>();
        var marked = Views.TextOf(html.Raises(plan, CareEvents.RoundStarted));
        html.RenderPlan(plan);
        Assert.That(marked, Is.EqualTo("data-alis-raise=\"roundStarted\""));
    }

    [Test]
    public void an_event_that_declares_a_payload_raised_with_none_marks_the_element_with_its_name_alone()
    {
        var html = Views.Of(new CareCall("North"));
        var plan = html.ReactivePlan<CareCall>();
        var marked = Views.TextOf(html.Raises(plan, CareEvents.Escalated));
        html.RenderPlan(plan);
        Assert.That(marked, Is.EqualTo("data-alis-raise=\"care-escalated\""));
    }

    [Test]
    public void each_member_is_an_attribute_holding_its_text_as_the_live_form_writes_it()
    {
        var html = Views.Of(new CareCall("North"));
        var plan = html.ReactivePlan<CareCall>();
        var payload = new Escalation { Paged = true, Bed = 4, Due = new DateTime(2026, 11, 15), Level = Severity.Urgent, Dose = 2.5m, Note = "Late" };
        var marked = Views.TextOf(html.Raises(plan, CareEvents.Escalated, payload));
        html.RenderPlan(plan);
        Assert.That(marked, Is.EqualTo(
            "data-alis-raise=\"care-escalated\" data-alis-detail-paged=\"true\" data-alis-detail-bed=\"4\" " +
            "data-alis-detail-due=\"2026-11-15T00:00:00\" data-alis-detail-level=\"1\" data-alis-detail-dose=\"2.5\" data-alis-detail-note=\"Late\" data-alis-null=\"\""));
    }

    [Test]
    public void a_member_that_holds_null_has_no_attribute_and_is_named_in_data_alis_null()
    {
        var html = Views.Of(new CareCall("North"));
        var plan = html.ReactivePlan<CareCall>();
        var payload = new Escalation { Paged = false, Bed = null, Due = new DateTime(2026, 11, 15, 7, 30, 0), Level = Severity.Routine, Note = null };
        var marked = Views.TextOf(html.Raises(plan, CareEvents.Escalated, payload));
        html.RenderPlan(plan);
        Assert.That(marked, Is.EqualTo(
            "data-alis-raise=\"care-escalated\" data-alis-detail-paged=\"false\" data-alis-detail-due=\"2026-11-15T07:30:00\" " +
            "data-alis-detail-level=\"0\" data-alis-detail-dose=\"0\" data-alis-null=\"bed note\""));
    }

    [Test]
    public void a_members_text_is_encoded_and_each_capital_of_its_name_is_a_hyphen_and_its_letter()
    {
        var html = Views.Of(new CareCall("North"));
        var plan = html.ReactivePlan<CareCall>();
        var payload = new ResidentFlagged { Room = 3, Reason = "\"fall\" & <bruise>", WardName = "East" };
        var marked = Views.TextOf(html.Raises(plan, CareEvents.Flagged, payload));
        html.RenderPlan(plan);
        Assert.That(marked, Is.EqualTo(
            "data-alis-raise=\"flagged\" data-alis-detail-room=\"3\" data-alis-detail-reason=\"&quot;fall&quot; &amp; &lt;bruise&gt;\" " +
            "data-alis-detail-ward-name=\"East\" data-alis-null=\"\""));
    }

    [Test]
    public void each_member_is_named_as_the_apps_json_names_it()
    {
        var html = Views.Of(new CareCall("North"), json => json.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        var plan = html.ReactivePlan<CareCall>();
        var payload = new ResidentFlagged { Room = 3, Reason = "fall", WardName = "East" };
        var marked = Views.TextOf(html.Raises(plan, CareEvents.Flagged, payload));
        html.RenderPlan(plan);
        Assert.That(marked, Is.EqualTo(
            "data-alis-raise=\"flagged\" data-alis-detail-room=\"3\" data-alis-detail-reason=\"fall\" data-alis-detail-ward_name=\"East\" data-alis-null=\"\""));
    }

    [Test]
    public void a_payload_of_null_marks_the_element_as_raised_with_none()
    {
        var html = Views.Of(new CareCall("North"));
        var plan = html.ReactivePlan<CareCall>();
        var marked = Views.TextOf(html.Raises(plan, CareEvents.Flagged, null!));
        html.RenderPlan(plan);
        Assert.That(marked, Is.EqualTo("data-alis-raise=\"flagged\""));
    }

    [Test]
    public void a_payload_of_null_whose_type_holds_a_list_is_refused_naming_the_member_and_dispatch()
    {
        var html = Views.Of(new CareCall("North"));
        var plan = html.ReactivePlan<CareCall>();

        Action theView = () =>
        {
            html.Raises(plan, CareEvents.RoundStaffed, null!);
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'RoundOfCarers.Carers' is a List<RoundCarer>, and markup carries each member of the payload it raises as the text of one attribute, "
                + "so a member it carries is text, a number, a bool, a date, a Guid or an enum. "
                + "Raise a payload that holds a record or a list from a pipeline, with p.Dispatch."));
    }

    [Test]
    public void a_payload_of_null_whose_type_names_a_member_so_no_attribute_carries_it_back_is_refused_naming_the_member_its_name_and_dispatch()
    {
        var html = Views.Of(new CareCall("North"));
        var plan = html.ReactivePlan<CareCall>();

        Action theView = () =>
        {
            html.Raises(plan, CareEvents.RoundSigned, null!);
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'SignedRound.Carer' is named 'SignedBy', and markup carries each member of the payload it raises as an attribute whose name the browser lowercases, "
                + "so a name it carries is camel case: a lowercase first letter, then letters, digits or underscores. "
                + "Rename it, or raise the payload from a pipeline, with p.Dispatch."));
    }
}
