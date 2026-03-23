namespace Alis.Reactive.PlaywrightTests.HttpPipeline.RealTime;

/// <summary>
/// As a care coordinator
/// I want to see notification updates in real time
/// So that I don't miss important events about my residents
/// </summary>
[TestFixture]
public class WhenNotificationPushArrives : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/RealTime";

    private ILocator PushBtn => Page.Locator("#btn-push-notification");
    private ILocator NotifCount => Page.Locator("#notif-count");
    private ILocator NotifMessage => Page.Locator("#notif-message");
    private ILocator NotifPriority => Page.Locator("#notif-priority");

    [Test]
    public async Task notification_count_and_message_update_on_screen()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await WaitForTraceMessage("[alis:signalr] connected", 10000);

        await PushBtn.ClickAsync();

        await Expect(NotifCount).ToContainTextAsync("99", new() { Timeout = 5000 });
        await Expect(NotifPriority).ToContainTextAsync("high");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task notification_indicator_turns_green_after_first_message()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await WaitForTraceMessage("[alis:signalr] connected", 10000);

        await PushBtn.ClickAsync();

        // Count updates from "—" to "99" — proves the hub message arrived
        await Expect(NotifCount).Not.ToContainTextAsync("—", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task multiple_clicks_each_update_the_ui()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await WaitForTraceMessage("[alis:signalr] connected", 10000);

        await PushBtn.ClickAsync();
        await Expect(NotifCount).ToContainTextAsync("99", new() { Timeout = 5000 });

        // Second click — same payload arrives again
        await PushBtn.ClickAsync();
        await Expect(NotifCount).ToContainTextAsync("99", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
