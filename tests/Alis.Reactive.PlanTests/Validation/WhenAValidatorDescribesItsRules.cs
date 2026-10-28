using Alis.Reactive.PlanTests.Support;

namespace Alis.Reactive.PlanTests.Validation;

/// <summary>A validator's rules are written once; the page carries the checks the browser can make, bound to the inputs.</summary>
[TestFixture]
public class WhenAValidatorDescribesItsRules
{
    [Test]
    public void a_check_the_browser_can_make_lands_on_the_input_that_renders_the_member()
    {
        var plan = Page.Plan<Admission>().TextInput(m => m.Name);
        plan.Triggers().DomReady(p => p.Post("/admit").Gather(g => g.Form()).Validate<AdmissionRules>("admission"));

        var rendered = plan.Rendered(Registered.Validator<AdmissionRules>());

        var form = rendered.Objects()["$form:admission"]!["data"]!;
        var field = form["fields"]!.AsArray().Single(each => each!["name"]!.GetValue<string>() == "Name")!;
        Assert.That(field["tests"]!.AsArray().Single()!["rule"]!.GetValue<string>(), Is.EqualTo("required"));
        Assert.That(field["tests"]!.AsArray().Single()!["message"]!.GetValue<string>(), Is.EqualTo("Name the resident."));
        Golden.Matches(rendered, "Validation", nameof(a_check_the_browser_can_make_lands_on_the_input_that_renders_the_member));
    }

    [Test]
    public void a_rule_under_a_condition_the_server_alone_decides_stays_with_the_server()
    {
        var plan = Page.Plan<Admission>().TextInput(m => m.Name).TextInput(m => m.Age);
        plan.Triggers().DomReady(p => p.Post("/admit").Gather(g => g.Form()).Validate<AdmissionRules>("admission"));

        var rendered = plan.Rendered(Registered.Validator<AdmissionRules>());

        var fields = rendered.Objects()["$form:admission"]!["data"]!["fields"]!.AsArray()
            .ToDictionary(field => field!["name"]!.GetValue<string>(), field => field!["tests"]!.AsArray().Count);
        Assert.That(fields["Name"], Is.EqualTo(1));
        Assert.That(fields["Age"], Is.Zero, "the age's rule is the server's, so the page carries its field and no check for it");
        Golden.Matches(rendered, "Validation", nameof(a_rule_under_a_condition_the_server_alone_decides_stays_with_the_server));
    }

    [Test]
    public void two_requests_that_validate_one_form_describe_it_once()
    {
        var plan = Page.Plan<Admission>().TextInput(m => m.Name);
        plan.Triggers()
            .DomReady(p => p.Post("/save").Gather(g => g.Form()).Validate<AdmissionRules>("admission"))
            .DomReady(p => p.Post("/close").Gather(g => g.Form()).Validate<AdmissionRules>("admission"));

        var rendered = plan.Rendered(Registered.Validator<AdmissionRules>());

        Assert.That(rendered.Handlers().Count, Is.EqualTo(2));
        var names = rendered.Objects()["$form:admission"]!["data"]!["fields"]!.AsArray().Select(field => field!["name"]!.GetValue<string>());
        Assert.That(names, Is.EqualTo(new[] { "Name", "Age" }), "each member a rule is written on, once");
        Golden.Matches(rendered, "Validation", nameof(two_requests_that_validate_one_form_describe_it_once));
    }
}
