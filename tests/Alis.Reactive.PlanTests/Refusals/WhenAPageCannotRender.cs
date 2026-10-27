using Alis.Reactive.Builders;
using Alis.Reactive.Components;
using Alis.Reactive.InputField;
using Alis.Reactive.PlanModel;
using Alis.Reactive.PlanTests.Support;

namespace Alis.Reactive.PlanTests.Refusals;

/// <summary>A developer's mistake is refused where the view renders, naming what to change; nothing reaches the browser.</summary>
[TestFixture]
public class WhenAPageCannotRender
{
    [Test]
    public void a_member_gathered_twice_is_refused_with_its_name()
    {
        var plan = Page.Plan<Admission>();

        var refused = Assert.Throws<InvalidOperationException>(() =>
            plan.Triggers().DomReady(p => p.Get("/residents").Gather<ResidentQuery>(g => g.Set(x => x.Wing, "Garden").Set(x => x.Wing, "Maple"))));

        Assert.That(refused!.Message, Does.Contain("'Wing' is gathered twice"));
    }

    [Test]
    public void a_dispatch_that_sets_no_field_of_its_payload_is_refused()
    {
        var plan = Page.Plan<Admission>();

        var refused = Assert.Throws<InvalidOperationException>(() =>
            plan.Triggers().DomReady(p => p.Dispatch(AdmissionEvents.RoomChecked, payload => { })));

        Assert.That(refused!.Message, Does.Contain("at least one field"));
    }

    [Test]
    public void form_data_with_nothing_gathered_is_refused_at_render()
    {
        var plan = Page.Plan<Admission>();
        plan.Triggers().DomReady(p => p.Post("/admit").AsFormData());

        var refused = Assert.Throws<InvalidOperationException>(() => plan.Render());

        Assert.That(refused!.Message, Does.Contain("gathers nothing"));
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
    public void a_member_rendered_twice_with_two_kinds_of_input_is_refused()
    {
        var plan = Page.Plan<Admission>().TextInput(m => m.Age);

        var refused = Assert.Throws<InvalidOperationException>(() =>
            plan.AddRenderedInput(RenderedInput.Of(
                InputIdentity.For<Admission, int>(m => m.Age, "Age", HtmlFieldPrefix.None), new NumberInput())));

        Assert.That(refused!.Message, Does.Contain("rendered twice with different components"));
    }

    [Test]
    public void a_step_written_after_its_callback_returned_is_refused()
    {
        var plan = Page.Plan<Admission>();
        PipelineBuilder<Admission>? kept = null;
        plan.Triggers().DomReady(p => kept = p);

        var refused = Assert.Throws<InvalidOperationException>(() => kept!.Element("status").Text.Set("late"));

        Assert.That(refused!.Message, Does.Contain("outside any callback"));
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

    [Test]
    public void a_plugin_registered_twice_is_refused_by_name()
    {
        var plan = Page.Plan<Admission>();
        plan.RegisterPlugin<Clock>();

        var refused = Assert.Throws<InvalidOperationException>(() => plan.RegisterPlugin<Clock>());

        Assert.That(refused!.Message, Does.Contain("'clock' is already registered"));
    }
}
