using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Html;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.InputsAndIds;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAnInputUnderAListIndexIsLeftOutWhileItIsBlank
{
    [Test]
    public void a_date_at_a_list_position_is_left_out_unless_alone_and_a_date_outside_every_list_is_left_out()
    {
        var html = Views.Of(new RoundSheet());
        var plan = html.ReactivePlan<RoundSheet>();
        html.InputField(plan, m => m.Dates[0]).Render<IntakeBox>(new HtmlString("<input>"));
        html.InputField(plan, m => m.Due).Render<IntakeBox>(new HtmlString("<input>"));
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(WhenBlankOf(json, "Dates[0]"), Is.EqualTo("""{"kind":"lit","value":"leaveUnlessAlone"}"""));
        Assert.That(WhenBlankOf(json, "Due"), Is.EqualTo("""{"kind":"lit","value":"leave"}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_date_and_a_link_inside_a_list_item_are_left_out_unless_alone_and_a_text_beside_them_is_sent()
    {
        var html = Views.Of(new RoundSheet());
        var plan = html.ReactivePlan<RoundSheet>();
        html.InputField(plan, m => m.Visits[0].When).Render<IntakeBox>(new HtmlString("<input>"));
        html.InputField(plan, m => m.Visits[0].Link).Render<IntakeBox>(new HtmlString("<input>"));
        html.InputField(plan, m => m.Visits[0].Note).Render<IntakeBox>(new HtmlString("<input>"));
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(WhenBlankOf(json, "Visits[0].When"), Is.EqualTo("""{"kind":"lit","value":"leaveUnlessAlone"}"""));
        Assert.That(WhenBlankOf(json, "Visits[0].Link"), Is.EqualTo("""{"kind":"lit","value":"leaveUnlessAlone"}"""));
        Assert.That(WhenBlankOf(json, "Visits[0].Note"), Is.EqualTo("""{"kind":"lit","value":"send"}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_link_outside_every_list_is_sent_as_the_nothing_its_member_leaves_out()
    {
        var html = Views.Of(new RoundSheet());
        var plan = html.ReactivePlan<RoundSheet>();
        html.InputField(plan, m => m.Link).Render<IntakeBox>(new HtmlString("<input>"));
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(WhenBlankOf(json, "Link"), Is.EqualTo("""{"kind":"lit","value":"send"}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    private static string WhenBlankOf(string json, string name)
    {
        using var document = JsonDocument.Parse(json);
        var input = document.RootElement.GetProperty("objects").GetProperty("$model").GetProperty("data").GetProperty("inputs").EnumerateObject()
            .Single(entry => entry.Value.GetProperty("name").GetString() == name).Value;
        return input.GetProperty("sent").GetProperty("fields").EnumerateArray().Single(field => field.GetProperty("name").GetString() == "whenBlank").GetProperty("value").GetRawText();
    }
}

public sealed class RoundSheet
{
    public DateTime Due { get; set; } = new(2026, 12, 1);

    public Uri? Link { get; set; }

    public List<DateTime> Dates { get; set; } = [new(2026, 11, 30)];

    public List<RoundSheetVisit> Visits { get; set; } = [new()];
}

public sealed class RoundSheetVisit
{
    public DateTime When { get; set; }

    public Uri? Link { get; set; }

    public string? Note { get; set; }
}
