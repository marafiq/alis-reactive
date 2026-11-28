using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeAppLevelObjects;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTheRoundsStreamGivesUp(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M25_F7.M25_X11_0)]
    public async Task the_offer_shows_the_apps_words_beside_its_reconnect_button()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(CarePages.LiveOffer);
        await Expect(refused.Page.Locator("#board-call")).ToHaveTextAsync("1");
        await refused.Refuse(CareEndpoints.WardBoard);
        var offer = refused.Page.GetByRole(AriaRole.Alert);
        await Expect(offer.GetByText("Live updates stopped.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(offer.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Reconnect", Exact = true })).ToBeVisibleAsync();

        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.LiveOffer);
        await Expect(open.Page.Locator("#board-call")).ToHaveTextAsync("1");
        await Expect(open.Page.Locator("#alis-reconnect")).ToBeHiddenAsync();
    }
}
