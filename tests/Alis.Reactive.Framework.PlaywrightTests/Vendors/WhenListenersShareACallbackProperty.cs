namespace Alis.Reactive.Framework.PlaywrightTests.Vendors;

[TestFixture]
public sealed class WhenListenersShareACallbackProperty : AppPageTest
{
    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator Ring => Page.Locator("#bell").GetByRole(AriaRole.Button, new() { Name = "Ring the bell", Exact = true });

    [Test]
    public async Task a_handler_assigned_later_runs_after_the_handlers_assigned_before_it()
    {
        await Open("/Vendors/SharedCallback");
        await Button("Listen from the north wing").ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "The north wing is listening" })).ToBeVisibleAsync();

        await Ring.ClickAsync();

        await Expect(Page.Locator("#page-heard")).ToHaveTextAsync("1");
        await Expect(Page.Locator("#last-handler")).ToHaveTextAsync("the north wing");
    }

    [Test]
    public async Task a_slots_handler_stops_when_its_slot_unloads_and_the_handlers_before_it_keep_running()
    {
        await Open("/Vendors/SharedCallback");
        await Button("Listen from the north wing").ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "The north wing is listening" })).ToBeVisibleAsync();
        await Ring.ClickAsync();
        await Expect(Page.Locator("#north-heard")).ToHaveTextAsync("1");

        await Button("Stop the north wing listening").ClickAsync();
        await Expect(Page.Locator("#north-slot")).ToBeEmptyAsync();
        await Ring.ClickAsync();

        await Expect(Page.Locator("#page-heard")).ToHaveTextAsync("2");
        await Expect(Page.Locator("#bell").GetByText("Rung 2 times")).ToBeVisibleAsync();
        await Expect(Page.Locator("#north-heard")).ToHaveTextAsync("1");
        await Expect(Page.Locator("#last-handler")).ToHaveTextAsync("the page");
    }

    [Test]
    public async Task a_handler_released_before_one_assigned_after_it_leaves_that_one_listening()
    {
        await Open("/Vendors/SharedCallback");
        await Button("Listen from the north wing").ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "The north wing is listening" })).ToBeVisibleAsync();
        await Button("Listen from the south wing").ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "The south wing is listening" })).ToBeVisibleAsync();
        await Ring.ClickAsync();
        await Expect(Page.Locator("#south-heard")).ToHaveTextAsync("1");

        await Button("Stop the north wing listening").ClickAsync();
        await Expect(Page.Locator("#north-slot")).ToBeEmptyAsync();
        await Ring.ClickAsync();

        await Expect(Page.Locator("#page-heard")).ToHaveTextAsync("2");
        await Expect(Page.Locator("#south-heard")).ToHaveTextAsync("2");
        await Expect(Page.Locator("#north-heard")).ToHaveTextAsync("1");
    }
}
