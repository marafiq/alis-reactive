namespace Alis.Reactive.PlaywrightTests.HttpPipeline.RealTime;

[TestFixture]
public class WhenHubConnectionEstablishes : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/RealTime";

    private ILocator LoadPanelBtn => Page.Locator("#load-panel");
    private ILocator PushStatusBtn => Page.Locator("#btn-push-status");
    private ILocator PanelResidentName => Page.Locator("#panel-resident-name");
    private ILocator PanelResidentStatus => Page.Locator("#panel-resident-status");
    private ILocator PanelConnections => Page.Locator("#panel-connections");
    private ILocator PanelSummary => Page.Locator("#panel-summary");
    private ILocator ClosePanelBtn => Page.Locator("#close-panel");
    private ILocator ResidentName => Page.Locator("#resident-name");

    private async Task NavigateAndConnect()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await WaitForTraceMessage("connected {\"hubUrl\":\"/hubs/resident-status", 10000);
    }

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

    // The server counts this page's live connections to the resident hub when the status is pushed:
    // the panel's trigger rides the page's connection instead of opening a second one.
    [Test]
    public async Task the_resident_panel_shares_the_pages_live_connection()
    {
        await NavigateAndConnect();

        await LoadPanelBtn.ClickAsync();
        await Expect(PanelSummary).ToHaveTextAsync("Resident panel open.", new() { Timeout = 10000 });
        await PushStatusBtn.ClickAsync();

        await Expect(PanelResidentName).ToContainTextAsync("Helen Martinez", new() { Timeout = 10000 });
        await Expect(PanelConnections).ToHaveTextAsync("1");
        AssertNoConsoleErrors();
    }

    // Closing the panel unloads its slot, and with it the panel's live update handler; the page's own
    // handler, called on the same message first, shows the push arrived.
    [Test]
    public async Task a_closed_resident_panel_takes_no_more_live_updates()
    {
        await NavigateAndConnect();
        await LoadPanelBtn.ClickAsync();
        await Expect(PanelSummary).ToHaveTextAsync("Resident panel open.", new() { Timeout = 10000 });

        await ClosePanelBtn.ClickAsync();
        await Expect(PanelSummary).ToHaveTextAsync("Resident panel closed.", new() { Timeout = 10000 });
        await Expect(PanelResidentName).ToHaveCountAsync(0);
        await PushStatusBtn.ClickAsync();

        await Expect(ResidentName).ToContainTextAsync("Helen Martinez (Manual)", new() { Timeout = 10000 });
        await Expect(PanelSummary).ToHaveTextAsync("Resident panel closed.");
        AssertNoConsoleErrors();
    }
}
