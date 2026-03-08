namespace Alis.Reactive.PlaywrightTests.Conditions;

[TestFixture]
public class WhenConditionsEvaluateInBrowser : PlaywrightTestBase
{
    [Test]
    public async Task When_then_else_takes_correct_branch()
    {
        await NavigateTo("/Sandbox/Conditions");
        await WaitForTraceMessage("booted", 5000);

        var grade = Page.Locator("#grade");
        var scoreDisplay = Page.Locator("#score-display");

        // Click Score 95 → grade should be "A"
        await Page.Locator("#btn-high").ClickAsync();
        await Expect(grade).ToHaveTextAsync("A");
        await Expect(scoreDisplay).ToHaveTextAsync("95");

        // Click Score 85 → grade should be "B"
        await Page.Locator("#btn-mid").ClickAsync();
        await Expect(grade).ToHaveTextAsync("B");
        await Expect(scoreDisplay).ToHaveTextAsync("85");

        // Click Score 40 → grade should be "F"
        await Page.Locator("#btn-low").ClickAsync();
        await Expect(grade).ToHaveTextAsync("F");
        await Expect(scoreDisplay).ToHaveTextAsync("40");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task And_condition_requires_both_guards()
    {
        await NavigateTo("/Sandbox/Conditions");
        await WaitForTraceMessage("booted", 5000);

        var result = Page.Locator("#and-result");

        // Score 95 + Active → both guards pass → "Active High Scorer"
        await Page.Locator("#btn-and-pass").ClickAsync();
        await Expect(result).ToHaveTextAsync("Active High Scorer");

        // Score 95 + Inactive → status guard fails → "Nope"
        await Page.Locator("#btn-and-fail").ClickAsync();
        await Expect(result).ToHaveTextAsync("Nope");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Or_condition_requires_any_guard()
    {
        await NavigateTo("/Sandbox/Conditions");
        await WaitForTraceMessage("booted", 5000);

        var result = Page.Locator("#or-result");

        // Admin → "Authorized"
        await Page.Locator("#btn-admin").ClickAsync();
        await Expect(result).ToHaveTextAsync("Authorized");

        // Superuser → "Authorized"
        await Page.Locator("#btn-superuser").ClickAsync();
        await Expect(result).ToHaveTextAsync("Authorized");

        // Viewer → "Denied"
        await Page.Locator("#btn-viewer").ClickAsync();
        await Expect(result).ToHaveTextAsync("Denied");

        AssertNoConsoleErrors();
    }
}
