using Alis.Reactive.PlanTests.Support;

namespace Alis.Reactive.PlanTests.Refusals;

/// <summary>A request that could not reach its endpoint as written is refused where the view renders.</summary>
[TestFixture]
public class WhenARequestCannotBeSent
{
    [Test]
    public void a_route_placeholder_no_gathered_member_fills_is_refused_naming_the_fill()
    {
        var plan = Page.Plan<Admission>();
        plan.Triggers().DomReady(p => p.Get("/residents/{id}").Gather<ResidentQuery>(g => g.Set(x => x.Wing, "Garden")));

        var refused = Assert.Throws<InvalidOperationException>(() => plan.Serialize());

        Assert.That(refused!.Message, Does.Contain("placeholder '{id}'"));
        Assert.That(refused.Message, Does.Contain("g.Set(x => x.Id, ..)"));
    }

    [Test]
    public void a_route_placeholder_filled_by_a_list_is_refused()
    {
        var plan = Page.Plan<Admission>();
        plan.Triggers().DomReady(p => p.Get("/residents/{Tags}").Gather<Admission>(g => g.Set(x => x.Tags, new List<string> { "fall-risk" })));

        var refused = Assert.Throws<InvalidOperationException>(() => plan.Serialize());

        Assert.That(refused!.Message, Does.Contain("needs a scalar value"));
        Assert.That(refused.Message, Does.Contain("array<string>"));
    }

    [Test]
    public void a_form_field_posted_to_a_request_without_that_member_is_refused()
    {
        var plan = Page.Plan<Admission>().TextInput(m => m.Name);

        var refused = Assert.Throws<InvalidOperationException>(() =>
            plan.Triggers().DomReady(p => p.Post("/residents").Gather<ResidentQuery>(g => g.Form(m => m.Name))));

        Assert.That(refused!.Message, Does.Contain("ResidentQuery"));
        Assert.That(refused.Message, Does.Contain("has no member 'Name'"));
    }

    [Test]
    public void a_form_field_of_another_type_than_the_request_member_is_refused()
    {
        var plan = Page.Plan<Admission>().TextInput(m => m.Age);

        var refused = Assert.Throws<InvalidOperationException>(() =>
            plan.Triggers().DomReady(p => p.Post("/admit").Gather<AdmissionAsText>(g => g.Form(m => m.Age))));

        Assert.That(refused!.Message, Does.Contain("'Age' as Int32"));
        Assert.That(refused.Message, Does.Contain("AdmissionAsText.Age"));
    }
}
