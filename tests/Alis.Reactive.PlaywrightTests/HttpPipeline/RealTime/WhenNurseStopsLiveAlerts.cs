namespace Alis.Reactive.PlaywrightTests.HttpPipeline.RealTime;

[TestFixture]
public class WhenNurseStopsLiveAlerts : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/RealTime";

    private ILocator AlertStatus => Page.Locator("#alert-status");
    private ILocator AlertMessage => Page.Locator("#alert-message");
    private ILocator AlertLevel => Page.Locator("#alert-level");
    private ILocator RetryIndicator => Page.Locator("#alis-realtime-connection-retry-container");
    private ILocator StopAlertsButton => Page.Locator("#btn-stop-alerts");
    private ILocator AllClearButton => Page.Locator("#btn-all-clear");
    private ILocator PushNotificationButton => Page.Locator("#btn-push-notification");
    private ILocator NotificationCount => Page.Locator("#notif-count");

    [Test]
    public async Task stopping_live_alerts_says_so_and_raises_no_reconnect_banner()
    {
        await NavigateAndSeeLiveAlerts();

        await ClickWhenStable(StopAlertsButton);

        await Expect(AlertStatus).ToHaveTextAsync("Stopped", new() { Timeout = 10000 });
        await Expect(RetryIndicator).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_all_clear_sounded_after_alerts_are_stopped_never_reaches_the_panel()
    {
        await NavigateAndSeeLiveAlerts();
        await WaitForTraceMessage("connected {\"hubUrl\":\"/hubs/notifications\"}", 10000);
        await ClickWhenStable(StopAlertsButton);
        await Expect(AlertStatus).ToHaveTextAsync("Stopped", new() { Timeout = 10000 });

        await Page.RunAndWaitForResponseAsync(
            async () => await ClickWhenStable(AllClearButton),
            "**/RealTime/SoundAllClear");
        await ClickWhenStable(PushNotificationButton);
        await Expect(NotificationCount).ToHaveTextAsync("99", new() { Timeout = 10000 });

        await Expect(AlertLevel).ToHaveTextAsync("info");
        await Expect(AlertMessage).ToHaveTextAsync("Facility check complete");
        await Expect(AlertStatus).ToHaveTextAsync("Stopped");
        AssertNoConsoleErrors();
    }

    private async Task NavigateAndSeeLiveAlerts()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(AlertStatus).ToHaveTextAsync("Connected", new() { Timeout = 10000 });
    }
}
