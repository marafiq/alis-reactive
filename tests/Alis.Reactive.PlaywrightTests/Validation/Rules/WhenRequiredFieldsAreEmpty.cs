using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.Validation.Rules;

[TestFixture]
public class WhenRequiredFieldsAreEmpty : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/AllRules";
    private const string R = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ValidationShowcaseModel__";

    private ILocator AllRulesBtn => Page.Locator("#validate-all-btn");
    private ILocator AllRulesResult => Page.Locator("#all-rules-result");
    private ILocator Input(string suffix) => Page.Locator($"#{R}{suffix}");
    private ILocator ErrorFor(string suffix) => Page.Locator($"#{R}{suffix}_error");

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

        await Expect(ErrorFor("AllRules_Name")).ToContainTextAsync("required");
        await Expect(ErrorFor("AllRules_Email")).ToContainTextAsync("required");
        await Expect(Input("AllRules_Name")).ToHaveClassAsync(new Regex("alis-has-error"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task password_minlength_validates_on_blur()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(AllRulesBtn);

        await Input("AllRules_Password").FillAsync("abc");
        await Input("AllRules_Password").BlurAsync();

        await Expect(ErrorFor("AllRules_Password")).ToContainTextAsync("at least 8", new() { Timeout = 2000 });

        await Input("AllRules_Password").FillAsync("securepassword");
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
        await Expect(ErrorFor("AllRules_Name")).ToContainTextAsync("required");

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
        await Expect(ErrorFor("AllRules_Email")).ToContainTextAsync("required");

        await Input("AllRules_Email").FillAsync("notanemail");
        await Input("AllRules_Email").BlurAsync();

        await Expect(ErrorFor("AllRules_Email")).ToContainTextAsync("valid email", new() { Timeout = 2000 });

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

        await Input("AllRules_Age").FillAsync("150");
        await Input("AllRules_Age").BlurAsync();

        await Expect(ErrorFor("AllRules_Age")).ToContainTextAsync("between", new() { Timeout = 2000 });

        await Input("AllRules_Age").FillAsync("75");
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

        await Expect(ErrorFor("Conditional_JobTitle")).ToContainTextAsync("required", new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task conditional_rule_clears_when_condition_becomes_false()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Conditional_IsEmployed").CheckAsync();
        await ClickWhenStable(ConditionalBtn);
        await Expect(ErrorFor("Conditional_JobTitle")).ToContainTextAsync("required", new() { Timeout = 2000 });

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

        await Expect(ErrorFor("AllRules_Phone")).ToContainTextAsync("123-456-7890", new() { Timeout = 2000 });

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

        await Input("AllRules_Salary").FillAsync("600000");
        await Input("AllRules_Salary").BlurAsync();

        await Expect(ErrorFor("AllRules_Salary")).ToContainTextAsync("500,000", new() { Timeout = 2000 });

        await Input("AllRules_Salary").FillAsync("75000");
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

        await Expect(AllRulesResult).ToContainTextAsync("passed", new() { Timeout = 5000 });
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

        await Expect(ErrorFor("Server_Name")).ToContainTextAsync("required", new() { Timeout = 2000 });
        await Expect(ErrorFor("Server_Email")).ToContainTextAsync("required", new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task server_rejects_incomplete_data_and_shows_errors()
    {
        // Client rules pass first; OnError(400) must map full-model server errors inline.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Server_Name").FillAsync("Harold Wilson");
        await Input("Server_Email").FillAsync("harold@care.com");

        await ClickWhenStable(ServerBtn);

        var serverResult = Page.Locator("#server-result");
        await Expect(serverResult).ToContainTextAsync("Server returned validation errors", new() { Timeout = 5000 });
        await Expect(serverResult).ToHaveClassAsync(new Regex("text-red-600"));

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task server_form_invalid_email_shows_format_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Server_Name").FillAsync("Harold Wilson");
        await Input("Server_Email").FillAsync("not-an-email");

        await ClickWhenStable(ServerBtn);

        await Expect(ErrorFor("Server_Email")).ToContainTextAsync("valid email", new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task conditional_job_title_live_clears_on_valid_input()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Conditional_IsEmployed").CheckAsync();
        await ClickWhenStable(ConditionalBtn);
        await Expect(ErrorFor("Conditional_JobTitle")).ToContainTextAsync("required", new() { Timeout = 2000 });

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

        await Expect(ErrorFor("Hidden_Name")).ToContainTextAsync("required", new() { Timeout = 2000 });

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

        await Expect(ErrorFor("Db_Name")).ToContainTextAsync("required", new() { Timeout = 2000 });
        await Expect(ErrorFor("Db_Email")).ToContainTextAsync("required", new() { Timeout = 2000 });

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

        await Expect(ErrorFor("Db_Name")).ToContainTextAsync("reserved", new() { Timeout = 2000 });

        AssertNoConsoleErrorsExcept("400");
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

        await Expect(ErrorFor("Db_Email")).ToContainTextAsync("already registered", new() { Timeout = 2000 });

        AssertNoConsoleErrorsExcept("400");
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
        await Expect(dbResult).ToContainTextAsync("Saved to database", new() { Timeout = 5000 });
        await Expect(dbResult).ToHaveClassAsync(new Regex("text-green-600"));

        AssertNoConsoleErrors();
    }
}
