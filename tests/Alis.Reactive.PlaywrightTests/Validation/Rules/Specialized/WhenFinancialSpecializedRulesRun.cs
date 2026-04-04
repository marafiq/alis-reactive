using Alis.Reactive.PlaywrightTests.Support.Pages;

namespace Alis.Reactive.PlaywrightTests.Validation.Rules.Specialized;

[TestFixture]
public sealed class WhenFinancialSpecializedRulesRun : PlaywrightTestBase
{
    private SpecializedValidationPage Rules => new(Page, () => NavigateToAndWaitForReady("/Sandbox/Validation/SpecializedRules"));

    [Test]
    public async Task invalid_card_number_shows_an_error()
    {
        await Rules.Open();

        await Rules.Input("CardNumber").FillAsync("1234567890123");
        await ClickWhenStable(Rules.ValidateButton);

        await Expect(Rules.ErrorFor("CardNumber")).ToContainTextAsync("not valid", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task valid_card_number_clears_the_error()
    {
        await Rules.Open();

        await Rules.Input("CardNumber").FillAsync("1234567890123");
        await ClickWhenStable(Rules.ValidateButton);
        await Expect(Rules.ErrorFor("CardNumber")).ToContainTextAsync("not valid", new() { Timeout = 2000 });

        await Rules.Input("CardNumber").FillAsync("4111111111111111");
        await Rules.Input("CardNumber").BlurAsync();
        await Expect(Rules.ErrorFor("CardNumber")).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task exclusive_range_rejects_the_lower_boundary()
    {
        await Rules.Open();

        await Rules.Input("Score").FillAsync("0");
        await ClickWhenStable(Rules.ValidateButton);

        await Expect(Rules.ErrorFor("Score")).ToContainTextAsync("exclusive", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task exclusive_range_rejects_the_upper_boundary()
    {
        await Rules.Open();

        await Rules.Input("Score").FillAsync("100");
        await ClickWhenStable(Rules.ValidateButton);

        await Expect(Rules.ErrorFor("Score")).ToContainTextAsync("exclusive", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task exclusive_range_clears_after_a_valid_score()
    {
        await Rules.Open();

        await Rules.Input("Score").FillAsync("0");
        await ClickWhenStable(Rules.ValidateButton);
        await Expect(Rules.ErrorFor("Score")).ToContainTextAsync("exclusive", new() { Timeout = 2000 });

        await Rules.Input("Score").FillAsync("50");
        await Rules.Input("Score").BlurAsync();
        await Expect(Rules.ErrorFor("Score")).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task greater_than_rule_rejects_zero_and_empty_values()
    {
        await Rules.Open();

        await Rules.Input("MonthlyRate").FillAsync("0");
        await ClickWhenStable(Rules.ValidateButton);
        await Expect(Rules.ErrorFor("MonthlyRate")).ToContainTextAsync("greater than zero", new() { Timeout = 2000 });

        await Rules.Input("MonthlyRate").ClearAsync();
        await ClickWhenStable(Rules.ValidateButton);
        await Expect(Rules.ErrorFor("MonthlyRate")).ToContainTextAsync("greater than zero", new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task greater_than_rule_passes_for_positive_values()
    {
        await Rules.Open();

        await Rules.Input("MonthlyRate").FillAsync("0");
        await ClickWhenStable(Rules.ValidateButton);
        await Expect(Rules.ErrorFor("MonthlyRate")).ToContainTextAsync("greater than zero", new() { Timeout = 2000 });

        await Rules.Input("MonthlyRate").FillAsync("4250");
        await Rules.Input("MonthlyRate").BlurAsync();
        await Expect(Rules.ErrorFor("MonthlyRate")).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task less_than_rule_rejects_the_limit_value()
    {
        await Rules.Open();

        await Rules.Input("MaxDeposit").FillAsync("1000000");
        await ClickWhenStable(Rules.ValidateButton);

        await Expect(Rules.ErrorFor("MaxDeposit")).ToContainTextAsync("less than", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task less_than_rule_passes_below_the_limit()
    {
        await Rules.Open();

        await Rules.Input("MaxDeposit").FillAsync("1000000");
        await ClickWhenStable(Rules.ValidateButton);
        await Expect(Rules.ErrorFor("MaxDeposit")).ToContainTextAsync("less than", new() { Timeout = 2000 });

        await Rules.Input("MaxDeposit").FillAsync("999999");
        await Rules.Input("MaxDeposit").BlurAsync();
        await Expect(Rules.ErrorFor("MaxDeposit")).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }
}
