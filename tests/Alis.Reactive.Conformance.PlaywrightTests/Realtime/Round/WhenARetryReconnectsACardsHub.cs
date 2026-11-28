using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Realtime;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARetryReconnectsACardsHub(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M10_F2.X12_6_1)]
    public async Task the_cards_hub_welcomes_it_again_and_the_offer_hides()
    {
        var refused = await NewWorld();
        await refused.InstallTheClock();
        await refused.HoldTheClock();
        await refused.Page.GotoAsync(CarePages.LiveLine);
        await refused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the card", Exact = true }).ClickAsync();
        await Expect(refused.Page.Locator("#line-call")).ToHaveTextAsync("1");
        var line = await refused.Refuse(CareEndpoints.Line);
        await refused.RunTheClockFor(TimeSpan.FromSeconds(45), CareEndpoints.Line);
        await Expect(refused.Page.GetByRole(AriaRole.Alert)).ToBeVisibleAsync();
        await line.Reopen();
        await refused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Reconnect", Exact = true }).ClickAsync();
        await Expect(refused.Page.Locator("#line-call")).ToHaveTextAsync("2");
        await Expect(refused.Page.Locator("#alis-reconnect")).ToBeHiddenAsync();
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Bed 7", Exact = true })).ToBeVisibleAsync();

        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.LiveLine);
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the card", Exact = true }).ClickAsync();
        await Expect(open.Page.Locator("#line-call")).ToHaveTextAsync("1");
        await Expect(open.Page.Locator("#alis-reconnect")).ToBeHiddenAsync();
    }
}
