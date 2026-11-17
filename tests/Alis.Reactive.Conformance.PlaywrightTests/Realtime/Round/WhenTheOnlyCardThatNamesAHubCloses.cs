using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Realtime;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTheOnlyCardThatNamesAHubCloses(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M10_F2.X12_6_0)]
    public async Task the_offer_of_the_hub_it_gave_up_on_hides()
    {
        var card = await NewWorld();
        await card.InstallTheClock();
        await card.HoldTheClock();
        await card.Page.GotoAsync(CarePages.LiveBedLine);
        await card.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the bed card", Exact = true }).ClickAsync();
        await Expect(card.Page.Locator("#bed-line-call")).ToHaveTextAsync("1");
        await card.Refuse(CareEndpoints.BedLine);
        await card.RunTheClockFor(TimeSpan.FromSeconds(45), CareEndpoints.BedLine);
        await Expect(card.Page.GetByRole(AriaRole.Alert)).ToBeVisibleAsync();
        await card.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the bed card", Exact = true }).ClickAsync();
        await Expect(card.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Bed 4", Exact = true })).ToHaveCountAsync(0);
        await Expect(card.Page.Locator("#alis-reconnect")).ToBeHiddenAsync();

        var page = await NewWorld();
        await page.InstallTheClock();
        await page.HoldTheClock();
        await page.Page.GotoAsync(CarePages.LiveBedLineHeldByThePage);
        await Expect(page.Page.Locator("#page-line")).ToHaveTextAsync("1");
        await page.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the bed card", Exact = true }).ClickAsync();
        await Expect(page.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Bed 4", Exact = true })).ToBeVisibleAsync();
        await page.Refuse(CareEndpoints.BedLine);
        await page.RunTheClockFor(TimeSpan.FromSeconds(45), CareEndpoints.BedLine);
        await Expect(page.Page.GetByRole(AriaRole.Alert)).ToBeVisibleAsync();
        await page.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the bed card", Exact = true }).ClickAsync();
        await Expect(page.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Bed 4", Exact = true })).ToHaveCountAsync(0);
        await Expect(page.Page.GetByRole(AriaRole.Alert)).ToBeVisibleAsync();
    }
}
