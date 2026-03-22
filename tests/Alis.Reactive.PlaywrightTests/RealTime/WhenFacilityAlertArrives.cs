using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.RealTime;

/// <summary>
/// As a facility manager
/// I want to see live facility alerts without refreshing
/// So that I can respond to issues immediately
/// </summary>
[TestFixture]
public class WhenFacilityAlertArrives : PlaywrightTestBase
{
    private const string Path = "/Sandbox/RealTime";

    private ILocator AlertMessage => Page.Locator("#alert-message");
    private ILocator AlertLevel => Page.Locator("#alert-level");
    private ILocator AlertStatus => Page.Locator("#alert-status");

    [Test]
    public async Task facility_alert_updates_the_alert_panel()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);

        await Expect(AlertMessage).ToContainTextAsync("Facility check complete",
            new() { Timeout = 10000 });
        await Expect(AlertLevel).ToContainTextAsync("info");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task alert_status_turns_green_when_connected()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);

        await Expect(AlertStatus).ToContainTextAsync("Connected",
            new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sse_connection_appears_in_trace()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await WaitForTraceMessage("[alis:server-push] connected", 10000);

        AssertTraceContains("server-push", "connected");
        AssertNoConsoleErrors();
    }
}
