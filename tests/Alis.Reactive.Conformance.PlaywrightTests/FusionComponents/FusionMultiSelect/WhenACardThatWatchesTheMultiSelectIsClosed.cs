using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACardThatWatchesTheMultiSelectIsClosed(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.OwnCells.lane_slot_unloadListener)]
    public async Task the_closed_cards_listener_is_gone()
    {
        var closed = await NewWorld();
        await closed.Page.GotoAsync(CarePages.AllergyWatch);
        await Expect(closed.Page.Locator("#card").GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("The card watches Helen's allergies.");
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close card", Exact = true }).ClickAsync();
        await Expect(closed.Page.Locator("#card")).ToBeEmptyAsync();
        await closed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();
        await closed.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Latex", Exact = true }).ClickAsync();
        await closed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Tab");

        await Expect(closed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByText("Latex", new LocatorGetByTextOptions { Exact = true }).Filter(new LocatorFilterOptions { Visible = true })).ToBeVisibleAsync();
        await Expect(closed.Page.Locator("#log")).ToHaveTextAsync("Nothing logged");

        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.AllergyWatch);
        await Expect(open.Page.Locator("#card").GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("The card watches Helen's allergies.");
        await open.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();
        await open.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Latex", Exact = true }).ClickAsync();
        await open.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Tab");

        await Expect(open.Page.Locator("#log")).ToHaveTextAsync("Card saw a change");
    }
}
