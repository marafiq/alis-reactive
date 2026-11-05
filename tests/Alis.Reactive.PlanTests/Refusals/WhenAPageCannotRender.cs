using Alis.Reactive.PlanTests.Support;

namespace Alis.Reactive.PlanTests.Refusals;

[TestFixture]
public class WhenAPageCannotRender
{
    [Test]
    public void a_dispatch_that_sets_no_field_of_its_payload_is_refused()
    {
        var (html, plan) = Page.Of<Admission>();

        var refused = Assert.Throws<InvalidOperationException>(() =>
            html.On(plan, t => t.DomReady(p => p.Dispatch(AdmissionEvents.RoomChecked, payload => { }))));

        Assert.That(refused!.Message, Does.Contain("at least one field"));
    }

    [Test]
    public void a_condition_the_browser_cannot_read_is_refused_naming_it_and_the_way_out()
    {
        var (html, plan) = Page.Of<Admission>(Registered.Validator<UnreadableAdmissionRules>());
        html.TextInput(plan, m => m.Age);
        html.On(plan, t => t.DomReady(p => p.Post("/admit").Gather(g => g.Form()).Validate<UnreadableAdmissionRules>("admission")));

        var refused = Assert.Throws<InvalidOperationException>(() => html.Rendered(plan));

        Assert.That(refused!.Message, Does.Contain("has no browser form"));
        Assert.That(refused.Message, Does.Contain("WhenOnServer"));
    }

    [Test]
    public void a_response_read_outside_the_routes_of_its_request_is_refused()
    {
        var (html, plan) = Page.Of<Admission>();
        html.On(plan, t => t.DomReady(p =>
        {
            Data<AdmissionSaved>? kept = null;
            p.Post("/admit").Response(r => r.OnSuccess<AdmissionSaved>((saved, s) => kept = saved));
            p.Element("status").Text.Set(kept!.Get(x => x.Confirmation));
        }));

        var refused = Assert.Throws<InvalidOperationException>(() => html.Rendered(plan));

        Assert.That(refused!.Message, Does.Contain("reads a response outside the routes of the request"));
    }
}
