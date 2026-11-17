using System;
using System.Text.Json.Serialization;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.TriggersAndPipelines;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenATemplateRaisesAnAppEvent
{
    [Test]
    public void the_element_is_marked_with_the_events_name_and_the_member_holding_the_templates_text()
    {
        var html = Views.Of(new CareCall("North"));
        var plan = html.ReactivePlan<CareCall>();
        var marked = plan.Raises(CareEvents.Flagged, p => p.WardName, "${alisFusionText(data.ward)}");
        html.RenderPlan(plan);
        Assert.That(marked, Is.EqualTo("data-alis-raise=\"flagged\" data-alis-detail-ward-name=\"${alisFusionText(data.ward)}\""));
    }

    [Test]
    public void a_member_that_carries_its_json_name_is_named_by_it_and_a_quote_in_the_text_is_encoded()
    {
        var html = Views.Of(new CareCall("North"));
        var plan = html.ReactivePlan<CareCall>();
        var marked = plan.Raises(TemplateEvents.Reviewed, p => p.ResidentId, "${if(bed)}\"${/if}");
        html.RenderPlan(plan);
        Assert.That(marked, Is.EqualTo("data-alis-raise=\"reviewed\" data-alis-detail-resident=\"${if(bed)}&quot;${/if}\""));
    }

    [Test]
    public void a_member_past_another_member_is_refused()
    {
        var html = Views.Of(new CareCall("North"));
        var plan = html.ReactivePlan<CareCall>();
        Action theView = () =>
        {
            plan.Raises(TemplateEvents.Reviewed, p => p.Bed.Room, "${alisFusionText(data.room)}");
            html.RenderPlan(plan);
        };
        Assert.That(theView, Throws.InvalidOperationException.With.Message.EqualTo(
            "'p => p.Bed.Room' is not one member of the payload 'ResidentReviewed', and markup carries each member of the payload it raises as the text of one "
            + "attribute, so a template fills one direct member of it, such as p => p.ResidentId."));
    }

    [Test]
    public void a_member_that_holds_a_list_is_refused_naming_a_member_of_text()
    {
        var html = Views.Of(new CareCall("North"));
        var plan = html.ReactivePlan<CareCall>();
        Action theView = () =>
        {
            plan.Raises(CareEvents.RoundStaffed, p => p.Carers, "${alisFusionText(data.carers)}");
            html.RenderPlan(plan);
        };
        Assert.That(theView, Throws.InvalidOperationException.With.Message.EqualTo(
            "'RoundOfCarers.Carers' is a List<RoundCarer>, and markup carries each member of the payload it raises as the text of one "
            + "attribute, so a member it carries is text, a number, a bool, a date, a Guid or an enum. "
            + "Fill a member that is text, a number, a bool, a date, a Guid or an enum."));
    }

    [Test]
    public void a_member_named_so_no_attribute_carries_it_is_refused_naming_json_property_name()
    {
        var html = Views.Of(new CareCall("North"));
        var plan = html.ReactivePlan<CareCall>();
        Action theView = () =>
        {
            plan.Raises(CareEvents.RoundSigned, p => p.Carer, "${alisFusionText(data.carer)}");
            html.RenderPlan(plan);
        };
        Assert.That(theView, Throws.InvalidOperationException.With.Message.EqualTo(
            "'SignedRound.Carer' is named 'SignedBy', and markup carries each member of the payload it raises as an "
            + "attribute whose name the browser lowercases, so a name it carries is camel case: a lowercase first letter, then letters, digits or underscores. "
            + "Name it in camel case with JsonPropertyName."));
    }
}

public sealed class ResidentReviewed
{
    [JsonPropertyName("resident")]
    public string ResidentId { get; set; } = "";

    public ReviewedBed Bed { get; set; } = new(0);
}

public sealed record ReviewedBed(int Room);

public static class TemplateEvents
{
    public static readonly AppEvent<ResidentReviewed> Reviewed = AppEvent.Declare<ResidentReviewed>();
}
