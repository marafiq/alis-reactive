using System.Collections.Concurrent;
using System.Linq.Expressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Validation.Rules;

/// <summary>
/// As a billing administrator, I want each specialized rule checked in the page before anything is
/// sent, so that a value the community would refuse never leaves the form.
/// </summary>
/// <remarks>
/// The page's server runs the same validator, so a message alone could be the server's. Each refusal
/// below fills every other field validly first and proves nothing was sent: the browser's own rule
/// refused it.
/// </remarks>
[TestFixture]
public class WhenSpecializedRulesApply : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/SpecializedRules";

    private ILocator ValidateBtn => Page.Locator("#validate-new-rules-btn");
    private ILocator Result => Page.Locator("#new-rules-result");
    private ILocator Input(Expression<Func<NewRuleTypesModel, object?>> member) => Page.Locator($"#{IdGenerator.For(member)}");
    private ILocator ErrorFor(Expression<Func<NewRuleTypesModel, object?>> member) => Page.Locator($"#{IdGenerator.For(member)}_error");

    private ConcurrentQueue<string> _checksSent = null!;

    private async Task NavigateAndBoot()
    {
        _checksSent = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.Contains("/Sandbox/Validation/SpecializedRules/ValidateClient")) _checksSent.Enqueue(request.Url);
        };
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
    }

    private void AssertNothingSent() =>
        Assert.That(_checksSent, Is.Empty, "The browser refused the value, so the page sent nothing.");

    [Test]
    public async Task when_billing_enters_invalid_card_number_error_shows()
    {
        await NavigateAndBoot();
        await FillEveryRuleValidly();

        await Input(m => m.CardNumber).FillAsync("1234567890123");
        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor(m => m.CardNumber)).ToHaveTextAsync("Card number is not valid.", new() { Timeout = 2000 });
        await Expect(Result).ToHaveTextAsync("Click to validate");
        AssertNothingSent();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_billing_enters_valid_visa_card_error_clears()
    {
        await NavigateAndBoot();
        await FillEveryRuleValidly();

        await Input(m => m.CardNumber).FillAsync("1234567890123");
        await ClickWhenStable(ValidateBtn);
        await Expect(ErrorFor(m => m.CardNumber)).ToHaveTextAsync("Card number is not valid.", new() { Timeout = 2000 });
        AssertNothingSent();

        await Input(m => m.CardNumber).FillAsync("4111111111111111");
        await Input(m => m.CardNumber).BlurAsync();

        await Expect(ErrorFor(m => m.CardNumber)).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_nurse_enters_score_at_boundary_exclusive_range_error_shows()
    {
        await NavigateAndBoot();
        await FillEveryRuleValidly();

        await Input(m => m.Score).FillAsync("0");
        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor(m => m.Score)).ToHaveTextAsync("Score must be between 0 and 100 (exclusive).", new() { Timeout = 2000 });
        await Expect(Result).ToHaveTextAsync("Click to validate");
        AssertNothingSent();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_nurse_enters_score_at_upper_boundary_exclusive_range_error_shows()
    {
        await NavigateAndBoot();
        await FillEveryRuleValidly();

        await Input(m => m.Score).FillAsync("100");
        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor(m => m.Score)).ToHaveTextAsync("Score must be between 0 and 100 (exclusive).", new() { Timeout = 2000 });
        await Expect(Result).ToHaveTextAsync("Click to validate");
        AssertNothingSent();
        AssertNoConsoleErrors();
    }

    // The bounds are excluded; the scores just inside them, 1 and 99, pass in the page and on the server.
    [Test]
    public async Task when_nurse_enters_valid_score_within_range_error_clears()
    {
        await NavigateAndBoot();
        await FillEveryRuleValidly();

        await Input(m => m.Score).FillAsync("0");
        await ClickWhenStable(ValidateBtn);
        await Expect(ErrorFor(m => m.Score)).ToHaveTextAsync("Score must be between 0 and 100 (exclusive).", new() { Timeout = 2000 });
        AssertNothingSent();

        await Input(m => m.Score).FillAsync("1");
        await Input(m => m.Score).BlurAsync();
        await Expect(ErrorFor(m => m.Score)).ToBeHiddenAsync(new() { Timeout = 2000 });

        await Input(m => m.Score).FillAsync("99");
        await ClickWhenStable(ValidateBtn);
        await Expect(Result).ToHaveTextAsync("All new rule types passed!", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_monthly_rate_is_zero_gt_error_shows()
    {
        await NavigateAndBoot();
        await FillEveryRuleValidly();

        await Input(m => m.MonthlyRate).FillAsync("0");
        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor(m => m.MonthlyRate)).ToHaveTextAsync("Monthly rate must be greater than zero.", new() { Timeout = 2000 });
        await Expect(Result).ToHaveTextAsync("Click to validate");
        AssertNothingSent();
        AssertNoConsoleErrors();
    }

    // The rate is a plain decimal: the server binds an empty input as 0, and 0 is not above 0. The
    // page reads the empty input the same way.
    [Test]
    public async Task a_monthly_rate_left_empty_is_refused_as_the_zero_the_server_binds()
    {
        await NavigateAndBoot();
        await FillEveryRuleValidly();
        await Input(m => m.MonthlyRate).FillAsync("");

        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor(m => m.MonthlyRate)).ToHaveTextAsync("Monthly rate must be greater than zero.", new() { Timeout = 2000 });
        await Expect(Result).ToHaveTextAsync("Click to validate");
        AssertNothingSent();
        AssertNoConsoleErrors();
    }

    // One is the smallest whole rate above zero: the page lets it through and the server accepts it.
    [Test]
    public async Task when_monthly_rate_is_positive_gt_passes()
    {
        await NavigateAndBoot();
        await FillEveryRuleValidly();

        await Input(m => m.MonthlyRate).FillAsync("0");
        await ClickWhenStable(ValidateBtn);
        await Expect(ErrorFor(m => m.MonthlyRate)).ToHaveTextAsync("Monthly rate must be greater than zero.", new() { Timeout = 2000 });
        AssertNothingSent();

        await Input(m => m.MonthlyRate).FillAsync("1");
        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor(m => m.MonthlyRate)).ToBeHiddenAsync(new() { Timeout = 2000 });
        await Expect(Result).ToHaveTextAsync("All new rule types passed!", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_deposit_equals_limit_lt_error_shows()
    {
        await NavigateAndBoot();
        await FillEveryRuleValidly();

        await Input(m => m.MaxDeposit).FillAsync("1000000");
        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor(m => m.MaxDeposit)).ToHaveTextAsync("Max deposit must be less than 1,000,000.", new() { Timeout = 2000 });
        await Expect(Result).ToHaveTextAsync("Click to validate");
        AssertNothingSent();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_deposit_is_under_limit_lt_passes()
    {
        await NavigateAndBoot();
        await FillEveryRuleValidly();

        await Input(m => m.MaxDeposit).FillAsync("1000000");
        await ClickWhenStable(ValidateBtn);
        await Expect(ErrorFor(m => m.MaxDeposit)).ToHaveTextAsync("Max deposit must be less than 1,000,000.", new() { Timeout = 2000 });
        AssertNothingSent();

        await Input(m => m.MaxDeposit).FillAsync("999999");
        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor(m => m.MaxDeposit)).ToBeHiddenAsync(new() { Timeout = 2000 });
        await Expect(Result).ToHaveTextAsync("All new rule types passed!", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_admin_sets_status_to_deleted_notEqual_error_shows()
    {
        await NavigateAndBoot();
        await FillEveryRuleValidly();

        await Input(m => m.Status).FillAsync("deleted");
        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor(m => m.Status)).ToHaveTextAsync("Status must not be 'deleted'.", new() { Timeout = 2000 });
        await Expect(Result).ToHaveTextAsync("Click to validate");
        AssertNothingSent();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_admin_sets_status_to_active_notEqual_passes()
    {
        await NavigateAndBoot();
        await FillEveryRuleValidly();

        await Input(m => m.Status).FillAsync("deleted");
        await ClickWhenStable(ValidateBtn);
        await Expect(ErrorFor(m => m.Status)).ToHaveTextAsync("Status must not be 'deleted'.", new() { Timeout = 2000 });
        AssertNothingSent();

        await Input(m => m.Status).FillAsync("active");
        await Input(m => m.Status).BlurAsync();

        await Expect(ErrorFor(m => m.Status)).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_alternate_email_matches_primary_notEqualTo_error_shows()
    {
        await NavigateAndBoot();
        await FillEveryRuleValidly();

        await Input(m => m.Email).FillAsync("nurse@facility.com");
        await Input(m => m.AlternateEmail).FillAsync("nurse@facility.com");
        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor(m => m.AlternateEmail)).ToHaveTextAsync("Alternate email must differ from primary email.", new() { Timeout = 2000 });
        await Expect(Result).ToHaveTextAsync("Click to validate");
        AssertNothingSent();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_alternate_email_differs_from_primary_notEqualTo_passes()
    {
        await NavigateAndBoot();
        await FillEveryRuleValidly();

        await Input(m => m.Email).FillAsync("nurse@facility.com");
        await Input(m => m.AlternateEmail).FillAsync("nurse@facility.com");
        await ClickWhenStable(ValidateBtn);
        await Expect(ErrorFor(m => m.AlternateEmail)).ToHaveTextAsync("Alternate email must differ from primary email.", new() { Timeout = 2000 });
        AssertNothingSent();

        await Input(m => m.AlternateEmail).FillAsync("backup@facility.com");
        await Input(m => m.AlternateEmail).BlurAsync();

        await Expect(ErrorFor(m => m.AlternateEmail)).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_facility_website_is_not_a_url_error_shows()
    {
        await NavigateAndBoot();
        await FillEveryRuleValidly();

        await Input(m => m.Website).FillAsync("not-a-url");
        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor(m => m.Website)).ToHaveTextAsync("Website must be a valid URL.", new() { Timeout = 2000 });
        await Expect(Result).ToHaveTextAsync("Click to validate");
        AssertNothingSent();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_facility_website_is_valid_url_error_clears()
    {
        await NavigateAndBoot();
        await FillEveryRuleValidly();

        await Input(m => m.Website).FillAsync("not-a-url");
        await ClickWhenStable(ValidateBtn);
        await Expect(ErrorFor(m => m.Website)).ToHaveTextAsync("Website must be a valid URL.", new() { Timeout = 2000 });
        AssertNothingSent();

        await Input(m => m.Website).FillAsync("https://sunnyacres.com");
        await Input(m => m.Website).BlurAsync();

        await Expect(ErrorFor(m => m.Website)).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_nickname_field_has_value_empty_rule_error_shows()
    {
        await NavigateAndBoot();
        await FillEveryRuleValidly();

        await Input(m => m.Nickname).FillAsync("Maggie");
        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor(m => m.Nickname)).ToHaveTextAsync("Nickname must be empty.", new() { Timeout = 2000 });
        await Expect(Result).ToHaveTextAsync("Click to validate");
        AssertNothingSent();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_nickname_is_cleared_empty_rule_passes()
    {
        await NavigateAndBoot();
        await FillEveryRuleValidly();

        await Input(m => m.Nickname).FillAsync("Maggie");
        await ClickWhenStable(ValidateBtn);
        await Expect(ErrorFor(m => m.Nickname)).ToHaveTextAsync("Nickname must be empty.", new() { Timeout = 2000 });
        AssertNothingSent();

        await Input(m => m.Nickname).FillAsync("");
        await Input(m => m.Nickname).BlurAsync();

        await Expect(ErrorFor(m => m.Nickname)).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    private async Task FillEveryRuleValidly()
    {
        await Input(m => m.CardNumber).FillAsync("4111111111111111");
        await Input(m => m.Score).FillAsync("50");
        await Input(m => m.MonthlyRate).FillAsync("4250");
        await Input(m => m.MaxDeposit).FillAsync("500000");
        await Input(m => m.Status).FillAsync("active");
        await Input(m => m.Email).FillAsync("nurse@facility.com");
        await Input(m => m.AlternateEmail).FillAsync("backup@facility.com");
        await Input(m => m.Website).FillAsync("https://sunnyacres.com");
    }

    // The server binds an empty decimal as 0, which is outside (0, 100); the browser judges the same 0.
    [Test]
    public async Task a_score_left_empty_is_refused_as_the_zero_the_server_binds()
    {
        await NavigateAndBoot();
        await FillEveryRuleValidly();
        await Input(m => m.Score).FillAsync("");

        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor(m => m.Score)).ToHaveTextAsync("Score must be between 0 and 100 (exclusive).", new() { Timeout = 2000 });
        AssertNothingSent();
        await Expect(Result).ToHaveTextAsync("Click to validate");
        AssertNoConsoleErrors();
    }

    // The card rule drops dashes and spaces and refuses any other character that is not a digit.
    [Test]
    public async Task a_card_number_with_a_letter_in_it_is_refused_though_its_digits_check_out()
    {
        await NavigateAndBoot();
        await FillEveryRuleValidly();
        await Input(m => m.CardNumber).FillAsync("4111 1111 1111 111x1");

        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor(m => m.CardNumber)).ToHaveTextAsync("Card number is not valid.", new() { Timeout = 2000 });
        AssertNothingSent();
        await Expect(Result).ToHaveTextAsync("Click to validate");
        AssertNoConsoleErrors();
    }

    // The card rule sets no minimum length: a twelve-digit number that passes the Luhn check is a
    // card number to the server, so the browser lets it through and the server accepts it.
    [Test]
    public async Task a_twelve_digit_card_number_that_passes_the_check_is_accepted_by_the_server_too()
    {
        await NavigateAndBoot();
        await FillEveryRuleValidly();
        await Input(m => m.CardNumber).FillAsync("4242 4242 4242");

        await ClickWhenStable(ValidateBtn);

        await Expect(Result).ToHaveTextAsync("All new rule types passed!", new() { Timeout = 5000 });
        await Expect(ErrorFor(m => m.CardNumber)).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_all_fields_are_valid_form_passes()
    {
        await NavigateAndBoot();

        await Input(m => m.CardNumber).FillAsync("4111111111111111");
        await Input(m => m.Score).FillAsync("50");
        await Input(m => m.MonthlyRate).FillAsync("4250");
        await Input(m => m.MaxDeposit).FillAsync("500000");
        await Input(m => m.Status).FillAsync("active");
        await Input(m => m.Email).FillAsync("nurse@facility.com");
        await Input(m => m.AlternateEmail).FillAsync("backup@facility.com");
        await Input(m => m.Website).FillAsync("https://sunnyacres.com");

        await ClickWhenStable(ValidateBtn);

        await Expect(Result).ToHaveTextAsync("All new rule types passed!", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
