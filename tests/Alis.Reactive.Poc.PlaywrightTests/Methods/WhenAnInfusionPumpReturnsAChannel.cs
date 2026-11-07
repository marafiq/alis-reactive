namespace Alis.Reactive.Poc.PlaywrightTests.Methods;

[TestFixture]
public sealed class WhenAnInfusionPumpReturnsAChannel : AppPageTest
{
    [Test]
    public async Task a_member_of_the_returned_channel_reads_as_the_channel_holds_it()
    {
        await Open("/Pumps/Infusion");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Show the first channel", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#channel-label")).ToHaveTextAsync("A");
    }

    [Test]
    public async Task a_channel_that_a_returned_channel_returns_reads_its_own_member()
    {
        await Open("/Pumps/Infusion");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Show the first channel's partner" }).ClickAsync();

        await Expect(Page.Locator("#channel-label")).ToHaveTextAsync("C");
    }

    [Test]
    public async Task each_channel_returns_its_own_partner_inside_a_lists_lambda()
    {
        await Open("/Pumps/Infusion");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Show each channel's partner" }).ClickAsync();

        await Expect(Page.Locator("#pump .pump-labels")).ToHaveTextAsync("C, A, B");
    }

    [Test]
    public async Task a_method_of_the_returned_channel_runs_on_that_channel()
    {
        await Open("/Pumps/Infusion");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Pause the second channel" }).ClickAsync();

        await Expect(Page.Locator("#pump .pump-channels li")).ToHaveTextAsync(["A: running", "B: paused", "C: paused"]);
        await Expect(Page.Locator("#pump-status")).ToHaveTextAsync("Paused");
    }

    [Test]
    public async Task each_returned_channel_calls_its_method_in_a_lists_lambda_as_it_is_when_the_step_runs()
    {
        await Open("/Pumps/Infusion");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count the running channels" }).ClickAsync();
        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Pause the second channel" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Count the running channels" }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("1");
    }
}
