namespace Alis.Reactive.Framework.PlaywrightTests.Vendors;

/// <summary>
/// Handlers of several owners assigned to one callback property (V2): the page's bell heard by the page and by
/// the wings loaded into two slots. Each handler runs after those assigned before it; a wing's handler stops
/// when its slot unloads while the page's and the bell's own keep running; and a wing released before one
/// assigned after it leaves that one listening.
/// </summary>
[TestFixture]
public sealed class WhenListenersShareACallbackProperty : AppPageTest
{
    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator Ring => Page.Locator("#bell").GetByRole(AriaRole.Button, new() { Name = "Ring the bell", Exact = true });

    private async Task Listen(string wing)
    {
        await Button($"Listen from the {wing} wing").ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = $"The {wing} wing is listening" })).ToBeVisibleAsync();
    }

    private async Task StopListening(string wing)
    {
        await Button($"Stop the {wing} wing listening").ClickAsync();
        await Expect(Page.Locator($"#{wing}-slot")).ToBeEmptyAsync();
    }

    [Test]
    public async Task a_handler_assigned_later_runs_after_the_handlers_assigned_before_it()
    {
        await Open("/Vendors/SharedCallback");
        await Listen("north");

        await Ring.ClickAsync();

        await Expect(Page.Locator("#page-heard")).ToHaveTextAsync("1");
        await Expect(Page.Locator("#last-handler")).ToHaveTextAsync("the north wing");
    }

    [Test]
    public async Task a_slots_handler_stops_when_its_slot_unloads_and_the_handlers_before_it_keep_running()
    {
        await Open("/Vendors/SharedCallback");
        await Listen("north");
        await Ring.ClickAsync();
        await Expect(Page.Locator("#north-heard")).ToHaveTextAsync("1");

        await StopListening("north");
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
        await Listen("north");
        await Listen("south");
        await Ring.ClickAsync();
        await Expect(Page.Locator("#south-heard")).ToHaveTextAsync("1");

        await StopListening("north");
        await Ring.ClickAsync();

        await Expect(Page.Locator("#page-heard")).ToHaveTextAsync("2");
        await Expect(Page.Locator("#south-heard")).ToHaveTextAsync("2");
        await Expect(Page.Locator("#north-heard")).ToHaveTextAsync("1");
    }
}
