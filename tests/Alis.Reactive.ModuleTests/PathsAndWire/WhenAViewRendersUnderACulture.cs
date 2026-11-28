using Alis.Reactive.RenderingAndDelivery;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.PathsAndWire;

[TestFixture]
public sealed class WhenAViewRendersUnderACulture
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>{\"version\":7,\"planId\":\"Alis.Reactive.ModuleTests.PathsAndWire.Dose\",";
    private const string ScriptClose = "\"timeZone\":{\"kind\":\"clock\"},\"trace\":{\"kind\":\"off\"},\"vendors\":{},\"objects\":{},\"handlers\":[]}</script>";
    private const string TheHostsNumberText = "\"integer\":{\"group\":false,\"exponent\":false},\"fraction\":{\"group\":true,\"exponent\":true}},";

    [Test]
    [SetCulture("de-DE")]
    public void the_plan_carries_the_views_culture_with_its_own_separators()
    {
        var html = Views.Of(new Dose(0.125m));
        var plan = html.ReactivePlan<Dose>();

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(
            ScriptOpen + "\"culture\":{\"name\":\"de-DE\",\"collation\":\"de-DE\",\"decimal\":\",\",\"group\":\".\"," + TheHostsNumberText + ScriptClose));
    }

    [Test]
    [SetCulture("")]
    public void the_invariant_culture_orders_text_as_english()
    {
        var html = Views.Of(new Dose(0.125m));
        var plan = html.ReactivePlan<Dose>();

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(
            ScriptOpen + "\"culture\":{\"name\":\"\",\"collation\":\"en\",\"decimal\":\".\",\"group\":\",\"," + TheHostsNumberText + ScriptClose));
    }
}

public sealed record Dose(decimal Milligrams);
