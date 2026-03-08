namespace Alis.Reactive.PlaywrightTests.Conditions;

[TestFixture]
public class WhenConditionsEvaluateInBrowser : PlaywrightTestBase
{
    private async Task NavigateAndBoot()
    {
        await NavigateTo("/Sandbox/Conditions");
        await WaitForTraceMessage("booted", 5000);
    }

    // ── int (ElseIf grade ladder) ──

    [Test]
    public async Task Int_elseif_takes_correct_branch()
    {
        await NavigateAndBoot();
        var grade = Page.Locator("#grade");

        await Page.Locator("#btn-score-95").ClickAsync();
        await Expect(grade).ToHaveTextAsync("A");

        await Page.Locator("#btn-score-85").ClickAsync();
        await Expect(grade).ToHaveTextAsync("B");

        await Page.Locator("#btn-score-40").ClickAsync();
        await Expect(grade).ToHaveTextAsync("F");

        AssertNoConsoleErrors();
    }

    // ── long ──

    [Test]
    public async Task Long_gt_threshold()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#long-result");

        await Page.Locator("#btn-long-high").ClickAsync();
        await Expect(result).ToHaveTextAsync("High Value");

        await Page.Locator("#btn-long-low").ClickAsync();
        await Expect(result).ToHaveTextAsync("Standard");

        AssertNoConsoleErrors();
    }

    // ── double ──

    [Test]
    public async Task Double_gt_comparison()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#double-result");

        await Page.Locator("#btn-double-high").ClickAsync();
        await Expect(result).ToHaveTextAsync("Fever");

        await Page.Locator("#btn-double-low").ClickAsync();
        await Expect(result).ToHaveTextAsync("Normal");

        AssertNoConsoleErrors();
    }

    // ── bool ──

    [Test]
    public async Task Bool_truthy_falsy()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#bool-result");

        await Page.Locator("#btn-bool-true").ClickAsync();
        await Expect(result).ToHaveTextAsync("Online");

        await Page.Locator("#btn-bool-false").ClickAsync();
        await Expect(result).ToHaveTextAsync("Offline");

        AssertNoConsoleErrors();
    }

    // ── string ──

    [Test]
    public async Task String_eq_comparison()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#string-result");

        await Page.Locator("#btn-string-match").ClickAsync();
        await Expect(result).ToHaveTextAsync("Welcome Alice!");

        await Page.Locator("#btn-string-miss").ClickAsync();
        await Expect(result).ToHaveTextAsync("Hello Stranger");

        AssertNoConsoleErrors();
    }

    // ── DateTime ──

    [Test]
    public async Task DateTime_gt_comparison()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#date-result");

        await Page.Locator("#btn-date-future").ClickAsync();
        await Expect(result).ToHaveTextAsync("On Time");

        await Page.Locator("#btn-date-past").ClickAsync();
        await Expect(result).ToHaveTextAsync("Overdue");

        AssertNoConsoleErrors();
    }

    // ── int? (nullable) ──

    [Test]
    public async Task Nullable_int_is_null()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#nullable-result");

        await Page.Locator("#btn-nullable-null").ClickAsync();
        await Expect(result).ToHaveTextAsync("No Score");

        await Page.Locator("#btn-nullable-value").ClickAsync();
        await Expect(result).ToHaveTextAsync("Has Score");

        AssertNoConsoleErrors();
    }

    // ── AND (int + string) ──

    [Test]
    public async Task And_mixed_types()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#and-result");

        await Page.Locator("#btn-and-pass").ClickAsync();
        await Expect(result).ToHaveTextAsync("Active High Scorer");

        await Page.Locator("#btn-and-fail").ClickAsync();
        await Expect(result).ToHaveTextAsync("Nope");

        AssertNoConsoleErrors();
    }

    // ── OR (string alternatives) ──

    [Test]
    public async Task Or_string_alternatives()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#or-result");

        await Page.Locator("#btn-or-admin").ClickAsync();
        await Expect(result).ToHaveTextAsync("Authorized");

        await Page.Locator("#btn-or-super").ClickAsync();
        await Expect(result).ToHaveTextAsync("Authorized");

        await Page.Locator("#btn-or-viewer").ClickAsync();
        await Expect(result).ToHaveTextAsync("Denied");

        AssertNoConsoleErrors();
    }
}
