using Alis.Reactive.Components;
using Alis.Reactive.PlanTests.Support;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.PlanTests.Refusals;

/// <summary>A brought component is a declaration of its members on a vendor; a declaration the framework cannot follow is refused naming the rule.</summary>
[TestFixture]
public class WhenAComponentIsDeclaredWrong
{
    [Test]
    public void a_declaration_used_as_a_live_object_is_refused_naming_the_way_in()
    {
        var plan = Page.Plan<Admission>();

        var refused = Assert.Throws<InvalidOperationException>(() =>
            plan.Triggers().DomReady(p => p.When(new Desk().IsFree("214")).Truthy().Then(t => t.Element("status").Text.Set("Free"))));

        Assert.That(refused!.Message, Does.Contain("'Desk' is a declaration, not a live object"));
        Assert.That(refused.Message, Does.Contain("p.Component<Desk>(..)"));
    }

    [Test]
    public void an_event_on_an_object_a_method_returns_is_refused()
    {
        var plan = Page.Plan<Admission>();

        var refused = Assert.Throws<InvalidOperationException>(() =>
            plan.Triggers().DomReady(p => _ = p.Component<Meter>("meter").Read("probe").Settled));

        Assert.That(refused!.Message, Does.Contain("'Reading.Settled' declares an event on an object a method returns"));
    }

    [Test]
    public void a_member_named_by_a_path_is_refused()
    {
        var plan = Page.Plan<Admission>();

        var refused = Assert.Throws<ArgumentException>(() =>
            plan.Triggers().DomReady(p => p.Component<Thermostat>("thermostat").Target.Set(21)));

        Assert.That(refused!.Message, Does.Contain("'Thermostat.Target' is named 'settings.target'"));
        Assert.That(refused.Message, Does.Contain("Object<T>()"));
    }

    [Test]
    public void a_vendor_name_with_a_space_is_refused()
    {
        var refused = Assert.Throws<ArgumentException>(() =>
            ComponentVendor.Define("my gauge", "", "on", "off", VendorEventPayload.Argument));

        Assert.That(refused!.Message, Does.Contain("Vendor name 'my gauge'"));
    }

    [Test]
    public void two_vendors_with_one_name_are_refused_at_render()
    {
        var plan = Page.Plan<Admission>();
        plan.Render<Gauge>("pressure", HtmlString.Empty).Reactive(g => g.Tick, p => p.Element("status").Text.Set("tick"));
        plan.Render<GaugeAlike>("flow", HtmlString.Empty).Reactive(g => g.Tick, p => p.Element("status").Text.Set("tick"));

        var refused = Assert.Throws<InvalidOperationException>(() => plan.Render());

        Assert.That(refused!.Message, Does.Contain("Two different vendors are named 'gauge'"));
    }

    [Test]
    public void an_id_rendered_with_one_vendor_cannot_be_read_as_another()
    {
        var plan = Page.Plan<Admission>();
        plan.Render<Button>("save", HtmlString.Empty).Reactive(b => b.Click, p => p.Element("status").Text.Set("Saved"));

        var refused = Assert.Throws<InvalidOperationException>(() =>
            plan.Triggers().DomReady(p => p.Element("status").Text.Set(p.Component<Gauge>("save").Label)));

        Assert.That(refused!.Message, Does.Contain("registered as vendor 'native'"));
        Assert.That(refused.Message, Does.Contain("re-referenced as 'gauge'"));
    }
}
