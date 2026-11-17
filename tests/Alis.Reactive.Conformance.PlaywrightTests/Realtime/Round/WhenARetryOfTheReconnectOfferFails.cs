using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Realtime;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARetryOfTheReconnectOfferFails(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M10_F1.X21_10_0)]
    public async Task the_offer_stays_shown_until_a_retry_opens_the_stream()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(CarePages.LiveOffer);
        await Expect(refused.Page.Locator("#board-call")).ToHaveTextAsync("1");
        var ward = await refused.Refuse(CareEndpoints.WardBoard);
        await Expect(refused.Page.GetByRole(AriaRole.Alert)).ToBeVisibleAsync();
        await refused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Reconnect", Exact = true }).ClickAsync();
        await ward.Refused(2);
        await Expect(refused.Page.GetByRole(AriaRole.Alert)).ToBeVisibleAsync();
        await ward.Reopen();
        await refused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Reconnect", Exact = true }).ClickAsync();
        await Expect(refused.Page.Locator("#board-call")).ToHaveTextAsync("2");
        await Expect(refused.Page.Locator("#alis-reconnect")).ToBeHiddenAsync();

        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.LiveOffer);
        await Expect(open.Page.Locator("#board-call")).ToHaveTextAsync("1");
        await Expect(open.Page.Locator("#alis-reconnect")).ToBeHiddenAsync();
    }
}
