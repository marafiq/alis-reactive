using Alis.Reactive.PlanTests.Support;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.PlanTests.Partials;

/// <summary>A partial's inputs bind under its prefix; a slot loads a response and unloads what it brought.</summary>
[TestFixture]
public class WhenAPartialComposes
{
    [Test]
    public void a_partial_rendered_for_a_member_binds_its_input_under_the_member_prefix()
    {
        var plan = Page.Partial<Contact>("NextOfKin").TextInput(m => m.Name);
        plan.Triggers().DomReady(p => p.Post("/contact").Gather(g => g.Form(m => m.Name)));

        var rendered = plan.Rendered();

        var input = rendered.Objects()["$model"]!["data"]!["inputs"]!.AsObject().Single();
        Assert.That(input.Value!["name"]!.GetValue<string>(), Is.EqualTo("NextOfKin.Name"));
        Assert.That(input.Key, Does.EndWith("__NextOfKin_Name"));
        Golden.Matches(rendered, "Partials", nameof(a_partial_rendered_for_a_member_binds_its_input_under_the_member_prefix));
    }

    [Test]
    public void a_response_loads_into_a_slot_and_a_click_unloads_it()
    {
        var plan = Page.Plan<Admission>();
        plan.Triggers().DomReady(p => p.Get("/card").Response(r => r.OnSuccess(s => s.Into("drawer"))));
        plan.Render<Button>("close", HtmlString.Empty).Reactive(button => button.Click, p => p.Unload("drawer"));

        var rendered = plan.Rendered();

        var slot = rendered.Objects()["$slot:drawer"]!;
        Assert.That(slot["name"]!.GetValue<string>(), Is.EqualTo("alis.slot"));
        Assert.That(slot["key"]!.GetValue<string>(), Is.EqualTo("drawer"));
        Assert.That(rendered.Handlers().Count, Is.EqualTo(2));
        Golden.Matches(rendered, "Partials", nameof(a_response_loads_into_a_slot_and_a_click_unloads_it));
    }
}
