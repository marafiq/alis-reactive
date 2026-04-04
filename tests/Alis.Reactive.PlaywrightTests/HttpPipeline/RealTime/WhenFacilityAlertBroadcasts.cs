using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.HttpPipeline.RealTime;

[TestFixture]
public class WhenFacilityAlertBroadcasts : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/RealTime";

    private ILocator AlertMessage => Page.Locator("#alert-message");
    private ILocator AlertLevel => Page.Locator("#alert-level");
    private ILocator AlertStatus => Page.Locator("#alert-status");

    [Test]
    public async Task facility_alert_updates_the_alert_panel()
    {
        await NavigateTo(Path);
        await WaitForPageReady(10000);

        await Expect(AlertMessage).ToContainTextAsync("Facility check complete",
            new() { Timeout = 10000 });
        await Expect(AlertLevel).ToContainTextAsync("info");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task alert_status_turns_green_when_connected()
    {
        await NavigateTo(Path);
        await WaitForPageReady(10000);

        await Expect(AlertStatus).ToContainTextAsync("Connected",
            new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
