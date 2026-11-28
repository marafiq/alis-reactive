using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Realtime;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTwoStreamsGiveUp(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M10_F1.X21_9_0)]
    public async Task one_offer_shows_stays_while_one_stream_is_given_up_and_hides_when_it_opens()
    {
        var both = await NewWorld();
        await both.Page.GotoAsync(CarePages.LiveOffer);
        await Expect(both.Page.Locator("#board-call")).ToHaveTextAsync("1");
        await both.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the bed card", Exact = true }).ClickAsync();
        await Expect(both.Page.Locator("#bed-line")).ToHaveTextAsync("live");
        var ward = await both.Refuse(CareEndpoints.WardBoard);
        var beds = await both.Refuse(CareEndpoints.BedBoard);
        await ward.Refused(1);
        await beds.Refused(1);
        await Expect(both.Page.GetByRole(AriaRole.Alert)).ToHaveCountAsync(1);
        await both.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the bed card", Exact = true }).ClickAsync();
        await Expect(both.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Bed 4", Exact = true })).ToHaveCountAsync(0);
        await Expect(both.Page.GetByRole(AriaRole.Alert)).ToBeVisibleAsync();
        await ward.Reopen();
        await both.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Reconnect", Exact = true }).ClickAsync();
        await Expect(both.Page.Locator("#board-call")).ToHaveTextAsync("2");
        await Expect(both.Page.Locator("#alis-reconnect")).ToBeHiddenAsync();

        var card = await NewWorld();
        await card.Page.GotoAsync(CarePages.LiveOffer);
        await Expect(card.Page.Locator("#board-call")).ToHaveTextAsync("1");
        await card.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the bed card", Exact = true }).ClickAsync();
        await Expect(card.Page.Locator("#bed-line")).ToHaveTextAsync("live");
        var cardBeds = await card.Refuse(CareEndpoints.BedBoard);
        await cardBeds.Refused(1);
        await Expect(card.Page.GetByRole(AriaRole.Alert)).ToBeVisibleAsync();
        await card.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the bed card", Exact = true }).ClickAsync();
        await Expect(card.Page.Locator("#alis-reconnect")).ToBeHiddenAsync();
    }
}
