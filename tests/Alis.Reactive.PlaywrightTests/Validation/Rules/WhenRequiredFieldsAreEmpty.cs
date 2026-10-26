using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.Validation.Rules;

[TestFixture]
public class WhenRequiredFieldsAreEmpty : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/AllRules";
    private const string ModelIdPrefix = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ValidationShowcaseModel__";

    private ILocator AllRulesBtn => Page.Locator("#validate-all-btn");
    private ILocator AllRulesResult => Page.Locator("#all-rules-result");
    private ILocator Input(string suffix) => Page.Locator($"#{ModelIdPrefix}{suffix}");
    private ILocator ErrorFor(string suffix) => Page.Locator($"#{ModelIdPrefix}{suffix}_error");

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

        await Expect(ErrorFor("AllRules_Name")).ToHaveTextAsync("Name is required.");
        await Expect(ErrorFor("AllRules_Email")).ToHaveTextAsync("Email is required.");
        await Expect(Input("AllRules_Name")).ToHaveClassAsync(new Regex("alis-has-error"));
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

        await Input("AllRules_Password").FillAsync("abcdefg");
        await Input("AllRules_Password").BlurAsync();

        await Expect(ErrorFor("AllRules_Password")).ToHaveTextAsync("Password must be at least 8 characters.", new() { Timeout = 2000 });

        await Input("AllRules_Password").FillAsync("abcdefgh");
        await Input("AllRules_Password").BlurAsync();

        await Expect(ErrorFor("AllRules_Password")).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task valid_name_clears_error_on_blur()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(AllRulesBtn);
        await Expect(ErrorFor("AllRules_Name")).ToHaveTextAsync("Name is required.");

        await Input("AllRules_Name").FillAsync("Margaret Thompson");
        await Input("AllRules_Name").BlurAsync();

        await Expect(ErrorFor("AllRules_Name")).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task email_validates_format()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(AllRulesBtn);
        await Expect(ErrorFor("AllRules_Email")).ToHaveTextAsync("Email is required.");

        await Input("AllRules_Email").FillAsync("notanemail");
        await Input("AllRules_Email").BlurAsync();

        await Expect(ErrorFor("AllRules_Email")).ToHaveTextAsync("Email must be a valid email address.", new() { Timeout = 2000 });

        await Input("AllRules_Email").FillAsync("margaret@care.com");
        await Input("AllRules_Email").BlurAsync();

        await Expect(ErrorFor("AllRules_Email")).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task age_range_validates()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(AllRulesBtn);

        await Input("AllRules_Age").FillAsync("121");
        await Input("AllRules_Age").BlurAsync();

        await Expect(ErrorFor("AllRules_Age")).ToHaveTextAsync("Age must be between 0 and 120.", new() { Timeout = 2000 });

        await Input("AllRules_Age").FillAsync("120");
        await Input("AllRules_Age").BlurAsync();

        await Expect(ErrorFor("AllRules_Age")).ToBeHiddenAsync(new() { Timeout = 2000 });

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

        await Input("Conditional_IsEmployed").CheckAsync();

        await ClickWhenStable(ConditionalBtn);

        await Expect(ErrorFor("Conditional_JobTitle")).ToHaveTextAsync("Job title is required when employed.", new() { Timeout = 2000 });
        await Expect(ConditionalResult).ToHaveTextAsync("Click to validate");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task conditional_rule_clears_when_condition_becomes_false()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Conditional_IsEmployed").CheckAsync();
        await ClickWhenStable(ConditionalBtn);
        await Expect(ErrorFor("Conditional_JobTitle")).ToHaveTextAsync("Job title is required when employed.", new() { Timeout = 2000 });

        await Input("Conditional_IsEmployed").UncheckAsync();
        await ClickWhenStable(ConditionalBtn);

        await Expect(ConditionalResult).ToContainTextAsync("passed", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task conditional_rule_passes_when_filled()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Conditional_IsEmployed").CheckAsync();
        await Input("Conditional_JobTitle").FillAsync("Care Coordinator");
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

        await Input("AllRules_Phone").FillAsync("badphone");
        await Input("AllRules_Phone").BlurAsync();

        await Expect(ErrorFor("AllRules_Phone")).ToHaveTextAsync("Phone must match format 123-456-7890.", new() { Timeout = 2000 });

        await Input("AllRules_Phone").FillAsync("123-456-7890");
        await Input("AllRules_Phone").BlurAsync();

        await Expect(ErrorFor("AllRules_Phone")).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task salary_range_shows_boundary_errors()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(AllRulesBtn);

        await Input("AllRules_Salary").FillAsync("500001");
        await Input("AllRules_Salary").BlurAsync();

        await Expect(ErrorFor("AllRules_Salary")).ToHaveTextAsync("Salary must be at most 500,000.", new() { Timeout = 2000 });

        await Input("AllRules_Salary").FillAsync("500000");
        await Input("AllRules_Salary").BlurAsync();

        await Expect(ErrorFor("AllRules_Salary")).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task all_rules_pass_shows_success_message()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("AllRules_Name").FillAsync("Dorothy Henderson");
        await Input("AllRules_Email").FillAsync("dorothy@seniorcare.com");
        await Input("AllRules_Age").FillAsync("82");
        await Input("AllRules_Phone").FillAsync("503-555-1234");
        await Input("AllRules_Salary").FillAsync("45000");
        await Input("AllRules_Password").FillAsync("securepass123");

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

        await Expect(Input("AllRules_Name")).ToHaveClassAsync(new Regex("alis-has-error"));
        await Expect(Input("AllRules_Email")).ToHaveClassAsync(new Regex("alis-has-error"));

        await Input("AllRules_Name").FillAsync("Valid Name");
        await Input("AllRules_Name").BlurAsync();

        await Expect(Input("AllRules_Name")).Not.ToHaveClassAsync(new Regex("alis-has-error"), new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task server_form_empty_shows_required_errors()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(ServerBtn);

        await Expect(ErrorFor("Server_Name")).ToHaveTextAsync("Name is required.", new() { Timeout = 2000 });
        await Expect(ErrorFor("Server_Email")).ToHaveTextAsync("Email is required.", new() { Timeout = 2000 });
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

        await Input("Server_Name").FillAsync("Harold Wilson");
        await Input("Server_Email").FillAsync("harold@care.com");

        await ClickWhenStable(ServerBtn);

        var serverResult = Page.Locator("#server-result");
        await Expect(serverResult).ToHaveTextAsync("Server returned validation errors", new() { Timeout = 5000 });
        await Expect(serverResult).ToHaveClassAsync(new Regex("text-red-600"));
        await Expect(ValidationSummary).ToBeVisibleAsync();
        await Expect(ValidationSummary.Locator("[data-valmsg-summary-for='AllRules.Name']")).ToHaveTextAsync("Name is required.");
        await Expect(ValidationSummary.Locator("[data-valmsg-summary-for='Db.Email']")).ToHaveTextAsync("Email is required.");
        await Expect(ErrorFor("Server_Name")).ToHaveTextAsync("");
        await Expect(ErrorFor("Server_Email")).ToHaveTextAsync("");

        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task server_form_invalid_email_shows_format_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Server_Name").FillAsync("Harold Wilson");
        await Input("Server_Email").FillAsync("not-an-email");

        await ClickWhenStable(ServerBtn);

        await Expect(ErrorFor("Server_Email")).ToHaveTextAsync("Email must be a valid email address.", new() { Timeout = 2000 });
        await Expect(Page.Locator("#server-result")).ToHaveTextAsync("Not submitted yet");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task conditional_job_title_live_clears_on_valid_input()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Conditional_IsEmployed").CheckAsync();
        await ClickWhenStable(ConditionalBtn);
        await Expect(ErrorFor("Conditional_JobTitle")).ToHaveTextAsync("Job title is required when employed.", new() { Timeout = 2000 });

        await Input("Conditional_JobTitle").FillAsync("Activities Director");
        await Input("Conditional_JobTitle").BlurAsync();

        await Expect(ErrorFor("Conditional_JobTitle")).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task hidden_form_name_required_when_extras_hidden()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(HiddenBtn);

        await Expect(ErrorFor("Hidden_Name")).ToHaveTextAsync("Name is required.", new() { Timeout = 2000 });
        await Expect(Page.Locator("#hidden-result")).ToHaveTextAsync("Click to validate or submit");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task hidden_form_name_valid_passes_when_extras_hidden()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Hidden_Name").FillAsync("Edith Collins");

        await ClickWhenStable(HiddenBtn);

        var hiddenResult = Page.Locator("#hidden-result");
        await Expect(hiddenResult).ToContainTextAsync("passed", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    private ILocator ValidationSummary => Page.Locator("[data-reactive-validation-summary]");
    private const string PhoneFormat = "Phone must match format 123-456-7890.";

    // A phone typed into the extra fields, which are then hidden, is reported in the page's summary:
    // its message slot is hidden with them.
    private async Task ReportAPhoneWhileTheExtraFieldsAreHidden()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
        await Input("Hidden_Name").FillAsync("Edith Collins");
        await Input("Hidden_ShowExtras").CheckAsync();
        await Input("Hidden_Phone").FillAsync("555");
        await Input("Hidden_ShowExtras").UncheckAsync();
        await Expect(Page.Locator("#hf_extras")).ToBeHiddenAsync();

        await ClickWhenStable(HiddenBtn);

        await Expect(ValidationSummary).ToBeVisibleAsync();
        await Expect(ValidationSummary).ToHaveTextAsync(PhoneFormat);
        await Input("Hidden_ShowExtras").CheckAsync();
    }

    [Test]
    public async Task correcting_a_phone_reported_while_hidden_takes_it_out_of_the_summary()
    {
        await ReportAPhoneWhileTheExtraFieldsAreHidden();

        await Input("Hidden_Phone").FillAsync("123-456-7890");
        await Input("Hidden_Phone").PressAsync("Tab");

        await Expect(ValidationSummary).ToBeHiddenAsync();
        await Expect(ErrorFor("Hidden_Phone")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_phone_error_shown_beside_the_field_leaves_no_empty_summary()
    {
        await ReportAPhoneWhileTheExtraFieldsAreHidden();

        await Input("Hidden_Phone").FillAsync("55512");
        await Input("Hidden_Phone").PressAsync("Tab");

        await Expect(ErrorFor("Hidden_Phone")).ToHaveTextAsync(PhoneFormat);
        await Expect(ValidationSummary).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task hidden_form_show_extras_reveals_additional_fields()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var extras = Page.Locator("#hf_extras");
        await Expect(extras).ToBeHiddenAsync();

        await Input("Hidden_ShowExtras").CheckAsync();

        await Expect(extras).ToBeVisibleAsync(new() { Timeout = 2000 });

        await Input("Hidden_ShowExtras").UncheckAsync();
        await Expect(extras).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task db_form_empty_shows_required_errors()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(DbBtn);

        await Expect(ErrorFor("Db_Name")).ToHaveTextAsync("Name is required.", new() { Timeout = 2000 });
        await Expect(ErrorFor("Db_Email")).ToHaveTextAsync("Email is required.", new() { Timeout = 2000 });
        await Expect(Page.Locator("#db-result")).ToHaveTextAsync("Not submitted yet");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task db_form_reserved_name_shows_server_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Db_Name").FillAsync("admin");
        await Input("Db_Email").FillAsync("valid@test.com");

        await ClickWhenStable(DbBtn);

        var dbResult = Page.Locator("#db-result");
        await Expect(dbResult).ToContainTextAsync("Database validation failed", new() { Timeout = 5000 });

        await Expect(ErrorFor("Db_Name")).ToHaveTextAsync("This username is reserved by the system.", new() { Timeout = 2000 });

        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task db_form_taken_email_shows_server_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Db_Name").FillAsync("validuser");
        await Input("Db_Email").FillAsync("taken@test.com");

        await ClickWhenStable(DbBtn);

        var dbResult = Page.Locator("#db-result");
        await Expect(dbResult).ToContainTextAsync("Database validation failed", new() { Timeout = 5000 });

        await Expect(ErrorFor("Db_Email")).ToHaveTextAsync("This email address is already registered.", new() { Timeout = 2000 });

        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task db_form_valid_data_shows_success()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Db_Name").FillAsync("newuser");
        await Input("Db_Email").FillAsync("newuser@example.com");

        await ClickWhenStable(DbBtn);

        var dbResult = Page.Locator("#db-result");
        await Expect(dbResult).ToHaveTextAsync("Saved to database!", new() { Timeout = 5000 });
        await Expect(dbResult).ToHaveClassAsync(new Regex("text-green-600"));

        AssertNoConsoleErrors();
    }
}
