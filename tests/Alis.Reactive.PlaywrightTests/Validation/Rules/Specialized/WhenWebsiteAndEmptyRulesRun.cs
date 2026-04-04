using Alis.Reactive.PlaywrightTests.Support.Controls;
using Alis.Reactive.PlaywrightTests.Support.Pages;

namespace Alis.Reactive.PlaywrightTests.Validation.Rules.Specialized;

[TestFixture]
public sealed class WhenWebsiteAndEmptyRulesRun : PlaywrightTestBase
{
    private SpecializedValidationPage Rules => new(Page, () => NavigateToAndWaitForReady("/Sandbox/Validation/SpecializedRules"));

    [Test]
    public async Task invalid_website_shows_an_error()
    {
        await Rules.Open();

        await Rules.Input("Website").FillAsync("not-a-url");
        await ClickWhenStable(Rules.ValidateButton);

        await Expect(Rules.ErrorFor("Website")).ToContainTextAsync("valid URL", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task valid_website_clears_the_error()
    {
        await Rules.Open();

        await Rules.Input("Website").FillAsync("not-a-url");
        await ClickWhenStable(Rules.ValidateButton);
        await Expect(Rules.ErrorFor("Website")).ToContainTextAsync("valid URL", new() { Timeout = 2000 });

        await Rules.Input("Website").FillAsync("https://sunnyacres.com");
        await Rules.Input("Website").BlurAsync();
        await Expect(Rules.ErrorFor("Website")).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task empty_rule_rejects_a_nickname_value()
    {
        await Rules.Open();

        await Rules.Input("Nickname").FillAsync("Maggie");
        await ClickWhenStable(Rules.ValidateButton);

        await Expect(Rules.ErrorFor("Nickname")).ToContainTextAsync("must be empty", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task empty_rule_passes_after_the_nickname_is_cleared()
    {
        await Rules.Open();

        await Rules.Input("Nickname").FillAsync("Maggie");
        await ClickWhenStable(Rules.ValidateButton);
        await Expect(Rules.ErrorFor("Nickname")).ToContainTextAsync("must be empty", new() { Timeout = 2000 });

        await Rules.Input("Nickname").ClearWhenStableAsync();
        await Rules.Input("Nickname").BlurAsync();
        await Expect(Rules.ErrorFor("Nickname")).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task fully_valid_specialized_form_shows_success()
    {
        await Rules.Open();

        await Rules.Input("CardNumber").FillAsync("4111111111111111");
        await Rules.Input("Score").FillAsync("50");
        await Rules.Input("MonthlyRate").FillAsync("4250");
        await Rules.Input("MaxDeposit").FillAsync("500000");
        await Rules.Input("Status").FillAsync("active");
        await Rules.Input("Email").FillAsync("nurse@facility.com");
        await Rules.Input("AlternateEmail").FillAsync("backup@facility.com");
        await Rules.Input("Website").FillAsync("https://sunnyacres.com");

        await ClickWhenStable(Rules.ValidateButton);

        await Expect(Rules.Result).ToContainTextAsync("passed", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
