namespace Alis.Reactive.PlaywrightTests.HttpPipeline.RealTime;

// Story: As a nurse watching the live facility-alert panel, I want a visible
// retry indicator when the live connection drops, so that I know the data may
// be stale and can restore live updates with one click.
//
// The retry indicator is the developer-owned layout element with the well-known
// container id; the runtime only toggles it and wires the click. Each page load
// is its own outage-drill world (the view scopes the stream and drill buttons by
// a per-render drill id), so these tests are hermetic: no shared server state,
// no cross-test or cross-session leakage, nothing to heal between tests.
// Connection loss is the boundary under test, so the runtime's fail-loud console
// error for the permanently dead stream is expected output in the drop tests.
[TestFixture]
public class WhenLiveConnectionDrops : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/RealTime";
    private const string PermanentDisconnectTrace = "[alis:server-push] connection.closed-permanent";

    private ILocator AlertStatus => Page.Locator("#alert-status");
    private ILocator AlertMessage => Page.Locator("#alert-message");
    private ILocator RetryIndicator => Page.Locator("#alis-realtime-connection-retry-container");
    private ILocator BreakButton => Page.Locator("#btn-break-stream");
    private ILocator RestoreButton => Page.Locator("#btn-restore-stream");
    private ILocator ResidentName => Page.Locator("#resident-name");
    private ILocator BreakHubButton => Page.Locator("#btn-break-hub");
    private ILocator RestoreHubButton => Page.Locator("#btn-restore-hub");
    private ILocator PushStatusButton => Page.Locator("#btn-push-status");

    [Test]
    public async Task retry_indicator_stays_hidden_while_live_updates_flow()
    {
        await NavigateAndSeeLiveAlerts();

        await Expect(RetryIndicator).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task losing_the_live_connection_shows_the_retry_indicator()
    {
        await NavigateAndSeeLiveAlerts();

        await ClickWhenStable(BreakButton);
        await WaitForTraceMessage(PermanentDisconnectTrace, 15000);

        await Expect(RetryIndicator).ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrorsExcept("connection.closed-permanent", "Failed to load resource");
    }

    [Test]
    public async Task clicking_the_retry_indicator_restores_live_updates_and_hides_it()
    {
        await NavigateAndSeeLiveAlerts();
        await ClickWhenStable(BreakButton);
        await WaitForTraceMessage(PermanentDisconnectTrace, 15000);
        await ClickWhenStable(RestoreButton);

        await ClickWhenStable(RetryIndicator);

        await Expect(AlertMessage).ToContainTextAsync("Facility check complete",
            new() { Timeout = 10000 });
        await Expect(RetryIndicator).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrorsExcept("connection.closed-permanent", "Failed to load resource");
    }

    [Test]
    public async Task clicking_retry_while_the_outage_continues_keeps_the_indicator_visible()
    {
        await NavigateAndSeeLiveAlerts();
        await ClickWhenStable(BreakButton);
        await WaitForTraceMessage(PermanentDisconnectTrace, 15000);

        await ClickWhenStable(RetryIndicator);

        // A retry against a still-broken server must never read as success: the
        // indicator stays visible through the attempt instead of blinking away.
        await Expect(RetryIndicator).ToBeVisibleAsync(new() { Timeout = 1000 });
        await WaitForTraceMessage("[alis:server-push] retry.manual", 10000);
        await Expect(RetryIndicator).ToBeVisibleAsync();

        await ClickWhenStable(RestoreButton);
        await ClickWhenStable(RetryIndicator);
        await Expect(RetryIndicator).ToBeHiddenAsync(new() { Timeout = 10000 });
        AssertNoConsoleErrorsExcept("connection.closed-permanent", "Failed to load resource");
    }

    // One test covers the full SignalR lifecycle: the client's automatic reconnect
    // schedule (~40s) must be exhausted before the indicator appears, and that wait
    // is paid once instead of once per criterion.
    [Test]
    public async Task losing_the_hub_connection_shows_the_retry_indicator_and_clicking_it_restores_live_updates()
    {
        await NavigateAndSeeLiveAlerts();

        // The client's reconnect schedule plus handshake overhead was observed at ~64s
        // in a real browser before onclose fires — 90s keeps headroom without masking.
        await ClickWhenStable(BreakHubButton);
        await Expect(RetryIndicator).ToBeVisibleAsync(new() { Timeout = 90000 });

        await ClickWhenStable(RestoreHubButton);
        await ClickWhenStable(RetryIndicator);
        await Expect(RetryIndicator).ToBeHiddenAsync(new() { Timeout = 15000 });

        // Round-trip proof the hub is live again: a manual push must land on the panel.
        // (Demo broadcasts are disabled under Playwright — tests control their own events.)
        await ClickWhenStable(PushStatusButton);
        await Expect(ResidentName).ToContainTextAsync("Helen Martinez (Manual)",
            new() { Timeout = 15000 });
        AssertNoConsoleErrorsExcept(
            "connection.disconnected", "start.failed", "WebSocket", "Failed to load resource");
    }

    private async Task NavigateAndSeeLiveAlerts()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(AlertStatus).ToContainTextAsync("Connected", new() { Timeout = 10000 });
    }
}
