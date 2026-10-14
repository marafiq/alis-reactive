namespace Alis.Reactive.PlaywrightTests.HttpPipeline.RealTime;

[TestFixture]
public class WhenResidentStatusUpdatesLive : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/RealTime";

    private ILocator PushStatusBtn => Page.Locator("#btn-push-status");
    private ILocator ResidentName => Page.Locator("#resident-name");
    private ILocator ResidentStatus => Page.Locator("#resident-status");
    private ILocator ResidentCareLevel => Page.Locator("#resident-care-level");

    [Test]
    public async Task resident_details_update_from_second_hub()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await WaitForTraceMessage("[alis:signalr] connected", 10000);

        await PushStatusBtn.ClickAsync();

        await Expect(ResidentName).ToContainTextAsync("Helen Martinez",
            new() { Timeout = 5000 });
        await Expect(ResidentStatus).ToContainTextAsync("Under Review");
        await Expect(ResidentCareLevel).ToContainTextAsync("Memory Care");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_the_board_at_shift_change_empties_the_resident_card()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await WaitForTraceMessage("connected {\"hubUrl\":\"/hubs/resident-status", 10000);
        await PushStatusBtn.ClickAsync();
        await Expect(ResidentName).ToHaveTextAsync("Helen Martinez (Manual)",
            new() { Timeout = 5000 });

        await Page.Locator("#btn-clear-board").ClickAsync();

        await Expect(Page.Locator("#resident-board-note"))
            .ToHaveTextAsync("The board was cleared at shift change.", new() { Timeout = 5000 });
        await Expect(ResidentName).ToHaveTextAsync("—");

        AssertNoConsoleErrors();
    }

    // Each hub has a connection of its own: while this page's resident hub connection is down
    // (aborted, and its reconnects refused), the notification hub still delivers.
    [Test]
    public async Task notifications_keep_arriving_while_the_resident_hub_is_down()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await WaitForTraceMessage("connected {\"hubUrl\":\"/hubs/notifications\"}", 10000);
        await WaitForTraceMessage("connected {\"hubUrl\":\"/hubs/resident-status", 10000);

        // Once the server has answered Break, it has dropped this page's resident hub connection.
        await Page.RunAndWaitForResponseAsync(
            async () => await ClickWhenStable(Page.Locator("#btn-break-hub")),
            "**/RealTime/BreakResidentHub");
        await ClickWhenStable(Page.Locator("#btn-push-notification"));

        await Expect(Page.Locator("#notif-count")).ToHaveTextAsync("99", new() { Timeout = 5000 });
        await Expect(Page.Locator("#notif-priority")).ToHaveTextAsync("high");
        AssertNoConsoleErrorsExcept("connection.disconnected", "start.failed", "WebSocket", "Failed to load resource");
    }
}
