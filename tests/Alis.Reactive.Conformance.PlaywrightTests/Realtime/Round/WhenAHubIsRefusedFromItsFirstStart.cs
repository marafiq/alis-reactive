using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Realtime;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHubIsRefusedFromItsFirstStart(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M10_F2.X21_13_0)]
    public async Task it_tries_again_after_0_2_10_and_30_seconds_then_runs_its_closed_steps()
    {
        var refused = await NewWorld();
        await refused.InstallTheClock();
        await refused.HoldTheClock();
        var line = await refused.Refuse(CareEndpoints.NurseLine);
        await refused.Page.GotoAsync(CarePages.LiveNurseLine);
        await line.Refused(1);
        await refused.RunTheClockFor(TimeSpan.FromSeconds(41), CareEndpoints.NurseLine);
        await line.Refused(4);
        await Expect(refused.Page.Locator("#line-state")).ToBeEmptyAsync();
        await Expect(refused.Page.Locator("#alis-reconnect")).ToBeHiddenAsync();
        await refused.RunTheClockFor(TimeSpan.FromSeconds(2), CareEndpoints.NurseLine);
        await line.Refused(5);
        await Expect(refused.Page.Locator("#line-state")).ToHaveTextAsync("closed");
        await Expect(refused.Page.GetByRole(AriaRole.Alert)).ToBeVisibleAsync();

        var reopened = await NewWorld();
        await reopened.InstallTheClock();
        await reopened.HoldTheClock();
        var thirdTry = await reopened.Refuse(CareEndpoints.NurseLine);
        await reopened.Page.GotoAsync(CarePages.LiveNurseLine);
        await thirdTry.Refused(1);
        await reopened.RunTheClockFor(TimeSpan.FromSeconds(3), CareEndpoints.NurseLine);
        await thirdTry.Refused(3);
        await thirdTry.Reopen();
        await reopened.RunTheClockFor(TimeSpan.FromSeconds(10), CareEndpoints.NurseLine);
        await Expect(reopened.Page.Locator("#line-state")).ToHaveTextAsync("open");
        await Expect(reopened.Page.Locator("#alis-reconnect")).ToBeHiddenAsync();
    }
}
