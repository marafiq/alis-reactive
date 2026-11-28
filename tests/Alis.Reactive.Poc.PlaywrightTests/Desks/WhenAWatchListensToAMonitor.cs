namespace Alis.Reactive.Poc.PlaywrightTests.Desks;

[TestFixture]
public sealed class WhenAWatchListensToAMonitor : AppPageTest
{
    private const string StationHeard = "The station heard an alarm";
    private const string WatchHeard = "The watch heard an alarm";

    [Test]
    public async Task a_watch_hears_the_monitor_its_desk_handed_over()
    {
        await Open("/Desks/Station");
        await Click("Open the watch");
        await Expect(Page.Locator("#watch h2")).ToHaveTextAsync("Watch");
        await Click("Pick ada-monitor");

        await Click("Sound Ada's monitor");

        await Expect(Page.Locator("#alarm-from")).ToHaveTextAsync("Ada");
        await Expect(Page.Locator("#calls li")).ToHaveTextAsync([StationHeard, WatchHeard]);
    }

    [Test]
    public async Task a_closed_watch_hears_no_more_alarms()
    {
        await Open("/Desks/Station");
        await Click("Open the watch");
        await Expect(Page.Locator("#watch h2")).ToHaveTextAsync("Watch");
        await Click("Pick ada-monitor");
        await Click("Sound Ada's monitor");
        await Expect(Page.Locator("#calls li")).ToHaveTextAsync([StationHeard, WatchHeard]);
        await Click("Close the watch");
        await Expect(Page.Locator("#watch-state")).ToHaveTextAsync("Watch closed");

        await Click("Sound Ada's monitor");

        await Expect(Page.Locator("#calls li")).ToHaveTextAsync([StationHeard, WatchHeard, StationHeard]);
    }

    [Test]
    public async Task a_watch_whose_monitor_was_switched_off_closes()
    {
        await Open("/Desks/Station");
        await Click("Open the watch");
        await Expect(Page.Locator("#watch h2")).ToHaveTextAsync("Watch");
        await Click("Pick ada-monitor");
        await Click("Switch off Ada's monitor");
        await Expect(Page.Locator("#ada-monitor .desk-monitor-status")).ToHaveTextAsync("Ada: switched off");

        await Click("Close the watch");

        await Expect(Page.Locator("#watch-state")).ToHaveTextAsync("Watch closed");
        await Expect(Page.Locator("#watch")).ToBeEmptyAsync();
    }

    [Test]
    public async Task a_watch_whose_monitor_was_restarted_closes()
    {
        await Open("/Desks/Station");
        await Click("Open the watch");
        await Expect(Page.Locator("#watch h2")).ToHaveTextAsync("Watch");
        await Click("Pick ada-monitor");
        await Click("Restart Ada's monitor");
        await Expect(Page.Locator("#ada-monitor .desk-monitor-status")).ToHaveTextAsync("Ada: idle again");

        await Click("Close the watch");

        await Expect(Page.Locator("#watch-state")).ToHaveTextAsync("Watch closed");
        await Expect(Page.Locator("#watch")).ToBeEmptyAsync();
    }

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();
}
