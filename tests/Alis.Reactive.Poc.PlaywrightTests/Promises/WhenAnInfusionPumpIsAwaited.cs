namespace Alis.Reactive.Poc.PlaywrightTests.Promises;

[TestFixture]
public sealed class WhenAnInfusionPumpIsAwaited : AppPageTest
{
    [Test]
    public async Task the_step_after_an_async_method_runs_only_once_its_promise_resolves()
    {
        await Open("/Pumps/Checks");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Run the self-test" }).ClickAsync();
        await Expect(Page.Locator("#pump .pump-check")).ToHaveTextAsync("checking");
        await Expect(Page.Locator("#check-status")).ToHaveTextAsync("Testing");

        await Page.Locator("#pump").GetByRole(AriaRole.Button, new() { Name = "Finish the check" }).ClickAsync();

        await Expect(Page.Locator("#check-status")).ToHaveTextAsync("Self-test passed");
    }

    [Test]
    public async Task a_rejected_promise_stops_the_pipeline_before_its_next_step()
    {
        await Open("/Pumps/Checks");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Run the self-test" }).ClickAsync();
        await Page.Locator("#pump").GetByRole(AriaRole.Button, new() { Name = "Fail the check" }).ClickAsync();

        await Expect(Page.Locator("#pump .pump-check")).ToHaveTextAsync("failed");
        await Expect(Page.Locator("#check-status")).ToHaveTextAsync("Testing");
    }

    [Test]
    public async Task an_async_methods_result_is_the_value_its_promise_resolves_to()
    {
        await Open("/Pumps/Checks");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Measure the volume" }).ClickAsync();
        await Expect(Page.Locator("#check-status")).ToHaveTextAsync("Measuring");
        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("unknown");

        await Page.Locator("#pump").GetByRole(AriaRole.Button, new() { Name = "Finish the check" }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("125");
        await Expect(Page.Locator("#check-status")).ToHaveTextAsync("Measured");
    }

    [Test]
    public async Task a_method_declared_async_whose_pump_returns_a_plain_value_is_awaited_as_that_value()
    {
        await Open("/Pumps/Checks");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Measure the older pump's volume" }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("90");
        await Expect(Page.Locator("#check-status")).ToHaveTextAsync("Older measured");
    }

    [Test]
    public async Task an_async_result_that_resolves_to_nothing_reaches_the_method_as_null()
    {
        await Open("/Pumps/Checks");
        await Expect(Page.Locator("#pump .pump-caption")).ToHaveTextAsync("Bed 4 pump");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Caption with the last check" }).ClickAsync();

        await Expect(Page.Locator("#pump .pump-caption")).ToHaveTextAsync("(no caption)");
    }

    [Test]
    public async Task a_member_of_the_data_an_async_method_resolves_to_reads_past_the_result()
    {
        await Open("/Pumps/Checks");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Show the remote volume limit" }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("750");
    }

    [Test]
    public async Task two_handlers_of_one_click_run_in_order_to_the_first_await_and_the_first_goes_on_when_its_promise_resolves()
    {
        await Open("/Pumps/Checks");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Note both handlers" }).ClickAsync();
        await Expect(Page.Locator("#pump .pump-notes")).ToHaveTextAsync("first before, second");

        await Page.Locator("#pump").GetByRole(AriaRole.Button, new() { Name = "Finish the check" }).ClickAsync();

        await Expect(Page.Locator("#pump .pump-notes")).ToHaveTextAsync("first before, second, first after");
    }
}
