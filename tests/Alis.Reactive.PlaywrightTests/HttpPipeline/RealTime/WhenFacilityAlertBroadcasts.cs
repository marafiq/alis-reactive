namespace Alis.Reactive.PlaywrightTests.HttpPipeline.RealTime;

[TestFixture]
public class WhenFacilityAlertBroadcasts : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/RealTime";

    private ILocator AlertMessage => Page.Locator("#alert-message");
    private ILocator AlertLevel => Page.Locator("#alert-level");
    private ILocator AlertStatus => Page.Locator("#alert-status");

    [Test]
    public async Task facility_alert_updates_the_alert_panel()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);

        await Expect(AlertMessage).ToContainTextAsync("Facility check complete",
            new() { Timeout = 10000 });
        await Expect(AlertLevel).ToContainTextAsync("info");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sounding_the_all_clear_tells_the_panel_nothing_needs_action()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(AlertMessage).ToContainTextAsync("Facility check complete",
            new() { Timeout = 10000 });

        await Page.Locator("#btn-all-clear").ClickAsync();

        await Expect(AlertLevel).ToHaveTextAsync("clear", new() { Timeout = 5000 });
        await Expect(AlertMessage).ToHaveTextAsync("All clear: the facility check found nothing to act on.");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task alert_status_turns_green_when_connected()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);

        await Expect(AlertStatus).ToContainTextAsync("Connected",
            new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sse_connection_appears_in_trace()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await WaitForTraceMessage("[alis:server-push] connected", 10000);

        AssertTraceContains("server-push", "connected");
        AssertNoConsoleErrors();
    }
}
