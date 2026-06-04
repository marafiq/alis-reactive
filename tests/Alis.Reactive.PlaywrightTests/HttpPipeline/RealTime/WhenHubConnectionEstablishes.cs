namespace Alis.Reactive.PlaywrightTests.HttpPipeline.RealTime;

[TestFixture]
public class WhenHubConnectionEstablishes : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/RealTime";

    private ILocator LoadPanelBtn => Page.Locator("#load-panel");
    private ILocator PushStatusBtn => Page.Locator("#btn-push-status");
    private ILocator PanelResidentName => Page.Locator("#panel-resident-name");
    private ILocator PanelResidentStatus => Page.Locator("#panel-resident-status");

    [Test]
    public async Task partial_receives_hub_updates_after_loading()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await WaitForTraceMessage("[alis:signalr] connected", 10000);

        await LoadPanelBtn.ClickAsync();
        await Expect(PanelResidentName).ToBeVisibleAsync(new() { Timeout = 10000 });

        await PushStatusBtn.ClickAsync();

        await Expect(PanelResidentName).ToContainTextAsync("Helen Martinez",
            new() { Timeout = 10000 });
        await Expect(PanelResidentStatus).ToContainTextAsync("Under Review");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task partial_and_parent_both_receive_same_hub_message()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await WaitForTraceMessage("[alis:signalr] connected", 10000);

        await LoadPanelBtn.ClickAsync();
        await Expect(PanelResidentName).ToBeVisibleAsync(new() { Timeout = 5000 });

        await PushStatusBtn.ClickAsync();

        var parentName = Page.Locator("#resident-name");
        await Expect(parentName).ToContainTextAsync("Helen Martinez",
            new() { Timeout = 5000 });

        await Expect(PanelResidentName).ToContainTextAsync("Helen Martinez",
            new() { Timeout = 5000 });
        await Expect(PanelResidentStatus).ToContainTextAsync("Under Review");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task partial_reuses_existing_hub_connection()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await WaitForTraceMessage("[alis:signalr] connected", 10000);

        await LoadPanelBtn.ClickAsync();
        await Expect(PanelResidentName).ToBeVisibleAsync(new() { Timeout = 5000 });

        var residentStatusConnectTraces = _consoleMessages
            .Where(m => m.Contains("[alis:signalr] connected")
                        && m.Contains("resident-status"))
            .ToList();

        Assert.That(residentStatusConnectTraces, Has.Count.EqualTo(1),
            "Expected partial to reuse parent's hub connection, not create a second one");

        AssertNoConsoleErrors();
    }
}
