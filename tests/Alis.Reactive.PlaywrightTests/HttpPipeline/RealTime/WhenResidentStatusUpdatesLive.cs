namespace Alis.Reactive.PlaywrightTests.HttpPipeline.RealTime;

[TestFixture]
public class WhenResidentStatusUpdatesLive : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/RealTime";
    private const string PushResidentStatusPattern = "**/Sandbox/HttpPipeline/RealTime/PushResidentStatus";

    private ILocator PushStatusBtn => Page.Locator("#btn-push-status");
    private ILocator ResidentName => Page.Locator("#resident-name");
    private ILocator ResidentStatus => Page.Locator("#resident-status");
    private ILocator ResidentCareLevel => Page.Locator("#resident-care-level");

    private async Task WaitForResidentFeed()
    {
        await Expect(ResidentName).Not.ToContainTextAsync("—", new() { Timeout = 10000 });
    }

    private async Task PushResidentStatus()
    {
        var response = await Page.RunAndWaitForResponseAsync(
            async () => await ClickWhenStable(PushStatusBtn),
            PushResidentStatusPattern);

        Assert.That((int)response.Status, Is.EqualTo(200),
            "PushResidentStatus must return 200 OK.");
    }

    [Test]
    public async Task resident_details_update_from_second_hub()
    {
        await NavigateTo(Path);
        await WaitForPageReady(10000);
        await WaitForResidentFeed();

        await PushResidentStatus();

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
        await WaitForPageReady(10000);
        await WaitForResidentFeed();

        // Push to Hub 2 only
        await PushResidentStatus();

        await Expect(ResidentName).ToContainTextAsync("Helen Martinez",
            new() { Timeout = 5000 });

        // Hub 1 (notifications) still shows initial "—"
        var notifCount = Page.Locator("#notif-count");
        await Expect(notifCount).ToContainTextAsync("—");

        AssertNoConsoleErrors();
    }
}
