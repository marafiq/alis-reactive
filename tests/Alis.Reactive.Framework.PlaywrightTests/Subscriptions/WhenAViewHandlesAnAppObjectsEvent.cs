namespace Alis.Reactive.Framework.PlaywrightTests.Subscriptions;

[TestFixture]
public sealed class WhenAViewHandlesAnAppObjectsEvent : AppPageTest
{
    private ILocator PageHeard => Page.Locator("#page-heard");
    private ILocator DeskHeard => Page.Locator("#desk-heard");
    private ILocator Desk => Page.Locator("#noticeboard-desk");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    [Test]
    public async Task the_page_runs_its_handler_when_the_app_object_raises_the_event()
    {
        await Open("/Subscriptions/Noticeboard");

        await Button("Flash the banner").ClickAsync();

        await Expect(PageHeard).ToHaveTextAsync("1");
    }

    [Test]
    public async Task a_partial_in_a_slot_runs_its_handler_while_it_is_on_the_page()
    {
        await Open("/Subscriptions/Noticeboard");
        await Button("Open the desk").ClickAsync();
        await Expect(Desk).ToBeVisibleAsync();

        await Button("Flash the banner").ClickAsync();

        await Expect(DeskHeard).ToHaveTextAsync("1");
    }

    [Test]
    public async Task a_partial_stops_handling_the_event_once_its_slot_unloads()
    {
        await Open("/Subscriptions/Noticeboard");
        await Button("Open the desk").ClickAsync();
        await Expect(Desk).ToBeVisibleAsync();
        await Button("Flash the banner").ClickAsync();
        await Expect(DeskHeard).ToHaveTextAsync("1");
        await Button("Close the desk").ClickAsync();
        await Expect(Desk).Not.ToBeAttachedAsync();

        await Button("Flash the banner").ClickAsync();

        await Expect(PageHeard).ToHaveTextAsync("2");
        await Expect(DeskHeard).ToHaveTextAsync("1");
    }
}
