using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACardThatListensToTheCarePanelIsClosed(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.X12_14_0)]
    public async Task the_closed_card_hears_no_more_raises_and_the_records_own_handler_still_does()
    {
        var closed = await NewWorld();
        await closed.Page.GotoAsync(CarePages.CarePanelCard);
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the card", Exact = true }).ClickAsync();
        await Expect(closed.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Pulse watch", Exact = true })).ToBeVisibleAsync();
        await closed.Page.Locator("#care-panel").GetByRole(AriaRole.Searchbox).FillAsync("72");
        await closed.Page.Locator("#care-panel").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(closed.Page.Locator("#page-count")).ToHaveTextAsync("72");
        await Expect(closed.Page.Locator("#card-count")).ToHaveTextAsync("72");
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the card", Exact = true }).ClickAsync();
        await Expect(closed.Page.Locator("#pulse-card")).ToBeEmptyAsync();
        await closed.Page.Locator("#care-panel").GetByRole(AriaRole.Searchbox).FillAsync("75");
        await closed.Page.Locator("#care-panel").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(closed.Page.Locator("#page-count")).ToHaveTextAsync("75");
        await Expect(closed.Page.Locator("#card-count")).ToHaveTextAsync("72");

        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.CarePanelCard);
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the card", Exact = true }).ClickAsync();
        await Expect(open.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Pulse watch", Exact = true })).ToBeVisibleAsync();
        await open.Page.Locator("#care-panel").GetByRole(AriaRole.Searchbox).FillAsync("72");
        await open.Page.Locator("#care-panel").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(open.Page.Locator("#card-count")).ToHaveTextAsync("72");
        await open.Page.Locator("#care-panel").GetByRole(AriaRole.Searchbox).FillAsync("75");
        await open.Page.Locator("#care-panel").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(open.Page.Locator("#page-count")).ToHaveTextAsync("75");
        await Expect(open.Page.Locator("#card-count")).ToHaveTextAsync("75");
    }
}
