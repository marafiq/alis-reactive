using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACardsMultiSelectIsDestroyedByItsHost(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.OwnCells.lane_slot_hostDestroyed)]
    public async Task closing_the_card_calls_nothing_on_the_destroyed_box_and_opening_it_shows_a_new_one()
    {
        var helen = await NewWorld();
        await helen.Page.GotoAsync(CarePages.AllergyRemoval);
        await Expect(helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByText("Penicillin", new LocatorGetByTextOptions { Exact = true }).Filter(new LocatorFilterOptions { Visible = true })).ToBeVisibleAsync();
        await helen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Remove box", Exact = true }).ClickAsync();
        await helen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close card", Exact = true }).ClickAsync();

        await Expect(helen.Page.Locator("#card")).ToBeEmptyAsync();
        Assert.That(helen.ConsoleErrors, Is.Empty);

        await helen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open card", Exact = true }).ClickAsync();
        await Expect(helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByText("Penicillin", new LocatorGetByTextOptions { Exact = true }).Filter(new LocatorFilterOptions { Visible = true })).ToBeVisibleAsync();
        await helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();
        await helen.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Latex", Exact = true }).ClickAsync();
        await helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Tab");
        await Expect(helen.Page.Locator("#log")).ToHaveTextAsync("Card saw a change");
        Assert.That(helen.ConsoleErrors, Is.Empty);
    }
}
