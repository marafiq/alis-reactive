using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.Validation;

/// <summary>
/// BDD Playwright tests for /Sandbox/NewRuleTypes — exercises every new validation
/// rule type end-to-end in the browser. Senior living domain scenarios.
/// </summary>
[TestFixture]
public class WhenValidatingNewRuleTypes : PlaywrightTestBase
{
    private const string Path = "/Sandbox/NewRuleTypes";
    private const string R = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_NewRuleTypesModel__";

    private ILocator ValidateBtn => Page.Locator("#validate-new-rules-btn");
    private ILocator Result => Page.Locator("#new-rules-result");
    private ILocator Input(string prop) => Page.Locator($"#{R}{prop}");
    private ILocator ErrorFor(string prop) => Page.Locator($"#{R}{prop}_error");

    // ── creditCard ───────────────────────────────────────────

    [Test]
    public async Task when_billing_enters_invalid_card_number_error_shows()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("CardNumber").FillAsync("1234567890123");
        await ValidateBtn.ClickAsync();

        await Expect(ErrorFor("CardNumber")).ToContainTextAsync("not valid", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_billing_enters_valid_visa_card_error_clears()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // First trigger error
        await Input("CardNumber").FillAsync("1234567890123");
        await ValidateBtn.ClickAsync();
        await Expect(ErrorFor("CardNumber")).ToContainTextAsync("not valid", new() { Timeout = 2000 });

        // Fix with valid Visa test number, blur to live-clear
        await Input("CardNumber").FillAsync("4111111111111111");
        await Input("CardNumber").BlurAsync();

        await Expect(ErrorFor("CardNumber")).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    // ── exclusiveRange ───────────────────────────────────────

    [Test]
    public async Task when_nurse_enters_score_at_boundary_exclusive_range_error_shows()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Score at exact boundary (0) — exclusive range rejects boundaries
        await Input("Score").FillAsync("0");
        await ValidateBtn.ClickAsync();

        await Expect(ErrorFor("Score")).ToContainTextAsync("exclusive", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_nurse_enters_score_at_upper_boundary_exclusive_range_error_shows()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Score").FillAsync("100");
        await ValidateBtn.ClickAsync();

        await Expect(ErrorFor("Score")).ToContainTextAsync("exclusive", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_nurse_enters_valid_score_within_range_error_clears()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Trigger error first
        await Input("Score").FillAsync("0");
        await ValidateBtn.ClickAsync();
        await Expect(ErrorFor("Score")).ToContainTextAsync("exclusive", new() { Timeout = 2000 });

        // Fix
        await Input("Score").FillAsync("50");
        await Input("Score").BlurAsync();

        await Expect(ErrorFor("Score")).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    // ── gt (greater than, implies required) ──────────────────

    [Test]
    public async Task when_monthly_rate_is_zero_gt_error_shows()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("MonthlyRate").FillAsync("0");
        await ValidateBtn.ClickAsync();

        await Expect(ErrorFor("MonthlyRate")).ToContainTextAsync("greater than zero", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_monthly_rate_is_empty_gt_still_fails_because_gt_implies_required()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Leave empty — gt implies required
        await ValidateBtn.ClickAsync();

        await Expect(ErrorFor("MonthlyRate")).ToContainTextAsync("greater than zero", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_monthly_rate_is_positive_gt_passes()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("MonthlyRate").FillAsync("0");
        await ValidateBtn.ClickAsync();
        await Expect(ErrorFor("MonthlyRate")).ToContainTextAsync("greater than zero", new() { Timeout = 2000 });

        await Input("MonthlyRate").FillAsync("4250");
        await Input("MonthlyRate").BlurAsync();

        await Expect(ErrorFor("MonthlyRate")).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    // ── lt (less than) ───────────────────────────────────────

    [Test]
    public async Task when_deposit_equals_limit_lt_error_shows()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("MaxDeposit").FillAsync("1000000");
        await ValidateBtn.ClickAsync();

        await Expect(ErrorFor("MaxDeposit")).ToContainTextAsync("less than", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_deposit_is_under_limit_lt_passes()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("MaxDeposit").FillAsync("1000000");
        await ValidateBtn.ClickAsync();
        await Expect(ErrorFor("MaxDeposit")).ToContainTextAsync("less than", new() { Timeout = 2000 });

        await Input("MaxDeposit").FillAsync("999999");
        await Input("MaxDeposit").BlurAsync();

        await Expect(ErrorFor("MaxDeposit")).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    // ── notEqual (fixed value) ───────────────────────────────

    [Test]
    public async Task when_admin_sets_status_to_deleted_notEqual_error_shows()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Status").FillAsync("deleted");
        await ValidateBtn.ClickAsync();

        await Expect(ErrorFor("Status")).ToContainTextAsync("must not be", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_admin_sets_status_to_active_notEqual_passes()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Status").FillAsync("deleted");
        await ValidateBtn.ClickAsync();
        await Expect(ErrorFor("Status")).ToContainTextAsync("must not be", new() { Timeout = 2000 });

        await Input("Status").FillAsync("active");
        await Input("Status").BlurAsync();

        await Expect(ErrorFor("Status")).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    // ── notEqualTo (cross-property) ──────────────────────────

    [Test]
    public async Task when_alternate_email_matches_primary_notEqualTo_error_shows()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Email").FillAsync("nurse@facility.com");
        await Input("AlternateEmail").FillAsync("nurse@facility.com");
        await ValidateBtn.ClickAsync();

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
        await ValidateBtn.ClickAsync();
        await Expect(ErrorFor("AlternateEmail")).ToContainTextAsync("differ", new() { Timeout = 2000 });

        await Input("AlternateEmail").FillAsync("backup@facility.com");
        await Input("AlternateEmail").BlurAsync();

        await Expect(ErrorFor("AlternateEmail")).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    // ── url ──────────────────────────────────────────────────

    [Test]
    public async Task when_facility_website_is_not_a_url_error_shows()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Website").FillAsync("not-a-url");
        await ValidateBtn.ClickAsync();

        await Expect(ErrorFor("Website")).ToContainTextAsync("valid URL", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_facility_website_is_valid_url_error_clears()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Website").FillAsync("not-a-url");
        await ValidateBtn.ClickAsync();
        await Expect(ErrorFor("Website")).ToContainTextAsync("valid URL", new() { Timeout = 2000 });

        await Input("Website").FillAsync("https://sunnyacres.com");
        await Input("Website").BlurAsync();

        await Expect(ErrorFor("Website")).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    // ── empty ────────────────────────────────────────────────

    [Test]
    public async Task when_nickname_field_has_value_empty_rule_error_shows()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Nickname").FillAsync("Maggie");
        await ValidateBtn.ClickAsync();

        await Expect(ErrorFor("Nickname")).ToContainTextAsync("must be empty", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_nickname_is_cleared_empty_rule_passes()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Nickname").FillAsync("Maggie");
        await ValidateBtn.ClickAsync();
        await Expect(ErrorFor("Nickname")).ToContainTextAsync("must be empty", new() { Timeout = 2000 });

        await Input("Nickname").FillAsync("");
        await Input("Nickname").BlurAsync();

        await Expect(ErrorFor("Nickname")).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    // ── Full valid form ──────────────────────────────────────

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
        // Nickname left empty (as required by empty rule)

        await ValidateBtn.ClickAsync();

        await Expect(Result).ToContainTextAsync("passed", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
