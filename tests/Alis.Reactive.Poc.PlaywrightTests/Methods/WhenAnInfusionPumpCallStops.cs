namespace Alis.Reactive.Poc.PlaywrightTests.Methods;

[TestFixture]
public sealed class WhenAnInfusionPumpCallStops : AppPageTest
{
    [Test]
    public async Task a_declared_method_the_current_pump_has_runs()
    {
        await Open("/Pumps/Infusion");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Prime the pump" }).ClickAsync();

        await Expect(Page.Locator("#pump .pump-state")).ToHaveTextAsync("primed");
        await Expect(Page.Locator("#pump-status")).ToHaveTextAsync("Primed");
    }

    [Test]
    public async Task a_declared_method_the_older_pump_lacks_stops_the_pipeline_before_its_next_step()
    {
        await Open("/Pumps/Infusion");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Prime the older pump" }).ClickAsync();

        await Expect(Page.Locator("#pump-status")).ToHaveTextAsync("Calling");
        await Expect(Page.Locator("#older-pump .pump-state")).ToHaveTextAsync("idle");
    }

    [Test]
    public async Task a_declared_method_the_pump_holds_as_text_stops_the_pipeline_before_its_next_step()
    {
        await Open("/Pumps/Infusion");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Sound the alarm" }).ClickAsync();

        await Expect(Page.Locator("#pump-status")).ToHaveTextAsync("Calling");
    }

    [Test]
    public async Task a_method_declared_to_return_nothing_that_returns_a_promise_stops_the_pipeline_before_its_next_step()
    {
        await Open("/Pumps/Infusion");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Calibrate the pump" }).ClickAsync();

        await Expect(Page.Locator("#pump-status")).ToHaveTextAsync("Calling");
    }

    [Test]
    public async Task a_method_declared_to_return_a_value_that_returns_a_promise_stops_the_pipeline_before_it_writes_the_value()
    {
        await Open("/Pumps/Infusion");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Show the reading" }).ClickAsync();

        await Expect(Page.Locator("#pump-status")).ToHaveTextAsync("Calling");
        await Expect(Page.Locator("#census .census-charges")).ToHaveTextAsync("unknown");
    }

    [Test]
    public async Task a_returned_channel_that_lacks_a_declared_method_stops_the_pipeline_at_the_call()
    {
        await Open("/Pumps/Infusion");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Pause the older pump's second channel" }).ClickAsync();

        await Expect(Page.Locator("#pump-status")).ToHaveTextAsync("Calling");
        await Expect(Page.Locator("#older-pump .pump-channels li")).ToHaveTextAsync(["A: running", "B: running", "C: paused"]);
    }

    [Test]
    public async Task a_returned_channel_that_reads_nothing_stops_the_pipeline_at_its_declared_member()
    {
        await Open("/Pumps/Infusion");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Show the eighth channel" }).ClickAsync();

        await Expect(Page.Locator("#pump-status")).ToHaveTextAsync("Calling");
        await Expect(Page.Locator("#channel-label")).ToHaveTextAsync("unread");
    }
}
