using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAKanbanIsRenderedWithTheViewsOptions(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F26.M21_X1017_0)]
    public async Task the_board_draws_the_views_columns_and_cards_and_a_clicked_card_reaches_the_views_handler()
    {
        var due = await NewWorld();
        await due.Page.GotoAsync(CarePages.DoseBoard);
        await Expect(due.Page.GetByRole(AriaRole.Listbox, new PageGetByRoleOptions { Name = "Due", Exact = true }).GetByRole(AriaRole.Option, new LocatorGetByRoleOptions { Name = "Aspirin", Exact = true })).ToBeVisibleAsync();
        await Expect(due.Page.GetByRole(AriaRole.Listbox, new PageGetByRoleOptions { Name = "Given", Exact = true }).GetByRole(AriaRole.Option, new LocatorGetByRoleOptions { Name = "Paracetamol", Exact = true })).ToBeVisibleAsync();
        await due.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Aspirin", Exact = true }).ClickAsync();

        await Expect(due.Page.Locator("#dose-picked")).ToHaveTextAsync("Aspirin");

        var given = await NewWorld();
        await given.Page.GotoAsync(CarePages.DoseBoard);
        await given.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Paracetamol", Exact = true }).ClickAsync();

        await Expect(given.Page.Locator("#dose-picked")).ToHaveTextAsync("Paracetamol");
    }
}
