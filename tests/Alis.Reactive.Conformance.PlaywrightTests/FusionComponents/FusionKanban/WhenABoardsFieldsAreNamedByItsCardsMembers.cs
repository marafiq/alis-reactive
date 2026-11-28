using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenABoardsFieldsAreNamedByItsCardsMembers(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F26.M21_X1051_0)]
    public async Task each_card_stands_in_its_column_headed_by_the_names_its_members_carry()
    {
        var before = await NewWorld();
        await before.Page.GotoAsync(CarePages.StagedRound);

        await Expect(before.Page.GetByRole(AriaRole.Listbox, new PageGetByRoleOptions { Name = "Due", Exact = true }).GetByRole(AriaRole.Option, new LocatorGetByRoleOptions { Name = "Aspirin", Exact = true })).ToBeVisibleAsync();
        await Expect(before.Page.GetByRole(AriaRole.Listbox, new PageGetByRoleOptions { Name = "Given", Exact = true }).GetByRole(AriaRole.Option, new LocatorGetByRoleOptions { Name = "Paracetamol", Exact = true })).ToBeVisibleAsync();

        var after = await NewWorld();
        await after.Page.GotoAsync(CarePages.StagedRoundAfterTheRound);

        await Expect(after.Page.GetByRole(AriaRole.Listbox, new PageGetByRoleOptions { Name = "Given", Exact = true }).GetByRole(AriaRole.Option, new LocatorGetByRoleOptions { Name = "Aspirin", Exact = true })).ToBeVisibleAsync();
        await Expect(after.Page.GetByRole(AriaRole.Listbox, new PageGetByRoleOptions { Name = "Due", Exact = true }).GetByRole(AriaRole.Option, new LocatorGetByRoleOptions { Name = "Paracetamol", Exact = true })).ToBeVisibleAsync();
    }
}
