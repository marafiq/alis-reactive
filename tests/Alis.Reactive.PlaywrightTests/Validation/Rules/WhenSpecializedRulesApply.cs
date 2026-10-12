namespace Alis.Reactive.PlaywrightTests.Validation.Rules;

[TestFixture]
public class WhenSpecializedRulesApply : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/SpecializedRules";
    private const string ModelIdPrefix = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_NewRuleTypesModel__";

    private ILocator ValidateBtn => Page.Locator("#validate-new-rules-btn");
    private ILocator Result => Page.Locator("#new-rules-result");
    private ILocator Input(string prop) => Page.Locator($"#{ModelIdPrefix}{prop}");
    private ILocator ErrorFor(string prop) => Page.Locator($"#{ModelIdPrefix}{prop}_error");

    [Test]
    public async Task when_billing_enters_invalid_card_number_error_shows()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("CardNumber").FillAsync("1234567890123");
        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor("CardNumber")).ToContainTextAsync("not valid", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_billing_enters_valid_visa_card_error_clears()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("CardNumber").FillAsync("1234567890123");
        await ClickWhenStable(ValidateBtn);
        await Expect(ErrorFor("CardNumber")).ToContainTextAsync("not valid", new() { Timeout = 2000 });

        await Input("CardNumber").FillAsync("4111111111111111");
        await Input("CardNumber").BlurAsync();

        await Expect(ErrorFor("CardNumber")).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_nurse_enters_score_at_boundary_exclusive_range_error_shows()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Score").FillAsync("0");
        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor("Score")).ToContainTextAsync("exclusive", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_nurse_enters_score_at_upper_boundary_exclusive_range_error_shows()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Score").FillAsync("100");
        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor("Score")).ToContainTextAsync("exclusive", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_nurse_enters_valid_score_within_range_error_clears()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Score").FillAsync("0");
        await ClickWhenStable(ValidateBtn);
        await Expect(ErrorFor("Score")).ToContainTextAsync("exclusive", new() { Timeout = 2000 });

        await Input("Score").FillAsync("50");
        await Input("Score").BlurAsync();

        await Expect(ErrorFor("Score")).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_monthly_rate_is_zero_gt_error_shows()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("MonthlyRate").FillAsync("0");
        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor("MonthlyRate")).ToContainTextAsync("greater than zero", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_monthly_rate_is_empty_gt_still_fails_because_gt_implies_required()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor("MonthlyRate")).ToContainTextAsync("greater than zero", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_monthly_rate_is_positive_gt_passes()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("MonthlyRate").FillAsync("0");
        await ClickWhenStable(ValidateBtn);
        await Expect(ErrorFor("MonthlyRate")).ToContainTextAsync("greater than zero", new() { Timeout = 2000 });

        await Input("MonthlyRate").FillAsync("4250");
        await Input("MonthlyRate").BlurAsync();

        await Expect(ErrorFor("MonthlyRate")).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_deposit_equals_limit_lt_error_shows()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("MaxDeposit").FillAsync("1000000");
        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor("MaxDeposit")).ToContainTextAsync("less than", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_deposit_is_under_limit_lt_passes()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("MaxDeposit").FillAsync("1000000");
        await ClickWhenStable(ValidateBtn);
        await Expect(ErrorFor("MaxDeposit")).ToContainTextAsync("less than", new() { Timeout = 2000 });

        await Input("MaxDeposit").FillAsync("999999");
        await Input("MaxDeposit").BlurAsync();

        await Expect(ErrorFor("MaxDeposit")).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_admin_sets_status_to_deleted_notEqual_error_shows()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Status").FillAsync("deleted");
        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor("Status")).ToContainTextAsync("must not be", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_admin_sets_status_to_active_notEqual_passes()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Status").FillAsync("deleted");
        await ClickWhenStable(ValidateBtn);
        await Expect(ErrorFor("Status")).ToContainTextAsync("must not be", new() { Timeout = 2000 });

        await Input("Status").FillAsync("active");
        await Input("Status").BlurAsync();

        await Expect(ErrorFor("Status")).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_alternate_email_matches_primary_notEqualTo_error_shows()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Email").FillAsync("nurse@facility.com");
        await Input("AlternateEmail").FillAsync("nurse@facility.com");
        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor("AlternateEmail")).ToContainTextAsync("differ", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_alternate_email_differs_from_primary_notEqualTo_passes()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Email").FillAsync("nurse@facility.com");
        await Input("AlternateEmail").FillAsync("nurse@facility.com");
        await ClickWhenStable(ValidateBtn);
        await Expect(ErrorFor("AlternateEmail")).ToContainTextAsync("differ", new() { Timeout = 2000 });

        await Input("AlternateEmail").FillAsync("backup@facility.com");
        await Input("AlternateEmail").BlurAsync();

        await Expect(ErrorFor("AlternateEmail")).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_facility_website_is_not_a_url_error_shows()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Website").FillAsync("not-a-url");
        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor("Website")).ToContainTextAsync("valid URL", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_facility_website_is_valid_url_error_clears()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Website").FillAsync("not-a-url");
        await ClickWhenStable(ValidateBtn);
        await Expect(ErrorFor("Website")).ToContainTextAsync("valid URL", new() { Timeout = 2000 });

        await Input("Website").FillAsync("https://sunnyacres.com");
        await Input("Website").BlurAsync();

        await Expect(ErrorFor("Website")).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_nickname_field_has_value_empty_rule_error_shows()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Nickname").FillAsync("Maggie");
        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor("Nickname")).ToContainTextAsync("must be empty", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_nickname_is_cleared_empty_rule_passes()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Nickname").FillAsync("Maggie");
        await ClickWhenStable(ValidateBtn);
        await Expect(ErrorFor("Nickname")).ToContainTextAsync("must be empty", new() { Timeout = 2000 });

        await Input("Nickname").FillAsync("");
        await Input("Nickname").BlurAsync();

        await Expect(ErrorFor("Nickname")).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    private async Task FillEveryRuleValidly()
    {
        await Input("CardNumber").FillAsync("4111111111111111");
        await Input("Score").FillAsync("50");
        await Input("MonthlyRate").FillAsync("4250");
        await Input("MaxDeposit").FillAsync("500000");
        await Input("Status").FillAsync("active");
        await Input("Email").FillAsync("nurse@facility.com");
        await Input("AlternateEmail").FillAsync("backup@facility.com");
        await Input("Website").FillAsync("https://sunnyacres.com");
    }

    // The server binds an empty decimal as 0, which is outside (0, 100); the browser judges the same 0.
    [Test]
    public async Task a_score_left_empty_is_refused_as_the_zero_the_server_binds()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
        await FillEveryRuleValidly();
        await Input("Score").FillAsync("");

        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor("Score")).ToContainTextAsync("exclusive", new() { Timeout = 2000 });
        await Expect(Result).ToHaveTextAsync("Click to validate");
        AssertNoConsoleErrors();
    }

    // FluentValidation drops dashes and spaces and refuses any other character that is not a digit.
    [Test]
    public async Task a_card_number_with_a_letter_in_it_is_refused_though_its_digits_check_out()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
        await FillEveryRuleValidly();
        await Input("CardNumber").FillAsync("4111 1111 1111 111x1");

        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor("CardNumber")).ToContainTextAsync("not valid", new() { Timeout = 2000 });
        await Expect(Result).ToHaveTextAsync("Click to validate");
        AssertNoConsoleErrors();
    }

    // FluentValidation sets no minimum length: a twelve-digit number that passes the Luhn check is a
    // card number to the server, so the browser lets it through and the server accepts it.
    [Test]
    public async Task a_twelve_digit_card_number_that_passes_the_check_is_accepted_by_the_server_too()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
        await FillEveryRuleValidly();
        await Input("CardNumber").FillAsync("4242 4242 4242");

        await ClickWhenStable(ValidateBtn);

        await Expect(Result).ToContainTextAsync("passed", new() { Timeout = 5000 });
        await Expect(ErrorFor("CardNumber")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_all_fields_are_valid_form_passes()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("CardNumber").FillAsync("4111111111111111");
        await Input("Score").FillAsync("50");
        await Input("MonthlyRate").FillAsync("4250");
        await Input("MaxDeposit").FillAsync("500000");
        await Input("Status").FillAsync("active");
        await Input("Email").FillAsync("nurse@facility.com");
        await Input("AlternateEmail").FillAsync("backup@facility.com");
        await Input("Website").FillAsync("https://sunnyacres.com");

        await ClickWhenStable(ValidateBtn);

        await Expect(Result).ToContainTextAsync("passed", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
