using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACascadeLoadsAPartialIntoAContainer(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X6_0)]
    public async Task the_partials_room_is_checked_and_sent_with_the_containers_own_inputs()
    {
        var chosen = await NewWorld();
        await chosen.Page.GotoAsync(CarePages.ArrivalDesk);
        await chosen.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Bea Hart");
        await chosen.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Ward", Exact = true }).SelectOptionAsync("East");
        await chosen.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Room", Exact = true }).SelectOptionAsync("15");
        await chosen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Record the arrival", Exact = true }).ClickAsync();
        await Expect(chosen.Page.Locator("#arrived-room")).ToHaveTextAsync("15");

        var unchosen = await NewWorld();
        await unchosen.Page.GotoAsync(CarePages.ArrivalDesk);
        await unchosen.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Bea Hart");
        await unchosen.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Ward", Exact = true }).SelectOptionAsync("East");
        await Expect(unchosen.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Room", Exact = true })).ToBeVisibleAsync();
        await unchosen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Record the arrival", Exact = true }).ClickAsync();
        await Expect(unchosen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Room", Exact = true })
            .GetByText("Choose a room.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(unchosen.Page.Locator("#arrived-room")).ToHaveTextAsync("none yet");
    }
}
