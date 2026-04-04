using Alis.Reactive.PlaywrightTests.Support.Pages;

namespace Alis.Reactive.PlaywrightTests.Validation.Rules.AllRules;

[TestFixture]
public sealed class WhenConditionalEmploymentRulesRun : PlaywrightTestBase
{
    private ValidationShowcasePage Showcase => new(Page, () => NavigateToAndWaitForReady("/Sandbox/Validation/AllRules"));

    [Test]
    public async Task job_title_rule_is_skipped_when_employment_is_unchecked()
    {
        await Showcase.Open();

        await ClickWhenStable(Showcase.ValidateConditionalButton);
        await Expect(Showcase.ConditionalResult).ToContainTextAsync("passed", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task job_title_rule_is_enforced_when_employment_is_checked()
    {
        await Showcase.Open();

        await Showcase.Input("Conditional_IsEmployed").CheckAsync();
        await ClickWhenStable(Showcase.ValidateConditionalButton);

        await Expect(Showcase.ErrorFor("Conditional_JobTitle")).ToContainTextAsync("required", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task job_title_error_clears_when_employment_becomes_false_again()
    {
        await Showcase.Open();

        await Showcase.Input("Conditional_IsEmployed").CheckAsync();
        await ClickWhenStable(Showcase.ValidateConditionalButton);
        await Expect(Showcase.ErrorFor("Conditional_JobTitle")).ToContainTextAsync("required", new() { Timeout = 2000 });

        await Showcase.Input("Conditional_IsEmployed").UncheckAsync();
        await ClickWhenStable(Showcase.ValidateConditionalButton);

        await Expect(Showcase.ConditionalResult).ToContainTextAsync("passed", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task job_title_rule_passes_when_the_field_is_filled()
    {
        await Showcase.Open();

        await Showcase.Input("Conditional_IsEmployed").CheckAsync();
        await Showcase.Input("Conditional_JobTitle").FillAsync("Care Coordinator");
        await ClickWhenStable(Showcase.ValidateConditionalButton);

        await Expect(Showcase.ConditionalResult).ToContainTextAsync("passed", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task job_title_error_live_clears_after_valid_entry()
    {
        await Showcase.Open();

        await Showcase.Input("Conditional_IsEmployed").CheckAsync();
        await ClickWhenStable(Showcase.ValidateConditionalButton);
        await Expect(Showcase.ErrorFor("Conditional_JobTitle")).ToContainTextAsync("required", new() { Timeout = 2000 });

        await Showcase.Input("Conditional_JobTitle").FillAsync("Activities Director");
        await Showcase.Input("Conditional_JobTitle").BlurAsync();

        await Expect(Showcase.ErrorFor("Conditional_JobTitle")).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }
}
