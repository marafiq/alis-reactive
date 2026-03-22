namespace Alis.Reactive.PlaywrightTests.RealTime;

/// <summary>
/// As a care coordinator
/// I want to open a resident detail panel that also receives live updates
/// So that the panel stays current without me manually refreshing it
/// </summary>
[TestFixture]
public class WhenPartialConnectsToHub : PlaywrightTestBase
{
    private const string Path = "/Sandbox/RealTime";

    private ILocator LoadPanelBtn => Page.Locator("#load-panel");
    private ILocator PushStatusBtn => Page.Locator("#btn-push-status");
    private ILocator PanelResidentName => Page.Locator("#panel-resident-name");
    private ILocator PanelResidentStatus => Page.Locator("#panel-resident-status");

    [Test]
    public async Task partial_receives_hub_updates_after_loading()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await WaitForTraceMessage("connected", 10000);

        await LoadPanelBtn.ClickAsync();
        await Expect(PanelResidentName).ToBeVisibleAsync(new() { Timeout = 10000 });
        await WaitForTraceMessage("merge", 10000);

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
        await WaitForTraceMessage("connected", 10000);

        await LoadPanelBtn.ClickAsync();
        await Expect(PanelResidentName).ToBeVisibleAsync(new() { Timeout = 5000 });
        await WaitForTraceMessage("merge", 5000);

        await PushStatusBtn.ClickAsync();

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

    [Test]
    public async Task partial_reuses_existing_hub_connection()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await WaitForTraceMessage("connected", 10000);

        await LoadPanelBtn.ClickAsync();
        await WaitForTraceMessage("merge", 5000);

        var connectTraces = _consoleMessages
            .Where(m => m.Contains("[alis:signalr]") && m.Contains("connected")
                        && m.Contains("resident-status"))
            .ToList();

        Assert.That(connectTraces, Has.Count.EqualTo(1),
            "Expected partial to reuse parent's hub connection, not create a second one");

        AssertNoConsoleErrors();
    }
}
