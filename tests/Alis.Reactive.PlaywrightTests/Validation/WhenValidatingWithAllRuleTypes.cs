using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.Validation;

/// <summary>
/// Playwright tests for /Sandbox/Validation — verifies all rule types
/// and conditional validation work end-to-end in the browser.
/// </summary>
[TestFixture]
public class WhenValidatingWithAllRuleTypes : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation";
    private const string R = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ValidationShowcaseModel__";

    // Section 1: All Rule Types
    private ILocator AllRulesBtn => Page.Locator("#validate-all-btn");
    private ILocator AllRulesResult => Page.Locator("#all-rules-result");
    private ILocator Input(string suffix) => Page.Locator($"#{R}{suffix}");
    private ILocator ErrorFor(string suffix) => Page.Locator($"#{R}{suffix}_error");

    // Section 3: Conditional
    private ILocator ConditionalBtn => Page.Locator("#conditional-validate-btn");
    private ILocator ConditionalResult => Page.Locator("#conditional-result");

    // ── Section 1: All Rule Types ──────────────────────────

    [Test]
    public async Task empty_form_shows_required_errors()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await AllRulesBtn.ClickAsync();

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

        // Submit to trigger first validation
        await AllRulesBtn.ClickAsync();

        // Type too-short password, blur → minLength(8) error
        await Input("AllRules_Password").FillAsync("abc");
        await Input("AllRules_Password").BlurAsync();

        await Expect(ErrorFor("AllRules_Password")).ToContainTextAsync("at least 8", new() { Timeout = 2000 });

        // Fix it
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

        // Submit to trigger first validation
        await AllRulesBtn.ClickAsync();
        await Expect(ErrorFor("AllRules_Name")).ToContainTextAsync("required");

        // Type valid name, blur → error clears
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

        await AllRulesBtn.ClickAsync();
        await Expect(ErrorFor("AllRules_Email")).ToContainTextAsync("required");

        // Type invalid email, blur
        await Input("AllRules_Email").FillAsync("notanemail");
        await Input("AllRules_Email").BlurAsync();

        await Expect(ErrorFor("AllRules_Email")).ToContainTextAsync("valid email", new() { Timeout = 2000 });

        // Fix it
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

        await AllRulesBtn.ClickAsync();

        // Age has range(0,120) — type 150, blur
        await Input("AllRules_Age").FillAsync("150");
        await Input("AllRules_Age").BlurAsync();

        await Expect(ErrorFor("AllRules_Age")).ToContainTextAsync("between", new() { Timeout = 2000 });

        // Fix it
        await Input("AllRules_Age").FillAsync("75");
        await Input("AllRules_Age").BlurAsync();

        await Expect(ErrorFor("AllRules_Age")).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    // ── Section 3: Conditional Rules ──────────────────────

    [Test]
    public async Task conditional_rule_skipped_when_unchecked()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // IsEmployed unchecked, submit → JobTitle should NOT show error
        await ConditionalBtn.ClickAsync();

        await Expect(ConditionalResult).ToContainTextAsync("passed", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task conditional_rule_enforced_when_checked()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Check IsEmployed
        await Input("Conditional_IsEmployed").CheckAsync();

        // Submit → JobTitle should show required error
        await ConditionalBtn.ClickAsync();

        await Expect(ErrorFor("Conditional_JobTitle")).ToContainTextAsync("required", new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task conditional_rule_clears_when_condition_becomes_false()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Check → submit → error shows
        await Input("Conditional_IsEmployed").CheckAsync();
        await ConditionalBtn.ClickAsync();
        await Expect(ErrorFor("Conditional_JobTitle")).ToContainTextAsync("required", new() { Timeout = 2000 });

        // Uncheck → submit again → error should clear (condition false → rule skipped)
        await Input("Conditional_IsEmployed").UncheckAsync();
        await ConditionalBtn.ClickAsync();

        await Expect(ConditionalResult).ToContainTextAsync("passed", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task conditional_rule_passes_when_filled()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Check + fill + submit → should pass
        await Input("Conditional_IsEmployed").CheckAsync();
        await Input("Conditional_JobTitle").FillAsync("Care Coordinator");
        await ConditionalBtn.ClickAsync();

        await Expect(ConditionalResult).ToContainTextAsync("passed", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
