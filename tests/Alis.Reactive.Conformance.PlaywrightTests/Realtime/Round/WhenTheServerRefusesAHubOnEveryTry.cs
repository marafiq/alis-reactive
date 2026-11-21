using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Realtime;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTheServerRefusesAHubOnEveryTry(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M10_F2.M10_X1_0)]
    public async Task the_hub_runs_its_closed_steps_and_no_reconnecting_steps_and_the_reconnect_offer_shows()
    {
        var refused = await NewWorld();
        await refused.InstallTheClock();
        await refused.HoldTheClock();
        var line = await refused.Refuse(CareEndpoints.WardLine);
        await refused.Page.GotoAsync(CarePages.LiveWardLine);
        await line.Refused(1);
        await refused.RunTheClockFor(TimeSpan.FromSeconds(45), CareEndpoints.WardLine);
        await Expect(refused.Page.GetByRole(AriaRole.Alert)).ToBeVisibleAsync();
        await Expect(refused.Page.Locator("#line-closed")).ToHaveTextAsync("closed");
        await Expect(refused.Page.Locator("#line-dropped")).ToBeEmptyAsync();

        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.LiveWardLine);
        await Expect(open.Page.Locator("#line-call")).ToHaveTextAsync("1");
        await Expect(open.Page.Locator("#line-closed")).ToBeEmptyAsync();
        await Expect(open.Page.Locator("#alis-reconnect")).ToBeHiddenAsync();
    }
}
