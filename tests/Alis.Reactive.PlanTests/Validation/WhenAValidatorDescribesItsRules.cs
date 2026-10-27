using Alis.Reactive.FluentValidator;
using Alis.Reactive.PlanTests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.PlanTests.Validation;

/// <summary>A validator's rules are written once; the page carries the checks the browser can make, bound to the inputs.</summary>
[TestFixture]
public class WhenAValidatorDescribesItsRules
{
    private static IServiceProvider Services<TValidator>() where TValidator : ReactiveValidator =>
        new ServiceCollection().AddReactiveFluentValidation(validation => validation.Add<TValidator>()).BuildServiceProvider();

    [Test]
    public void a_check_the_browser_can_make_lands_on_the_input_that_renders_the_member()
    {
        var plan = Page.Plan<Admission>().TextInput(m => m.Name);
        plan.Triggers().DomReady(p => p.Post("/admit").Gather(g => g.Form()).Validate<AdmissionRules>("admission"));

        var rendered = plan.Rendered(Services<AdmissionRules>());

        var form = rendered.Objects()["$form:admission"]!["data"]!;
        var field = form["fields"]!.AsArray().Single()!;
        Assert.That(field["name"]!.GetValue<string>(), Is.EqualTo("Name"));
        Assert.That(field["tests"]!.AsArray().Single()!["rule"]!.GetValue<string>(), Is.EqualTo("required"));
        Assert.That(field["tests"]!.AsArray().Single()!["message"]!.GetValue<string>(), Is.EqualTo("Name the resident."));
        Golden.Matches(rendered, "Validation", nameof(a_check_the_browser_can_make_lands_on_the_input_that_renders_the_member));
    }

    [Test]
    public void a_rule_under_a_condition_the_server_alone_decides_stays_with_the_server()
    {
        var plan = Page.Plan<Admission>().TextInput(m => m.Name).TextInput(m => m.Age);
        plan.Triggers().DomReady(p => p.Post("/admit").Gather(g => g.Form()).Validate<AdmissionRules>("admission"));

        var rendered = plan.Rendered(Services<AdmissionRules>());

        var names = rendered.Objects()["$form:admission"]!["data"]!["fields"]!.AsArray().Select(field => field!["name"]!.GetValue<string>());
        Assert.That(names, Is.EqualTo(new[] { "Name" }), "the age's rule is the server's, so the page carries no check for it");
        Golden.Matches(rendered, "Validation", nameof(a_rule_under_a_condition_the_server_alone_decides_stays_with_the_server));
    }
}
