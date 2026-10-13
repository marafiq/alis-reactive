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
        await PushStatusBtn.ClickAsync();
        await Expect(ResidentName).ToContainTextAsync("Helen Martinez",
            new() { Timeout = 5000 });

        await Page.Locator("#btn-clear-board").ClickAsync();

        await Expect(Page.Locator("#resident-board-note"))
            .ToHaveTextAsync("The board was cleared at shift change.", new() { Timeout = 5000 });
        await Expect(ResidentName).ToHaveTextAsync("—");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task two_hubs_operate_independently()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await WaitForTraceMessage("[alis:signalr] connected", 10000);

        await PushStatusBtn.ClickAsync();

        await Expect(ResidentName).ToContainTextAsync("Helen Martinez",
            new() { Timeout = 5000 });

        var notifCount = Page.Locator("#notif-count");
        await Expect(notifCount).ToContainTextAsync("—");

        AssertNoConsoleErrors();
    }
}
