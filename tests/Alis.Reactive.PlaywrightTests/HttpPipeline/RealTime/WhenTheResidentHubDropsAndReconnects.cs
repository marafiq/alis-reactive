using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.HttpPipeline.RealTime;

// Story: As a charge nurse watching the resident board, I want the board to say whether its live
// link is up, being restored, or back, so that I know whether what I see is current.
//
// The link line reads the hub's own state and connection id: with each status that lands, when the
// connection drops (Reconnecting) and when it is back (Reconnected, under the new id the event
// carries). The drill drops this page's transport and refuses its negotiates until Restore, so the
// client's automatic reconnect is what brings the link back.
[TestFixture]
public class WhenTheResidentHubDropsAndReconnects : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/RealTime";
    private static readonly Regex AConnectionId = new("^[A-Za-z0-9_-]{20,}$");

    private ILocator HubState => Page.Locator("#resident-hub-state");
    private ILocator ConnectionId => Page.Locator("#resident-connection");
    private ILocator BoardNote => Page.Locator("#resident-board-note");
    private ILocator ResidentName => Page.Locator("#resident-name");
    private ILocator PushStatusButton => Page.Locator("#btn-push-status");
    private ILocator BreakHubButton => Page.Locator("#btn-break-hub");
    private ILocator RestoreHubButton => Page.Locator("#btn-restore-hub");

    [Test]
    public async Task a_status_landing_shows_the_link_connected_under_its_connection_id()
    {
        await NavigateAndConnect();
        await Expect(ConnectionId).ToHaveTextAsync("—");

        await ClickWhenStable(PushStatusButton);

        await Expect(ResidentName).ToHaveTextAsync("Helen Martinez (Manual)", new() { Timeout = 10000 });
        await Expect(HubState).ToHaveTextAsync("Connected");
        await Expect(ConnectionId).ToHaveTextAsync(AConnectionId);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_dropped_link_says_reconnecting_until_restored_then_connected_under_a_new_id()
    {
        await NavigateAndConnect();
        await ClickWhenStable(PushStatusButton);
        await Expect(ConnectionId).ToHaveTextAsync(AConnectionId, new() { Timeout = 10000 });
        var idBeforeTheDrop = await ConnectionId.TextContentAsync();

        await ClickWhenStable(BreakHubButton);

        await Expect(HubState).ToHaveTextAsync("Reconnecting", new() { Timeout = 15000 });
        await Expect(BoardNote).ToHaveTextAsync("Live updates paused while the connection is restored.");
        await Expect(ConnectionId).ToHaveTextAsync(idBeforeTheDrop!);

        await ClickWhenStable(RestoreHubButton);

        // The client's next automatic attempt (2 s, then 10 s after the drop) is the one that succeeds.
        await Expect(HubState).ToHaveTextAsync("Connected", new() { Timeout = 45000 });
        await Expect(BoardNote).ToHaveTextAsync("Live updates resumed.");
        await Expect(ConnectionId).ToHaveTextAsync(AConnectionId);
        await Expect(ConnectionId).Not.ToHaveTextAsync(idBeforeTheDrop!);
        AssertNoConsoleErrorsExcept("WebSocket", "Failed to load resource");
    }

    private async Task NavigateAndConnect()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await WaitForTraceMessage("connected {\"hubUrl\":\"/hubs/resident-status", 10000);
    }
}
