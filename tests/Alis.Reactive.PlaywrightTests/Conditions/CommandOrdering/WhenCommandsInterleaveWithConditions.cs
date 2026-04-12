namespace Alis.Reactive.PlaywrightTests.Conditions.CommandOrdering;

/// <summary>
/// Proves that commands execute in declaration order around conditions.
/// A post-condition command must fire AFTER the condition branch, not before.
/// </summary>
[TestFixture]
public class WhenCommandsInterleaveWithConditions : PlaywrightTestBase
{
    private async Task NavigateAndBoot()
    {
        await NavigateTo("/Sandbox/Conditions/CommandOrdering");
        await WaitForTraceMessage("booted", 10000);
    }

    [Test]
    public async Task post_condition_command_overwrites_conditional_text_when_condition_matches()
    {
        // Pipeline: SetText("before") → When(level=="high").Then(SetText("conditional")) → SetText("after")
        // Correct ordering: before → conditional → after → proof shows "after"
        // Buggy ordering:   before → after → conditional → proof shows "conditional"
        await NavigateAndBoot();

        await Page.Locator("#btn-high").ClickAsync();

        // "after" must be the final value — it fires AFTER the condition
        await Expect(Page.Locator("#proof")).ToHaveTextAsync("after", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task post_condition_command_fires_even_when_condition_does_not_match()
    {
        // Pipeline: SetText("before") → When(level=="high").Then(...) → SetText("after")
        // With level="low", condition doesn't fire — proof should still show "after"
        await NavigateAndBoot();

        await Page.Locator("#btn-low").ClickAsync();

        await Expect(Page.Locator("#proof")).ToHaveTextAsync("after", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
