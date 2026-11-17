using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Realtime;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARetryOpensAHubTheServerHungUpOn(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M10_F2.M10_X2_0)]
    public async Task the_hub_runs_its_reconnected_steps_and_the_offer_hides()
    {
        var hungUp = await NewWorld();
        await hungUp.InstallTheClock();
        await hungUp.HoldTheClock();
        await hungUp.Page.GotoAsync(CarePages.LiveWardLine);
        await Expect(hungUp.Page.Locator("#line-call")).ToHaveTextAsync("1");
        await Expect(hungUp.Page.Locator("#line-back")).ToBeEmptyAsync();
        var line = await hungUp.Refuse(CareEndpoints.WardLine);
        await hungUp.RunTheClockFor(TimeSpan.FromSeconds(45), CareEndpoints.WardLine);
        await Expect(hungUp.Page.GetByRole(AriaRole.Alert)).ToBeVisibleAsync();
        await line.Reopen();
        await hungUp.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Reconnect", Exact = true }).ClickAsync();
        await Expect(hungUp.Page.Locator("#line-call")).ToHaveTextAsync("2");
        await Expect(hungUp.Page.Locator("#line-back")).ToHaveTextAsync("back");
        await Expect(hungUp.Page.Locator("#alis-reconnect")).ToBeHiddenAsync();
    }
}
