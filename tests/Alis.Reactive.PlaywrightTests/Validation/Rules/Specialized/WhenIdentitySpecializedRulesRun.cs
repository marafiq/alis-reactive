using Alis.Reactive.PlaywrightTests.Support.Pages;

namespace Alis.Reactive.PlaywrightTests.Validation.Rules.Specialized;

[TestFixture]
public sealed class WhenIdentitySpecializedRulesRun : PlaywrightTestBase
{
    private SpecializedValidationPage Rules => new(Page, () => NavigateToAndWaitForReady("/Sandbox/Validation/SpecializedRules"));

    [Test]
    public async Task fixed_value_rule_rejects_deleted_status()
    {
        await Rules.Open();

        await Rules.Input("Status").FillAsync("deleted");
        await ClickWhenStable(Rules.ValidateButton);

        await Expect(Rules.ErrorFor("Status")).ToContainTextAsync("must not be", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task fixed_value_rule_clears_for_allowed_status()
    {
        await Rules.Open();

        await Rules.Input("Status").FillAsync("deleted");
        await ClickWhenStable(Rules.ValidateButton);
        await Expect(Rules.ErrorFor("Status")).ToContainTextAsync("must not be", new() { Timeout = 2000 });

        await Rules.Input("Status").FillAsync("active");
        await Rules.Input("Status").BlurAsync();
        await Expect(Rules.ErrorFor("Status")).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task alternate_email_must_differ_from_primary_email()
    {
        await Rules.Open();

        await Rules.Input("Email").FillAsync("nurse@facility.com");
        await Rules.Input("AlternateEmail").FillAsync("nurse@facility.com");
        await ClickWhenStable(Rules.ValidateButton);

        await Expect(Rules.ErrorFor("AlternateEmail")).ToContainTextAsync("differ", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task alternate_email_error_clears_when_the_addresses_differ()
    {
        await Rules.Open();

        await Rules.Input("Email").FillAsync("nurse@facility.com");
        await Rules.Input("AlternateEmail").FillAsync("nurse@facility.com");
        await ClickWhenStable(Rules.ValidateButton);
        await Expect(Rules.ErrorFor("AlternateEmail")).ToContainTextAsync("differ", new() { Timeout = 2000 });

        await Rules.Input("AlternateEmail").FillAsync("backup@facility.com");
        await Rules.Input("AlternateEmail").BlurAsync();
        await Expect(Rules.ErrorFor("AlternateEmail")).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }
}
