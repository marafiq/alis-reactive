namespace Alis.Reactive.PlaywrightTests.HttpPipeline.RealTime;

[TestFixture]
public class WhenHubConnectionEstablishes : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/RealTime";
    private const string PushResidentStatusPattern = "**/Sandbox/HttpPipeline/RealTime/PushResidentStatus";

    private ILocator LoadPanelBtn => Page.Locator("#load-panel");
    private ILocator PushStatusBtn => Page.Locator("#btn-push-status");
    private ILocator PanelResidentName => Page.Locator("#panel-resident-name");
    private ILocator PanelResidentStatus => Page.Locator("#panel-resident-status");

    private async Task WaitForResidentFeed()
    {
        await Expect(Page.Locator("#resident-name"))
            .Not.ToContainTextAsync("—", new() { Timeout = 10000 });
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
    public async Task partial_receives_hub_updates_after_loading()
    {
        await NavigateTo(Path);
        await WaitForPageReady(10000);
        await WaitForResidentFeed();

        await LoadPanelBtn.ClickAsync();
        await Expect(PanelResidentName).ToBeVisibleAsync(new() { Timeout = 10000 });

        await PushResidentStatus();

        await Expect(PanelResidentName).ToContainTextAsync("Helen Martinez",
            new() { Timeout = 10000 });
        await Expect(PanelResidentStatus).ToContainTextAsync("Under Review");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task partial_and_parent_both_receive_same_hub_message()
    {
        await NavigateTo(Path);
        await WaitForPageReady(10000);
        await WaitForResidentFeed();

        await LoadPanelBtn.ClickAsync();
        await Expect(PanelResidentName).ToBeVisibleAsync(new() { Timeout = 5000 });

        await PushResidentStatus();

        // Parent DOM updates
        var parentName = Page.Locator("#resident-name");
        await Expect(parentName).ToContainTextAsync("Helen Martinez",
            new() { Timeout = 5000 });

        // Partial DOM also updates (same hub, reused connection)
        await Expect(PanelResidentName).ToContainTextAsync("Helen Martinez",
            new() { Timeout = 5000 });
        await Expect(PanelResidentStatus).ToContainTextAsync("Under Review");

        AssertNoConsoleErrors();
    }
}
