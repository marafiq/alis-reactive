using System;
using Alis.Reactive.RenderingAndDelivery;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.PathsAndWire;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAViewRendersUnderATimeZone
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";

    private const string AVisitPlanUnderEnglish =
        "{\"version\":7,\"planId\":\"Alis.Reactive.ModuleTests.PathsAndWire.Visit\"," +
        "\"culture\":{\"name\":\"en-US\",\"collation\":\"en-US\",\"decimal\":\".\",\"group\":\",\"," +
        "\"integer\":{\"group\":false,\"exponent\":false},\"fraction\":{\"group\":true,\"exponent\":true}},";

    private const string WithNothingWritten = ",\"trace\":{\"kind\":\"off\"},\"vendors\":{},\"objects\":{},\"handlers\":[]}";

    private const string UnderNewYork = AVisitPlanUnderEnglish + "\"timeZone\":{\"kind\":\"zone\",\"id\":\"America/New_York\"}" + WithNothingWritten;

    private const string UnderKolkata = AVisitPlanUnderEnglish + "\"timeZone\":{\"kind\":\"zone\",\"id\":\"Asia/Kolkata\"}" + WithNothingWritten;

    private const string UnderCalcutta = AVisitPlanUnderEnglish + "\"timeZone\":{\"kind\":\"zone\",\"id\":\"Asia/Calcutta\"}" + WithNothingWritten;

    private const string UnderTheBrowsersClock = AVisitPlanUnderEnglish + "\"timeZone\":{\"kind\":\"clock\"}" + WithNothingWritten;

    [Test]
    public void the_plan_carries_the_zone_its_request_renders_under()
        => Assert.That(RenderedUnder(TimeZoneInfo.FindSystemTimeZoneById("America/New_York")), Is.EqualTo(ScriptOpen + UnderNewYork + ScriptClose));

    [Test]
    public void a_request_under_another_zone_writes_its_own()
        => Assert.That(RenderedUnder(TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata")), Is.EqualTo(ScriptOpen + UnderKolkata + ScriptClose));

    [Test]
    public void a_zone_found_by_its_windows_id_is_named_as_net_converts_it()
        => Assert.That(RenderedUnder(TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")), Is.EqualTo(ScriptOpen + UnderCalcutta + ScriptClose));

    [Test]
    public void a_request_that_chooses_none_renders_under_the_browsers_clock()
    {
        RenderedUnder(TimeZoneInfo.FindSystemTimeZoneById("America/New_York"));
        var html = Views.Of(new Visit("Ada Hart"));
        var plan = html.ReactivePlan<Visit>();

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + UnderTheBrowsersClock + ScriptClose));
    }

    [Test]
    public void a_zone_net_converts_to_no_iana_id_is_refused_naming_it()
    {
        var html = Views.Of(new Visit("Ada Hart"));
        html.ViewContext.HttpContext.RendersUnder(TimeZoneInfo.CreateCustomTimeZone("Ward clock", TimeSpan.FromMinutes(330), "Ward clock", "Ward clock"));

        Assert.That(() => html.ReactivePlan<Visit>(), Throws.InvalidOperationException.With.Message.EqualTo(
            "This request renders under the time zone 'Ward clock', which .NET converts to no IANA id on this host, so the plan could not name it to the browser; " +
            "choose a time zone TimeZoneInfo.FindSystemTimeZoneById finds."));
    }

    [Test]
    public void the_plan_under_a_zone_is_valid_by_the_plan_schema()
    {
        Assert.That(PlanSchema.ProblemsOf(UnderNewYork), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(UnderKolkata), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(UnderCalcutta), Is.Empty);
    }

    private static string RenderedUnder(TimeZoneInfo zone)
    {
        var html = Views.Of(new Visit("Ada Hart"));
        html.ViewContext.HttpContext.RendersUnder(zone);
        var plan = html.ReactivePlan<Visit>();
        return Views.TextOf(html.RenderPlan(plan));
    }
}

public sealed record Visit(string Resident);
