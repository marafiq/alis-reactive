using Alis.Reactive.PlanTests.Support;

namespace Alis.Reactive.PlanTests.Refusals;

/// <summary>A developer's mistake is refused where the view renders, naming what to change; nothing reaches the browser.</summary>
[TestFixture]
public class WhenAPageCannotRender
{
    [Test]
    public void a_dispatch_that_sets_no_field_of_its_payload_is_refused()
    {
        var plan = Page.Plan<Admission>();

        var refused = Assert.Throws<InvalidOperationException>(() =>
            plan.Triggers().DomReady(p => p.Dispatch(AdmissionEvents.RoomChecked, payload => { })));

        Assert.That(refused!.Message, Does.Contain("at least one field"));
    }

    [Test]
    public void a_condition_the_browser_cannot_read_is_refused_naming_it_and_the_way_out()
    {
        var plan = Page.Plan<Admission>().TextInput(m => m.Age);
        plan.Triggers().DomReady(p => p.Post("/admit").Gather(g => g.Form()).Validate<UnreadableAdmissionRules>("admission"));

        var refused = Assert.Throws<InvalidOperationException>(() => plan.Render(Registered.Validator<UnreadableAdmissionRules>()));

        Assert.That(refused!.Message, Does.Contain("has no browser form"));
        Assert.That(refused.Message, Does.Contain("WhenOnServer"));
    }

    [Test]
    public void a_response_read_outside_the_routes_of_its_request_is_refused()
    {
        var plan = Page.Plan<Admission>();
        plan.Triggers().DomReady(p =>
        {
            Data<AdmissionSaved>? kept = null;
            p.Post("/admit").Response(r => r.OnSuccess<AdmissionSaved>((saved, s) => kept = saved));
            p.Element("status").Text.Set(kept!.Get(x => x.Confirmation));
        });

        var refused = Assert.Throws<InvalidOperationException>(() => plan.Render());

        Assert.That(refused!.Message, Does.Contain("reads a response outside the routes of the request"));
    }
}
