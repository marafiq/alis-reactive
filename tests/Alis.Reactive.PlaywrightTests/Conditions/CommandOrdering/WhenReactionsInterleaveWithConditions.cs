namespace Alis.Reactive.PlaywrightTests.Conditions.CommandOrdering;

// Post-condition reactions must run after the selected condition branch.
[TestFixture]
public class WhenReactionsInterleaveWithConditions : PlaywrightTestBase
{
    private async Task NavigateAndBoot()
    {
        await NavigateTo("/Sandbox/Conditions/CommandOrdering");
        await WaitForTraceMessage("booted", 10000);
    }

    [Test]
    public async Task post_condition_reaction_overwrites_conditional_text_when_condition_matches()
    {
        await NavigateAndBoot();

        await Page.Locator("#btn-high").ClickAsync();

        await Expect(Page.Locator("#proof")).ToHaveTextAsync("after", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task post_condition_reaction_fires_even_when_condition_does_not_match()
    {
        await NavigateAndBoot();

        await Page.Locator("#btn-low").ClickAsync();

        await Expect(Page.Locator("#proof")).ToHaveTextAsync("after", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
