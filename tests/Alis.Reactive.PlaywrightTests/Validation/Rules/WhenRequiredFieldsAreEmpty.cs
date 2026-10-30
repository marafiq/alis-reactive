using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Validation.Rules;

[TestFixture]
public class WhenRequiredFieldsAreEmpty : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/AllRules";

    private ILocator AllRulesBtn => Page.Locator("#validate-all-btn");
    private ILocator AllRulesResult => Page.Locator("#all-rules-result");
    private ILocator Input(Expression<Func<ValidationShowcaseModel, object?>> member) => Page.Locator($"#{IdGenerator.For(member)}");
    private ILocator ErrorFor(Expression<Func<ValidationShowcaseModel, object?>> member) => Page.Locator($"#{IdGenerator.For(member)}_error");

    private ILocator ConditionalBtn => Page.Locator("#conditional-validate-btn");
    private ILocator ConditionalResult => Page.Locator("#conditional-result");
    private ILocator ServerBtn => Page.Locator("#server-save-btn");
    private ILocator HiddenBtn => Page.Locator("#hidden-validate-btn");
    private ILocator DbBtn => Page.Locator("#db-save-btn");

    [Test]
    public async Task empty_form_shows_required_errors()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(AllRulesBtn);

        await Expect(ErrorFor(m => m.AllRules.Name)).ToHaveTextAsync("Name is required.");
        await Expect(ErrorFor(m => m.AllRules.Email)).ToHaveTextAsync("Email is required.");
        await Expect(Input(m => m.AllRules.Name)).ToHaveClassAsync(new Regex("alis-has-error"));
        await Expect(AllRulesResult).ToHaveTextAsync("Click to validate");

        AssertNoConsoleErrors();
    }

    // Seven characters are one short; eight are the minimum.
    [Test]
    public async Task password_minlength_validates_on_blur()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(AllRulesBtn);

        await Input(m => m.AllRules.Password).FillAsync("abcdefg");
        await Input(m => m.AllRules.Password).BlurAsync();

        await Expect(ErrorFor(m => m.AllRules.Password)).ToHaveTextAsync("Password must be at least 8 characters.", new() { Timeout = 2000 });

        await Input(m => m.AllRules.Password).FillAsync("abcdefgh");
        await Input(m => m.AllRules.Password).BlurAsync();

        await Expect(ErrorFor(m => m.AllRules.Password)).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task valid_name_clears_error_on_blur()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(AllRulesBtn);
        await Expect(ErrorFor(m => m.AllRules.Name)).ToHaveTextAsync("Name is required.");

        await Input(m => m.AllRules.Name).FillAsync("Margaret Thompson");
        await Input(m => m.AllRules.Name).BlurAsync();

        await Expect(ErrorFor(m => m.AllRules.Name)).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task email_validates_format()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(AllRulesBtn);
        await Expect(ErrorFor(m => m.AllRules.Email)).ToHaveTextAsync("Email is required.");

        await Input(m => m.AllRules.Email).FillAsync("notanemail");
        await Input(m => m.AllRules.Email).BlurAsync();

        await Expect(ErrorFor(m => m.AllRules.Email)).ToHaveTextAsync("Email must be a valid email address.", new() { Timeout = 2000 });

        await Input(m => m.AllRules.Email).FillAsync("margaret@care.com");
        await Input(m => m.AllRules.Email).BlurAsync();

        await Expect(ErrorFor(m => m.AllRules.Email)).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task age_range_validates()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(AllRulesBtn);

        await Input(m => m.AllRules.Age).FillAsync("121");
        await Input(m => m.AllRules.Age).BlurAsync();

        await Expect(ErrorFor(m => m.AllRules.Age)).ToHaveTextAsync("Age must be between 0 and 120.", new() { Timeout = 2000 });

        await Input(m => m.AllRules.Age).FillAsync("120");
        await Input(m => m.AllRules.Age).BlurAsync();

        await Expect(ErrorFor(m => m.AllRules.Age)).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task conditional_rule_skipped_when_unchecked()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(ConditionalBtn);

        await Expect(ConditionalResult).ToContainTextAsync("passed", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task conditional_rule_enforced_when_checked()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input(m => m.Conditional.IsEmployed).CheckAsync();

        await ClickWhenStable(ConditionalBtn);

        await Expect(ErrorFor(m => m.Conditional.JobTitle)).ToHaveTextAsync("Job title is required when employed.", new() { Timeout = 2000 });
        await Expect(ConditionalResult).ToHaveTextAsync("Click to validate");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task conditional_rule_clears_when_condition_becomes_false()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input(m => m.Conditional.IsEmployed).CheckAsync();
        await ClickWhenStable(ConditionalBtn);
        await Expect(ErrorFor(m => m.Conditional.JobTitle)).ToHaveTextAsync("Job title is required when employed.", new() { Timeout = 2000 });

        await Input(m => m.Conditional.IsEmployed).UncheckAsync();
        await ClickWhenStable(ConditionalBtn);

        await Expect(ConditionalResult).ToContainTextAsync("passed", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task conditional_rule_passes_when_filled()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input(m => m.Conditional.IsEmployed).CheckAsync();
        await Input(m => m.Conditional.JobTitle).FillAsync("Care Coordinator");
        await ClickWhenStable(ConditionalBtn);

        await Expect(ConditionalResult).ToContainTextAsync("passed", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task phone_regex_shows_format_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(AllRulesBtn);

        await Input(m => m.AllRules.Phone).FillAsync("badphone");
        await Input(m => m.AllRules.Phone).BlurAsync();

        await Expect(ErrorFor(m => m.AllRules.Phone)).ToHaveTextAsync("Phone must match format 123-456-7890.", new() { Timeout = 2000 });

        await Input(m => m.AllRules.Phone).FillAsync("123-456-7890");
        await Input(m => m.AllRules.Phone).BlurAsync();

        await Expect(ErrorFor(m => m.AllRules.Phone)).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task salary_range_shows_boundary_errors()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(AllRulesBtn);

        await Input(m => m.AllRules.Salary).FillAsync("500001");
        await Input(m => m.AllRules.Salary).BlurAsync();

        await Expect(ErrorFor(m => m.AllRules.Salary)).ToHaveTextAsync("Salary must be at most 500,000.", new() { Timeout = 2000 });

        await Input(m => m.AllRules.Salary).FillAsync("500000");
        await Input(m => m.AllRules.Salary).BlurAsync();

        await Expect(ErrorFor(m => m.AllRules.Salary)).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task all_rules_pass_shows_success_message()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input(m => m.AllRules.Name).FillAsync("Dorothy Henderson");
        await Input(m => m.AllRules.Email).FillAsync("dorothy@seniorcare.com");
        await Input(m => m.AllRules.Age).FillAsync("82");
        await Input(m => m.AllRules.Phone).FillAsync("503-555-1234");
        await Input(m => m.AllRules.Salary).FillAsync("45000");
        await Input(m => m.AllRules.Password).FillAsync("securepass123");

        await ClickWhenStable(AllRulesBtn);

        await Expect(AllRulesResult).ToHaveTextAsync("All fields passed client-side validation!", new() { Timeout = 5000 });
        await Expect(AllRulesResult).ToHaveClassAsync(new Regex("text-green-600"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task error_class_applied_to_invalid_fields()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(AllRulesBtn);

        await Expect(Input(m => m.AllRules.Name)).ToHaveClassAsync(new Regex("alis-has-error"));
        await Expect(Input(m => m.AllRules.Email)).ToHaveClassAsync(new Regex("alis-has-error"));

        await Input(m => m.AllRules.Name).FillAsync("Valid Name");
        await Input(m => m.AllRules.Name).BlurAsync();

        await Expect(Input(m => m.AllRules.Name)).Not.ToHaveClassAsync(new Regex("alis-has-error"), new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task server_form_empty_shows_required_errors()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(ServerBtn);

        await Expect(ErrorFor(m => m.Server.Name)).ToHaveTextAsync("Name is required.", new() { Timeout = 2000 });
        await Expect(ErrorFor(m => m.Server.Email)).ToHaveTextAsync("Email is required.", new() { Timeout = 2000 });
        await Expect(Page.Locator("#server-result")).ToHaveTextAsync("Not submitted yet");

        AssertNoConsoleErrors();
    }

    // The page checks only the server section, so the server, checking the whole model, refuses the
    // sections the page did not send: their messages have no field on this form and go to the summary.
    [Test]
    public async Task server_rejects_incomplete_data_and_shows_errors()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input(m => m.Server.Name).FillAsync("Harold Wilson");
        await Input(m => m.Server.Email).FillAsync("harold@care.com");

        await ClickWhenStable(ServerBtn);

        var serverResult = Page.Locator("#server-result");
        await Expect(serverResult).ToHaveTextAsync("Server returned validation errors", new() { Timeout = 5000 });
        await Expect(serverResult).ToHaveClassAsync(new Regex("text-red-600"));
        await Expect(SummaryOf("server-form")).ToBeVisibleAsync();
        await Expect(SummaryOf("server-form").Locator("[data-valmsg-summary-for='AllRules.Name']")).ToHaveTextAsync("Name is required.");
        await Expect(SummaryOf("server-form").Locator("[data-valmsg-summary-for='Db.Email']")).ToHaveTextAsync("Email is required.");
        await Expect(ErrorFor(m => m.Server.Name)).ToHaveTextAsync("");
        await Expect(ErrorFor(m => m.Server.Email)).ToHaveTextAsync("");

        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task server_form_invalid_email_shows_format_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input(m => m.Server.Name).FillAsync("Harold Wilson");
        await Input(m => m.Server.Email).FillAsync("not-an-email");

        await ClickWhenStable(ServerBtn);

        await Expect(ErrorFor(m => m.Server.Email)).ToHaveTextAsync("Email must be a valid email address.", new() { Timeout = 2000 });
        await Expect(Page.Locator("#server-result")).ToHaveTextAsync("Not submitted yet");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task conditional_job_title_live_clears_on_valid_input()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input(m => m.Conditional.IsEmployed).CheckAsync();
        await ClickWhenStable(ConditionalBtn);
        await Expect(ErrorFor(m => m.Conditional.JobTitle)).ToHaveTextAsync("Job title is required when employed.", new() { Timeout = 2000 });

        await Input(m => m.Conditional.JobTitle).FillAsync("Activities Director");
        await Input(m => m.Conditional.JobTitle).BlurAsync();

        await Expect(ErrorFor(m => m.Conditional.JobTitle)).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task hidden_form_name_required_when_extras_hidden()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(HiddenBtn);

        await Expect(ErrorFor(m => m.Hidden.Name)).ToHaveTextAsync("Name is required.", new() { Timeout = 2000 });
        await Expect(Page.Locator("#hidden-result")).ToHaveTextAsync("Click to validate or submit");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task hidden_form_name_valid_passes_when_extras_hidden()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input(m => m.Hidden.Name).FillAsync("Edith Collins");

        await ClickWhenStable(HiddenBtn);

        var hiddenResult = Page.Locator("#hidden-result");
        await Expect(hiddenResult).ToContainTextAsync("passed", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // Each form on the page has a summary of its own: the messages no field of that form shows.
    private ILocator SummaryOf(string formId) => Page.Locator($"#{formId}_validation_summary");
    private const string PhoneFormat = "Phone must match format 123-456-7890.";

    // A phone typed into the extra fields, which are then hidden, is reported in the page's summary:
    // its message slot is hidden with them.
    private async Task ReportAPhoneWhileTheExtraFieldsAreHidden()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
        await Input(m => m.Hidden.Name).FillAsync("Edith Collins");
        await Input(m => m.Hidden.ShowExtras).CheckAsync();
        await Input(m => m.Hidden.Phone).FillAsync("555");
        await Input(m => m.Hidden.ShowExtras).UncheckAsync();
        await Expect(Page.Locator("#hf_extras")).ToBeHiddenAsync();

        await ClickWhenStable(HiddenBtn);

        await Expect(SummaryOf("hidden-fields-form")).ToBeVisibleAsync();
        await Expect(SummaryOf("hidden-fields-form")).ToHaveTextAsync(PhoneFormat);
        await Input(m => m.Hidden.ShowExtras).CheckAsync();
    }

    [Test]
    public async Task correcting_a_phone_reported_while_hidden_takes_it_out_of_the_summary()
    {
        await ReportAPhoneWhileTheExtraFieldsAreHidden();

        await Input(m => m.Hidden.Phone).FillAsync("123-456-7890");
        await Input(m => m.Hidden.Phone).PressAsync("Tab");

        await Expect(SummaryOf("hidden-fields-form")).ToBeHiddenAsync();
        await Expect(ErrorFor(m => m.Hidden.Phone)).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_phone_error_shown_beside_the_field_leaves_no_empty_summary()
    {
        await ReportAPhoneWhileTheExtraFieldsAreHidden();

        await Input(m => m.Hidden.Phone).FillAsync("55512");
        await Input(m => m.Hidden.Phone).PressAsync("Tab");

        await Expect(ErrorFor(m => m.Hidden.Phone)).ToHaveTextAsync(PhoneFormat);
        await Expect(SummaryOf("hidden-fields-form")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task hidden_form_show_extras_reveals_additional_fields()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var extras = Page.Locator("#hf_extras");
        await Expect(extras).ToBeHiddenAsync();

        await Input(m => m.Hidden.ShowExtras).CheckAsync();

        await Expect(extras).ToBeVisibleAsync(new() { Timeout = 2000 });

        await Input(m => m.Hidden.ShowExtras).UncheckAsync();
        await Expect(extras).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task db_form_empty_shows_required_errors()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(DbBtn);

        await Expect(ErrorFor(m => m.Db.Name)).ToHaveTextAsync("Name is required.", new() { Timeout = 2000 });
        await Expect(ErrorFor(m => m.Db.Email)).ToHaveTextAsync("Email is required.", new() { Timeout = 2000 });
        await Expect(Page.Locator("#db-result")).ToHaveTextAsync("Not submitted yet");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task db_form_reserved_name_shows_server_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input(m => m.Db.Name).FillAsync("admin");
        await Input(m => m.Db.Email).FillAsync("valid@test.com");

        await ClickWhenStable(DbBtn);

        var dbResult = Page.Locator("#db-result");
        await Expect(dbResult).ToContainTextAsync("Database validation failed", new() { Timeout = 5000 });

        await Expect(ErrorFor(m => m.Db.Name)).ToHaveTextAsync("This username is reserved by the system.", new() { Timeout = 2000 });

        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task db_form_taken_email_shows_server_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input(m => m.Db.Name).FillAsync("validuser");
        await Input(m => m.Db.Email).FillAsync("taken@test.com");

        await ClickWhenStable(DbBtn);

        var dbResult = Page.Locator("#db-result");
        await Expect(dbResult).ToContainTextAsync("Database validation failed", new() { Timeout = 5000 });

        await Expect(ErrorFor(m => m.Db.Email)).ToHaveTextAsync("This email address is already registered.", new() { Timeout = 2000 });

        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task db_form_valid_data_shows_success()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input(m => m.Db.Name).FillAsync("newuser");
        await Input(m => m.Db.Email).FillAsync("newuser@example.com");

        await ClickWhenStable(DbBtn);

        var dbResult = Page.Locator("#db-result");
        await Expect(dbResult).ToHaveTextAsync("Saved to database!", new() { Timeout = 5000 });
        await Expect(dbResult).ToHaveClassAsync(new Regex("text-green-600"));

        AssertNoConsoleErrors();
    }
}
