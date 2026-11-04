namespace Alis.Reactive.PlaywrightTests.HttpPipeline.RealTime;

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
    private ILocator HubState => Page.Locator("#resident-hub-state");
    private ILocator BoardNote => Page.Locator("#resident-board-note");

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

        await Expect(RetryIndicator).ToBeVisibleAsync(new() { Timeout = 1000 });
        await WaitForTraceMessage("[alis:server-push] retry.manual", 10000);
        await Expect(RetryIndicator).ToBeVisibleAsync();

        await ClickWhenStable(RestoreButton);
        await ClickWhenStable(RetryIndicator);
        await Expect(RetryIndicator).ToBeHiddenAsync(new() { Timeout = 10000 });
        AssertNoConsoleErrorsExcept("connection.closed-permanent", "Failed to load resource");
    }

    [Test]
    public async Task losing_the_hub_connection_shows_the_retry_indicator_and_clicking_it_restores_live_updates()
    {
        await NavigateAndSeeLiveAlerts();

        await ClickWhenStable(BreakHubButton);
        await Expect(RetryIndicator).ToBeVisibleAsync(new() { Timeout = 90000 });
        await Expect(HubState).ToHaveTextAsync("Disconnected");
        await Expect(BoardNote).ToHaveTextAsync("Live updates stopped: reconnect from the banner at the top of the page.");

        await ClickWhenStable(RestoreHubButton);
        await ClickWhenStable(RetryIndicator);
        await Expect(RetryIndicator).ToBeHiddenAsync(new() { Timeout = 15000 });

        await ClickWhenStable(PushStatusButton);
        await Expect(ResidentName).ToContainTextAsync("Helen Martinez (Manual)",
            new() { Timeout = 15000 });
        await Expect(HubState).ToHaveTextAsync("Connected");
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
