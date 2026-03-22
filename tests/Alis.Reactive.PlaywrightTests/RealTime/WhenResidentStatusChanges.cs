namespace Alis.Reactive.PlaywrightTests.RealTime;

/// <summary>
/// As a shift supervisor
/// I want to see resident status updates from a different hub
/// So that I know when care levels change across the facility
/// </summary>
[TestFixture]
public class WhenResidentStatusChanges : PlaywrightTestBase
{
    private const string Path = "/Sandbox/RealTime";

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
    public async Task two_hubs_operate_independently()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await WaitForTraceMessage("[alis:signalr] connected", 10000);

        // Push to Hub 2 only
        await PushStatusBtn.ClickAsync();

        await Expect(ResidentName).ToContainTextAsync("Helen Martinez",
            new() { Timeout = 5000 });

        // Hub 1 (notifications) still shows initial "—"
        var notifCount = Page.Locator("#notif-count");
        await Expect(notifCount).ToContainTextAsync("—");

        AssertNoConsoleErrors();
    }
}
