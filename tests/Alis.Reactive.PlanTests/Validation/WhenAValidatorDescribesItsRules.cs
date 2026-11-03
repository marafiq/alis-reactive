using System.Text.Json.Nodes;
using Alis.Reactive.PlanTests.Support;

namespace Alis.Reactive.PlanTests.Validation;

/// <summary>A validator's rules are written once; the page carries the checks the browser can make, bound to the inputs.</summary>
[TestFixture]
public class WhenAValidatorDescribesItsRules
{
    [Test]
    public void a_check_the_browser_can_make_lands_on_the_input_that_renders_the_member()
    {
        var (html, plan) = Page.Of<Admission>(Registered.Validator<AdmissionRules>());
        html.TextInput(plan, m => m.Name);
        html.On(plan, t => t.DomReady(p => p.Post("/admit").Gather(g => g.Form()).Validate<AdmissionRules>("admission")));

        var rendered = html.Rendered(plan);

        var form = rendered.Objects()["$form:admission"]!["data"]!;
        var field = form["fields"]!.AsArray().Single(each => each!["name"]!.GetValue<string>() == "Name")!;
        Assert.That(field["tests"]!.AsArray().Single()!["message"]!.GetValue<string>(), Is.EqualTo("Name the resident."));
        Golden.Matches(rendered, "Validation", nameof(a_check_the_browser_can_make_lands_on_the_input_that_renders_the_member));
    }

    [Test]
    public void a_rule_under_a_condition_the_server_alone_decides_stays_with_the_server()
    {
        var (html, plan) = Page.Of<Admission>(Registered.Validator<AdmissionRules>());
        html.TextInput(plan, m => m.Name);
        html.TextInput(plan, m => m.Age);
        html.On(plan, t => t.DomReady(p => p.Post("/admit").Gather(g => g.Form()).Validate<AdmissionRules>("admission")));

        var rendered = html.Rendered(plan);

        var fields = rendered.Objects()["$form:admission"]!["data"]!["fields"]!.AsArray()
            .ToDictionary(field => field!["name"]!.GetValue<string>(), field => field!["tests"]!.AsArray().Count);
        Assert.That(fields["Name"], Is.EqualTo(1));
        Assert.That(fields["Age"], Is.Zero, "the age's rule is the server's, so the page carries its field and no check for it");
        Golden.Matches(rendered, "Validation", nameof(a_rule_under_a_condition_the_server_alone_decides_stays_with_the_server));
    }

    [Test]
    public void two_requests_that_validate_one_form_describe_it_once()
    {
        var (html, plan) = Page.Of<Admission>(Registered.Validator<AdmissionRules>());
        html.TextInput(plan, m => m.Name);
        html.On(plan, t => t
            .DomReady(p => p.Post("/save").Gather(g => g.Form()).Validate<AdmissionRules>("admission"))
            .DomReady(p => p.Post("/close").Gather(g => g.Form()).Validate<AdmissionRules>("admission")));

        var rendered = html.Rendered(plan);

        Assert.That(rendered.Handlers().Count, Is.EqualTo(2));
        var names = rendered.Objects()["$form:admission"]!["data"]!["fields"]!.AsArray().Select(field => field!["name"]!.GetValue<string>());
        Assert.That(names, Is.EqualTo(new[] { "Name", "Age" }), "each member a rule is written on, once");
        Golden.Matches(rendered, "Validation", nameof(two_requests_that_validate_one_form_describe_it_once));
    }

    [Test]
    public void an_enum_check_is_the_pattern_of_every_value_the_server_binds_with_its_default_message()
    {
        var (html, plan) = Page.Of<Admission>(Registered.Validator<CareLevelRules>());
        html.TextInput(plan, m => m.Level);
        html.On(plan, t => t.DomReady(p => p.Post("/admit").Gather(g => g.Form()).Validate<CareLevelRules>("admission")));

        var rendered = html.Rendered(plan);

        var field = rendered.Objects()["$form:admission"]!["data"]!["fields"]!.AsArray().Single(each => each!["name"]!.GetValue<string>() == "Level")!;
        var test = field["tests"]!.AsArray().Single()!;
        Assert.That(test["message"]!.GetValue<string>(), Is.EqualTo("'Level' is not a known value."));
        var patterns = test["check"]!.Nodes().OfType<JsonObject>()
            .Select(node => node["value"] as JsonValue)
            .Select(value => value != null && value.TryGetValue<string>(out var text) ? text : null)
            .Where(text => text != null && text.StartsWith("^", StringComparison.Ordinal));
        Assert.That(patterns, Is.EqualTo(new[] { @"^\s*(?:Independent|Assisted|Memory|0|1|2)\s*$" }));
        Golden.Matches(rendered, "Validation", nameof(an_enum_check_is_the_pattern_of_every_value_the_server_binds_with_its_default_message));
    }

    [Test]
    public void a_digit_count_is_the_pattern_of_the_number_the_page_binds_which_carries_no_trailing_zero()
    {
        var (html, plan) = Page.Of<Admission>(Registered.Validator<DepositRules>());
        html.TextInput(plan, m => m.Deposit);
        html.On(plan, t => t.DomReady(p => p.Post("/admit").Gather(g => g.Form()).Validate<DepositRules>("admission")));

        var rendered = html.Rendered(plan);

        var field = rendered.Objects()["$form:admission"]!["data"]!["fields"]!.AsArray().Single(each => each!["name"]!.GetValue<string>() == "Deposit")!;
        var test = field["tests"]!.AsArray().Single()!;
        Assert.That(test["message"]!.GetValue<string>(), Is.EqualTo("'Deposit' must not be more than 5 digits in total, with allowance for 2 decimals."));
        var patterns = test["check"]!.Nodes().OfType<JsonObject>()
            .Select(node => node["value"] as JsonValue)
            .Select(value => value != null && value.TryGetValue<string>(out var text) ? text : null)
            .Where(text => text != null && text.StartsWith("^", StringComparison.Ordinal));
        Assert.That(patterns, Is.EqualTo(new[] { @"^-?(?:0|[1-9]\d{0,2})(?:\.\d{1,2})?$" }));
        Golden.Matches(rendered, "Validation", nameof(a_digit_count_is_the_pattern_of_the_number_the_page_binds_which_carries_no_trailing_zero));
    }

    [Test]
    public void a_rule_given_a_name_says_it_in_place_of_the_members_spelling_and_stays_under_the_members_path()
    {
        var (html, plan) = Page.Of<Admission>(Registered.Validator<NamedDepositRules>());
        html.TextInput(plan, m => m.Deposit);
        html.On(plan, t => t.DomReady(p => p.Post("/admit").Gather(g => g.Form()).Validate<NamedDepositRules>("admission")));

        var rendered = html.Rendered(plan);

        var field = rendered.Objects()["$form:admission"]!["data"]!["fields"]!.AsArray().Single(each => each!["name"]!.GetValue<string>() == "Deposit")!;
        Assert.That(field["tests"]!.AsArray().Single()!["message"]!.GetValue<string>(), Is.EqualTo("'Deposit held' must not be more than 5 digits in total, with allowance for 2 decimals."));
        Golden.Matches(rendered, "Validation", nameof(a_rule_given_a_name_says_it_in_place_of_the_members_spelling_and_stays_under_the_members_path));
    }
}
